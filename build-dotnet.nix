{ dotnet-sdk, stdenv, makeWrapper }:
{ baseName
  , version
  , src
  , additionalWrapperArgs ? ""
  , nugetsFile ? ./nugets.json }:
let fetchurl = import <nix/fetchurl.nix>;
    fetchNuPkg = 
      { url , name , sha512, ... }:
      fetchurl {
          inherit sha512 url;
          name = "${name}.${version}.nupkg";
    };
    nugetInfos = builtins.fromJSON (builtins.readFile nugetsFile );
    nugets = map (n: fetchNuPkg n) nugetInfos;
in
stdenv.mkDerivation rec {
  name = "${baseName}-${version}";
  nativeBuildInputs =  [ dotnet-sdk makeWrapper ];
  inherit src;
  nugetsWithFileName = map (n: "${n}:${n.name}") nugets;
  buildPhase = ''
    runHook preBuild

    export DOTNET_CLI_TELEMETRY_OPTOUT=true
    export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
    # avoid permission denied error
    export HOME=$PWD

    mkdir -p packages
    for n in $nugetsWithFileName; do
        ln -s `echo $n|cut -f1 -d:` packages/`echo $n | cut -f2 -d:`
    done

    echo "Running dotnet restore"
    dotnet restore --source $PWD/packages
    echo "Running dotnet build"
    dotnet build --no-restore

    runHook postBuild
  '';
  dontStrip = true;

  installPhase = ''
    runHook preInstall

    echo Running dotnet publish
    dotnet publish --no-restore --no-build -o $out

    echo Creating wrapper
    mkdir $out/bin
    makeWrapper ${dotnet-sdk}/bin/dotnet $out/bin/${baseName} --add-flags $out/${baseName}.dll ${additionalWrapperArgs}
    chmod +x $out/bin/${baseName}
    runHook postInstall
  '';
}

{ fetchurl, dotnet-sdk, stdenv }:
{
  baseName
  , version
  , src
  , nugetsFile ? ./nugets.json
  , feedUrlsFile ? ./urls.json
  , defaultFeedUrl ? "https://www.nuget.org/api/v2"
  , netrc-file ? null
}:
let
    myFetchurl = if netrc-file == null
                 then fetchurl
                 else args: fetchurl (args // {
                      curlOpts = "--netrc-file ${netrc-file}";
                    });
    feedUrls = if builtins.pathExists feedUrlsFile
               then builtins.fromJSON (builtins.readFile feedUrlsFile )
               else [ defaultFeedUrl ];
    getUrls = baseName: version: urls:
              if urls != null
              then urls
              else map (base: "${base}/package/${baseName}/${version}" ) feedUrls;
    fetchNuPkg = ({
      baseName
      , version
      , urls ? null
      , sha512
      }: myFetchurl {
          inherit sha512;
          urls = getUrls baseName version urls;
          name = "${baseName}.${version}.nupkg";
    });
    nugetInfos = builtins.fromJSON (builtins.readFile nugetsFile );
    nugetsList = map (n: fetchNuPkg n) nugetInfos;
in
stdenv.mkDerivation rec {
  name = "${baseName}-${version}";
  buildInputs =  [ dotnet-sdk ];
  nugets = map (n: "${n}:${n.name}") nugetsList;
  src = ./. ;
  buildPhase = ''
    runHook preBuild

    export DOTNET_CLI_TELEMETRY_OPTOUT=true
    export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
    # avoid permission denied error
    export HOME=$PWD

    mkdir packages
    cd packages
    for p in $nugets; do
      pstore=$(echo $p|cut -f1 -d:)
      plink=$(echo $p|cut -f2 -d:)
      ln -s $pstore $plink
    done
    cd ..

    echo Running dotnet restore
    dotnet restore --source $PWD/packages
    echo Running dotnet build
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
    cat << EOF >> $out/bin/${baseName}
    #!/bin/sh
    ${dotnet-sdk}/bin/dotnet $out/${baseName}.dll -- "$@"
    EOF
    chmod +x $out/bin/${baseName}
    runHook postInstall
  '';

}

{ stdenv, libunwind, libuuid, icu, openssl, zlib, curl, makeWrapper, callPackage, linkFarm, runCommand, fetchurl }:
{ pname
  , dotnetSdkPackage
  , version
  , src
  , additionalWrapperArgs ? ""
  , mono ? ""
  , project ? ""
  , configuration ? "Release"
  , meta ? {}
  , nugetPackagesJson
}:
let openssl_1_0_symlinks = runCommand "openssl-1.0-symlinks" { } ''
            mkdir -p $out/lib
            ln -s ${openssl.out}/lib/libcrypto.so $out/lib/libcrypto.so.1.0.0
            ln -s ${openssl.out}/lib/libcrypto.so $out/lib/libcrypto.so.10
            ln -s ${openssl.out}/lib/libssl.so $out/lib/libssl.so.1.0.0
            ln -s ${openssl.out}/lib/libssl.so $out/lib/libssl.so.10
        '';
    fetchNuPkg = 
      { url , fileName , sha512, ... }:
      fetchurl {
          inherit url sha512;
          name = "${fileName}.nupkg";
    };
    packageInfos = builtins.fromJSON (builtins.readFile nugetPackagesJson);
    nugetPackages = map (p: {
      name = "${p.fileName}.nupkg";
      path = fetchNuPkg p;
    }) packageInfos;
    nugetSource = linkFarm "${pname}-packages" nugetPackages;
in 
stdenv.mkDerivation rec {
  name = "${pname}-${version}";
  nativeBuildInputs =  [ dotnetSdkPackage makeWrapper ];

  inherit src mono;

  buildPhase = ''
    runHook preBuild
    if [ "$mono" != "" ]; then
    export FrameworkPathOverride=${mono}/lib/mono/4.5/
    fi
    export DOTNET_CLI_TELEMETRY_OUTPUT=true
    export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
    # avoid permission denied error
    export HOME=$PWD
    touch $HOME/.dotnet/$(dotnet --version).dotnetFirstUseSentinel
    echo "Running dotnet restore"
    dotnet restore --use-lock-file --locked-mode --source ${nugetSource}
    echo "Running dotnet build"
    dotnet build --no-restore --configuration ${configuration} ${project}
    runHook postBuild
  '';

  installPhase = ''
    runHook preInstall
    echo Running dotnet publish
    dotnet publish  --no-restore --no-build --configuration ${configuration} -o $out/ ${project}
    mkdir -p $out/bin
    makeWrapper  $out/${pname} $out/bin/${pname} --set DOTNET_ROOT ${dotnetSdkPackage} 
    runHook postInstall
  '';

  rpath = stdenv.lib.makeLibraryPath [ stdenv.cc.cc libunwind libuuid icu openssl zlib curl ];
  postFixup = ''
      patchelf --set-interpreter "${stdenv.cc.bintools.dynamicLinker}" $out/${pname}
      patchelf --set-rpath "${rpath}" $out/${pname}
  '';

  inherit meta;
}
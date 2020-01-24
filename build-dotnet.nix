{ dotnet-sdk, dotnetSdkPackage ? dotnet-sdk, stdenv, libunwind, libuuid, icu, openssl, zlib, curl, makeWrapper, callPackage }:
{ baseName
  , version
  , src
  , additionalWrapperArgs ? ""
  , mono ? ""
  , project ? ""
  , configuration ? "Release"
}:
let fetchDotnet = callPackage ./fetchDotnet.nix { inherit dotnetSdkPackage; };
in
stdenv.mkDerivation rec {
  name = "${baseName}-${version}";
  nativeBuildInputs =  [ dotnetSdkPackage makeWrapper ];

  nugetPackages = fetchDotnet { 
    inherit src name;
    sha256 = "0w6xwznddfwam3m7lvfq2y6sdqbb0i253xcanf2v92vvaf5va6qy";
  };
  inherit src mono;

  buildPhase = ''
    runHook preBuild

    if [ "$mono" != "" ]; then
    export FrameworkPathOverride=${mono}/lib/mono/4.5/
    fi

    export DOTNET_CLI_TELEMETRY_OPTOUT=true
    export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
    # avoid permission denied error
    export HOME=$PWD
    touch $HOME/.dotnet/$(dotnet --version).dotnetFirstUseSentinel


    echo "Running dotnet restore"
    export NUGET_PACKAGES=$nugetPackages
    dotnet restore --locked-mode ${project}
    echo "Running dotnet build"
    dotnet build --no-restore --configuration ${configuration} ${project}


    runHook postBuild
  '';

  installPhase = ''
    runHook preInstall

    echo Running dotnet publish
    dotnet publish  --no-restore --no-build --configuration ${configuration} -o $out/ ${project}

    mkdir -p $out/bin
    makeWrapper  $out/${baseName} $out/bin/${baseName} --set DOTNET_ROOT ${dotnetSdkPackage} 

    runHook postInstall
  '';

  rpath = stdenv.lib.makeLibraryPath [ stdenv.cc.cc libunwind libuuid icu openssl zlib curl ];
  postFixup = ''
      patchelf --set-interpreter "${stdenv.cc.bintools.dynamicLinker}" $out/${baseName}
      patchelf --set-rpath "${rpath}" $out/${baseName}
  '';
}

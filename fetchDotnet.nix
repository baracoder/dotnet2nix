{ dotnet-sdk, dotnetSdkPackage ? dotnet-sdk, stdenv, makeWrapper, lib, curlFull, cacert }:
{ name
  , src
  , sha256
 }:
stdenv.mkDerivation {
  nativeBuildInputs =  [ dotnetSdkPackage curlFull cacert ];
  src = lib.cleanSource (lib.sourceFilesBySuffices src [ ".csproj" ".fsproj" ".fs" ".sln" "packages.lock.json" ]) ;
  #inherit src;
  name = "${name}-packages";

  outputHashAlgo = "sha256";
  outputHash = sha256;
  outputHashMode = "recursive";


  impureEnvVars = stdenv.lib.fetchers.proxyImpureEnvVars;

  dontBuild = true;
  installPhase = ''
    export DOTNET_CLI_TELEMETRY_OPTOUT=true
    export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
    export DOTNET_CLI_TELEMETRY_OPTOUT=1

    # avoid permission denied error
    export HOME=$PWD
    touch $HOME/.dotnet/$(dotnet --version).dotnetFirstUseSentinel

    mkdir -p $out
    dotnet restore --locked-mode --packages $out
  '';
  dontStrip = true;

}

{ stdenv
, fetchFromGitHub
, dotnetCorePackages
, boost
, libsodium
, pkgconfig
, zeromq
, lib
, dotnet2nixBuilder
}:
dotnet2nixBuilder {
  pname = "Miningcore";
  version = "50";
  project = "Miningcore";
  dotnetSdkPackage = dotnetCorePackages.sdk_3_1;

  src = fetchFromGitHub {
    owner = "akshaynexus";
    repo = "miningcore";
    rev = "318c0c26a8f0fd7fd7588fb6845abacf6fc27c79";
    hash = "sha256-TZrhNE/5XUxAirAXRnAZax4OBOeWQlwNkiackXuJ7bI=";
  } + "/src";

  nugetPackagesJson = ./nugets.json;

  buildInputs = [
    boost
    libsodium
    pkgconfig
    zeromq
  ];

  doCheck = true;
  checkPhase = ''
    dotnet test -c Release --no-build
  '';

  meta = with stdenv.lib; {
    description = "A high-performance Mining-Pool Engine";
    homepage = "https://github.com/akshaynexus/miningcore";
    license = licenses.mit;
    maintainers = with maintainers; [ nrdxp ];
    platforms = [ "x86_64-linux" ];
  };
}
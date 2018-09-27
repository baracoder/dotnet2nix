{
  nixpkgs ? <nixpkgs>
  , pkgs ? import nixpkgs {}
}:
let
  builder = pkgs.callPackage ./build-dotnet.nix {};
in
{
  dotnet2nix = builder {
    baseName = "dotnet2nix";
    version = "2018.1";
    src = ./. ;
  };
}

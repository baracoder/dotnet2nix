{
  pkgs ? import (fetchTarball https://github.com/nixos/nixpkgs-channels/archive/nixos-19.03.tar.gz) {}
}:
let
  builder = pkgs.callPackage ./build-dotnet.nix {};
in
{
  dotnet2nix = builder {
    baseName = "dotnet2nix";
    version = "2018.1";
    project = "./";
    src = pkgs.lib.cleanSource (pkgs.lib.sourceFilesBySuffices ./. [".fs" ".fsproj" ".nix" ]) ;
  };
}

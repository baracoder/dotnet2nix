{
  pkgs ? import (fetchTarball https://github.com/nixos/nixpkgs-channels/archive/nixos-unstable.tar.gz) {}
}:
let
  builder = pkgs.callPackage ./build-dotnet.nix {dotnetSdkPackage = pkgs.dotnetCorePackages.sdk_3_1; };
in
{
  dotnet2nix = builder {
    baseName = "dotnet2nix";
    version = "2020.1";
    project = "./";
    src = pkgs.lib.cleanSource (pkgs.lib.sourceFilesBySuffices ./. [ ".nix" ".sln" "packages.lock.json" ".fs" ".fsproj" ]) ;
  };
}

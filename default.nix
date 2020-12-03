{
  pkgs ? import (fetchTarball https://github.com/nixos/nixpkgs-channels/archive/nixos-unstable.tar.gz) {}
}:
let
  builder = pkgs.callPackage ./lib/dotnet2nix {};
in
{
  dotnet2nix = builder {
    pname = "dotnet2nix";
    version = "2020.1";
    project = "./src/dotnet2nix";
    dotnetSdkPackage = pkgs.dotnetCorePackages.sdk_3_1; 
    src = pkgs.lib.cleanSource (pkgs.lib.sourceFilesBySuffices ./. [ ".sln" "packages.lock.json" ".fs" ".fsproj" ]) ;
    nugetPackagesJson = ./nugets.json;
  };
}

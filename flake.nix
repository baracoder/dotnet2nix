rec {
  description = "dotnet2nix & builder";
  inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils}: 
  with flake-utils.lib;
  {
      overlay = import ./overlay.nix;

  } // eachSystem [ "x86_64-linux" ] (system: 
    let pkgs = import nixpkgs { 
          inherit system;
          overlays = [
             (import ./overlay.nix)
          ];
        };
    in
    with pkgs; rec {
      checks = {
        miningcore = pkgs.callPackage ./test/miningcore {};
        inherit dotnet2nix;
      };

      jobs = {
        inherit packages;
        inherit apps;
      };
      packages = {
        inherit dotnet2nix dotnet2nixBuilder;
      };

      apps = {
        dotnet2nix = mkApp { drv = dotnet2nix; name = "nyris-data"; };
      };
    }
  );
}

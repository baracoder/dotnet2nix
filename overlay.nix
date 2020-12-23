final: prev:
with final; {

    dotnet2nix = callPackage ./pkgs/dotnet2nix {};
    dotnet2nixBuilder = callPackage ./pkgs/dotnet2nixBuilder {};
}
{ lib, dotnetCorePackages, dotnet2nixBuilder }:
dotnet2nixBuilder {
  pname = "dotnet2nix";
  version = "2020.2";
  project = "src/dotnet2nix";
  dotnetSdkPackage = dotnetCorePackages.sdk_3_1; 
  src = lib.cleanSource (lib.sourceFilesBySuffices ./../.. [ ".sln" "packages.lock.json" ".fs" ".fsproj" ]) ;
  nugetPackagesJson = ./nugets.json;
}

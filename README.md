# dotnet2nix WiP

WiP tool to convert dotnet dependendencies to nix expressions

The first goal is to use the generated files to build this package itself,
manipulationg nuget config for dotnet restore.

## TODO

**general**

* [x] Build a working derivation from nix files.
* [ ] use `builtins.fromJSON` for nuget list
* [ ] Create besides `nuget.nix` file, a template for `default.nix`
* [ ] Test with sources different then `./.` maybe `fetchFromGitHub`
* [ ] Check if integration with `fetchFromNuGet` of `nixpkgs` makes sense
* [ ] Add MSBuild restore hook? (Always call dotnet2nix, after `dotnet restore` to update dependencies list)
* [ ] Allow `--self-contained` binaries, requires `--runtime` identifier + additional packages


**custom nuget sources**

* [ ] Allow custom urls
* [ ] Basic authentication
* [ ] Parse Nuget.Config files for sources and credentials?
* [ ] Allow for additional information to be an optional input suplied by hydra or CI server


## Installation

```
nix-env -i dotnet2nix -f ./
```

## Usage

(Some of this might work already)

1. Change into your dotnet project directory
2. Restore the packages `dotnet restore`
3. Generate the nix files `dotnet2nix`
4. Try to build your project `nix-build --no-out-link`

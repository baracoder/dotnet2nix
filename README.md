# dotnet2nix WiP

WiP tool to convert dotnet dependendencies to nix expressions

The first goal is to use the generated files to build this package itself,
manipulationg nuget config for dotnet restore.

## TODO

**general**

* [x] Build a working derivation from nix files.
* [x] use `builtins.fromJSON` for nuget list
* [ ] Create besides `nuget.nix` file, a template for `default.nix`
* [ ] Test with sources different then `./.` maybe `fetchFromGitHub`
* [ ] Check if integration with `fetchFromNuGet` of `nixpkgs` makes sense
* [ ] Add MSBuild restore hook? (Always call dotnet2nix, after `dotnet restore` to update dependencies list)
* [ ] Allow `--self-contained` binaries, requires `--runtime` identifier + additional packages


**custom nuget sources**

* [x] Allow custom urls
* [ ] Basic authentication
* [ ] Parse Nuget.Config files for sources and credentials?
* [ ] Allow for additional information to be an optional input suplied by hydra or CI server


## Installation

```
nix-env -i dotnet2nix -f ./
```

## Problems 

### Updated packages

Some packages get overwritten with an updated version while the name and the url
does not change. This changes the hash.
I didn't find any way to address packages on the nuget API by the content hash yet.

### Multiple sources

If a package is present in multiple sources, the fastest one wins.
This can lead to inconsistencies, while generating the nuget list on different
machines. 
See https://github.com/NuGet/Home/issues/5611

## Usage

(Some of this might work already)

1. Change into your dotnet project directory
2. Restore the packages `dotnet restore`
3. Generate the nix files `dotnet2nix`
4. Try to build your project `nix-build --no-out-link`

### Autentication

This seams to be in a not to good state at the moment of writing.

It is possible to add credentials to the url directly, which is simple and works
good, but those will show up in all the logs. You can also use a `netrc` file
instead. [Nix](https://nixos.org/nix/manual/#description-41) will use the
following
location by default: `$NIX_CONF_DIR/netrc`. The connents are as follows:

```
machine my-machine
login my-username
password my-password
...
```
For more details see the 
[curl documentation on netrc](https://ec.haxx.se/usingcurl-netrc.html)

# dotnet2nix

Tool to convert dotnet nuget dependencies to nix expressions

The first goal is to use the generated files to build this package itself,
manipulating nuget config for dotnet restore.

## TODO

**general**

* [x] Build a working derivation from nix files.
* [x] use `builtins.fromJSON` for nuget list
* [ ] Create besides `nuget.nix` file, a template for `default.nix`
* [ ] Test with sources different then `./.` maybe `fetchFromGitHub`
* [ ] Check if integration with `fetchFromNuGet` of `nixpkgs` makes sense
* [ ] Allow `--self-contained` binaries, requires `--runtime` identifier + additional individual packages on restore


**custom nuget sources**

* [x] Allow custom urls
* [ ] Basic authentication
* [ ] Parse Nuget.Config files for sources and credentials?
* [ ] Allow for additional information to be an optional input supplied by hydra or other CI server


## Installation

```
nix-env -i dotnet2nix -f ./
```

## Problems 

### Packages updated in-place (might be a problem)

Some packages got overwritten with an updated version to add repo signatures.
This changes the hash. If packages will get updated often in place, it could get problematic.
See [nuget.org blog post](https://blog.nuget.org/20180810/Introducing-Repository-Signatures.html)
and [my ticket](https://github.com/dotnet/coreclr/issues/20489)

### Multiple sources

If a package is present in multiple sources, the fastest one wins.
This can lead to inconsistencies if there are packages with different signatures.
This will lead to wrong checksums. See [nuget ticket](https://github.com/NuGet/Home/issues/5611)

## Usage

1. Change into your dotnet project directory
2. Restore the packages `dotnet restore`
3. Generate the nix files `dotnet2nix`
4. Try to build your project `nix-build --no-out-link`

### Authentication

This seams to be in a not to good state at the moment of writing.

It is possible to add credentials to the url directly, which is simple and works
good, but those will show up in all the logs.

You *should be able* (currently it does not work) to use a `netrc` file
instead. [Nix](https://nixos.org/nix/manual/#description-41) will use the
following
location by default: `$NIX_CONF_DIR/netrc`. The contents are as follows:

```
machine my-machine
login my-username
password my-password
...
```
For more details see the 
[curl documentation on netrc](https://ec.haxx.se/usingcurl-netrc.html)

Most download commands do not use it and sandboxing requires additional
tuning to allow accessing this file.

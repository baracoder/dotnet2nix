# dotnet2nix

Tool to build and package dotnet-sdk applications with nix

## Project status

It is currently working for simple projects and solutions with one project.

## Features

* [ ] Support for whole solutions or multiple projects
    * [x] Discovery works
    * [ ] Publishing
* [x] Authentication through netrc file
* [x] Parses `Nuget.Config` files for sources and credentials
* [x] `<DotNetCliToolReference>` support (Not part of project.assets.json)
* [ ] `--self-contained` binaries, requires `--runtime` identifier + additional individual packages on restore


## Installation

```
nix-env -i dotnet2nix -f ./
```

## Usage

1. Change into your dotnet project directory
2. Restore the packages `dotnet restore`
3. Run `dotnet2nix` to get a `nugets.json` and `build-dotnet.nix` file.
4. Build your project `nix-build`

## Problems 

### Packages updated in-place

With NuGet, packages can be updated in place.
I experienced this problem with a project having multiple package sources.
If multiple sources have a package with the same version string,
the faster one wins with `dotnet restore`

Some packages got overwritten with an updated version to add repo signatures.
This changes the hash. If packages will get updated often in place, it could get problematic.
See [nuget.org blog post](https://blog.nuget.org/20180810/Introducing-Repository-Signatures.html)
and [my ticket](https://github.com/dotnet/coreclr/issues/20489)

With the current `dotnet restore`, if a package is present in multiple sources, the fastest one wins.
This can lead to inconsistencies if there are packages with different signatures and checksums.
See [nuget ticket](https://github.com/NuGet/Home/issues/5611)


### Authentication

You *should be able* to use a `netrc` file
instead. [Nix](https://nixos.org/nix/manual/#description-41) will use the
following
location by default: `$NIX_CONF_DIR/netrc`. The contents are as follows:

```
machine hostname-of-the-feed
login my-username
password my-password
...
```
For more details see the 
[curl documentation on netrc](https://ec.haxx.se/usingcurl-netrc.html)


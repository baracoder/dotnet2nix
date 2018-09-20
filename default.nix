{nixpkgs ? <nixpkgs> }:
let
    pkgs = import nixpkgs {};
    fetchNuGet = (
      attrs @
      { baseName
      , version
      , urls ? [ "https://www.nuget.org/api/v2/package/${baseName}/${version}" ]
      , sha512
      , ...
      }:
      pkgs.fetchurl {
      inherit urls sha512;
      name = "${baseName}.${version}.nupkg";
    });
    nugets = pkgs.callPackages ./dotnet2nix/nuget.nix {fetchNuGet = fetchNuGet;};
    allNuGets = pkgs.recurseIntoAttrs nugets;
    nugetsList = builtins.filter pkgs.lib.isDerivation (pkgs.lib.mapAttrsToList (name: value: value) allNuGets);
    jobs = rec {
        nugets = allNuGets;
        dotnet2nix = pkgs.stdenv.mkDerivation rec {
            baseName = "dotnet2nix";
            version = "2018.1";
            name = "${baseName}-${version}";
            buildInputs =  [ pkgs.dotnet-sdk ];
            nugets = map (n: "${n}:${n.name}") nugetsList;
            src = ./. ;
            buildPhase = ''
                runHook preBuild

                export DOTNET_CLI_TELEMETRY_OPTOUT=true
                export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
                # avoid permission denied error
                export HOME=$PWD

                mkdir packages
                cd packages
                for p in $nugets; do
                  pstore=$(echo $p|cut -f1 -d:)
                  plink=$(echo $p|cut -f2 -d:)
                  ln -s $pstore $plink
                done
                cd ..

                echo Running dotnet restore
                dotnet restore --source $PWD/packages
                echo Running dotnet build
                dotnet build --no-restore

                runHook postBuild
            '';
            dontStrip = true;

            # TODO get dotnet runtime accepted, replace sdk by runtime here for much smaller closures
            # TODO add option to produce self-contained results
            installPhase = ''
                runHook preInstall

                echo Running dotnet publish
                dotnet publish --no-restore --no-build -o $out

                echo Creating wrapper
                mkdir $out/bin
                cat << EOF >> $out/bin/dotnet2nix
                #!/bin/sh
                ${pkgs.dotnet-sdk}/bin/dotnet $out/dotnet2nix.dll -- "$@"
                EOF
                chmod +x $out/bin/dotnet2nix
                runHook postInstall
            '';

        };
    };
in
    jobs

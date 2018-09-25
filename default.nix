{nixpkgs ? <nixpkgs> }:
let
    pkgs = import nixpkgs {};
    baseUrls = if builtins.pathExists ./dotnet2nix/urls.json then builtins.fromJSON (builtins.readFile ./dotnet2nix/urls.json ) else [ "https://www.nuget.org/api/v2/package" ];
    getUrls = baseName: version: urls: if urls != null then urls else map (base: "${base}/${baseName}/${version}" ) baseUrls;
    fetchNuGet = ({
      baseName
      , version
      , urls ? null
      , sha512
      }: pkgs.fetchurl {
          inherit sha512;
          urls = getUrls baseName version urls;
          name = "${baseName}.${version}.nupkg";
    });
    nugetInfos = builtins.fromJSON (builtins.readFile ./dotnet2nix/nugets.json );
    nugetsList = builtins.filter pkgs.lib.isDerivation (map (n: fetchNuGet n) nugetInfos);
in
{
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
}


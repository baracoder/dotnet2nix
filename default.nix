{ system ? builtins.currentSystem or "unknown-system" }:

#((builtins.getFlake (toString ./.)).packages.${system})
(builtins.getFlake (toString ./.)).packages.${system}


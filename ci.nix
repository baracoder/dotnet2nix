{ system ? builtins.currentSystem or "unknown-system" }:

(builtins.getFlake (toString ./.)).checks.${system}


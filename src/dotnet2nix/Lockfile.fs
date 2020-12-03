module dotnet2nix.Lockfile

open FSharp.Data
open Types

let dependencyToPkgInfo framework (name: string, value: JsonValue) =
    let _type = value.Item("type").AsString()
    if _type = "Project" then
        None
    else
        let resolvedVersion = value.Item("resolved").AsString()
        let hash =
            value.Item("contentHash").AsString()
            |> Utils.b64ToHex
        Some { Name=name; Version=resolvedVersion; Sha512=hash; Framework=framework; }

let getDependencies fileContents =
    let root = JsonValue.Parse fileContents
    let frameworks = root.Item("dependencies").Properties()
    
    frameworks
    |> Array.collect (fun (fwk, values) -> values.Properties() |> Array.choose (dependencyToPkgInfo fwk))
    
    

    
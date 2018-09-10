// Learn more about F# at http://fsharp.org

open System
open System.IO
open FSharp.Data

type PkgInfo = { AttrName: string; Name: string ; Version: string; Sha512: string }

// combine paths with custom operator
let (+/) p1 p2 = Path.Combine(p1, p2)

let splitNameVersion (name:string) = 
    name.Split "/" 
    |> Array.toList

let getPackageInfos (path: string) =
    let json = JsonValue.Load path
    let libraries = json.GetProperty  "libraries"
    libraries.Properties ()
    |> Array.map (fun (s, p:JsonValue) -> 
        let 
            [ name ; version ] = splitNameVersion s
        in
            {
                AttrName = name.Replace(".", "")
                Name = name;
                Version = version;
                Sha512 = p.GetProperty("sha512").AsString()
            }
    )

let pkgInfoAsNixStr pkgInfo =
    sprintf """
            %s = fetchNuGet { 
              baseName = "%s";
              version = "%s";
              sha512 = "%s";
              outputFiles = [ "*" ];
            };
            """ pkgInfo.AttrName pkgInfo.Name pkgInfo.Version pkgInfo.Sha512

[<EntryPoint>]
let main argv =
    let loadPath = Environment.CurrentDirectory +/ "obj/project.assets.json"
    let pkgInfos = getPackageInfos loadPath
    Array.iter (fun e -> printfn "%A\n" e) pkgInfos
    
    let packageDescriptions = 
        pkgInfos
        |> Array.toList
        |> List.map pkgInfoAsNixStr
        |> String.concat "\n"
    let contents =  
        sprintf """
                { fetchNuGet } : {
                %s
                }
                """ packageDescriptions
    
    System.IO.File.WriteAllText ("nuget.nix", contents)
    
    0 // return an integer exit code

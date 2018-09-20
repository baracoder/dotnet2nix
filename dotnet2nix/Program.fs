﻿// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.IO.Enumeration
open FSharp.Data

type PkgInfo = { Name: string ; Version: string; Sha512: string }

// combine paths with custom operator
let (+/) p1 p2 = Path.Combine(p1, p2)

let b64ToBytes s = System.Convert.FromBase64String(s)

let bytesToHex bytes =
    bytes
    |> Array.map (fun (x : byte) -> System.String.Format("{0:x2}", x))
    |> String.concat System.String.Empty

let splitNameVersion (name:string) = 
    name.Split "/" 
    |> Array.toList

let hexToNixNotation s =
    let si = System.Diagnostics.ProcessStartInfo(RedirectStandardOutput=true, FileName="nix-hash", Arguments="--type sha512 --to-base32 " + s)
    use p = new System.Diagnostics.Process(StartInfo = si)
    p.Start() |> ignore
    p.WaitForExit()
    p.StandardOutput.ReadLine()


let getPackageInfos (path: string) =
    let json = JsonValue.Load path
    let libraries = json.GetProperty  "libraries"
    libraries.Properties ()
    |> Array.map (fun (s, p:JsonValue) -> 
        let [ name ; version ] = splitNameVersion s
        let sha512str = p.GetProperty("sha512").AsString()
                        |> b64ToBytes
                        |> bytesToHex
                        |> hexToNixNotation
        {
            Name = name;
            Version = version;
            Sha512 =  sha512str
        }
    )


let pkgInfoAsJsonValue pkgInfo =
    [|
      "baseName", JsonValue.String pkgInfo.Name;
      "version",  JsonValue.String pkgInfo.Version;
      "sha512", JsonValue.String pkgInfo.Sha512
    |] 
    |> JsonValue.Record

[<EntryPoint>]
let main argv =
    let loadPath = Environment.CurrentDirectory +/ "obj/project.assets.json"
    let pkgInfos = getPackageInfos loadPath
    Array.iter (fun e -> printfn "%A\n" e) pkgInfos
    
    let packageDescriptions = 
        pkgInfos
        |> Array.map pkgInfoAsJsonValue
        |> JsonValue.Array
        
    use f = File.CreateText("nugets.json")
    packageDescriptions.WriteTo (f, JsonSaveOptions.None)
    
    0

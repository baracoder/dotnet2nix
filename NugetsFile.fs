module dotnet2nix.NugetsFile

open dotnet2nix.Types
open dotnet2nix.Utils
open FSharp.Data
open System.IO
open System

let private downloadInfoAsJsonValue p =
    [|
      "fileName", JsonValue.String p.FileName
      "sha512", JsonValue.String p.Sha512
      "url", JsonValue.String p.SourceUrl
    |] 
    |> JsonValue.Record
    
        
/// Writes buider file "build-dotnet.nix"
let writeBuilder replace =
    let builderFilename = "build-dotnet.nix"
    if File.Exists builderFilename && not replace then 
        printf "%s exists, not replacing.\n" builderFilename
    else
        File.Delete(Environment.CurrentDirectory +/ builderFilename)
        File.Copy(
            AppDomain.CurrentDomain.BaseDirectory +/ builderFilename,
            Environment.CurrentDirectory +/ builderFilename, true)
        
/// Writes nugets.json.
let writeNugets replace downloadInfos =
    try
        let packageDescriptions =
            downloadInfos
            |> Seq.sortBy (fun i -> i.FileName)
            |> Seq.map downloadInfoAsJsonValue
            |> Array.ofSeq
            |> JsonValue.Array
        do
            use f = File.CreateText("nugets.json")
            packageDescriptions.WriteTo (f, JsonSaveOptions.None)
            f.Close()
        writeBuilder replace
        Ok "File written"
    with
        | e -> Error e.Message
        

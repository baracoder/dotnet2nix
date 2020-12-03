module dotnet2nix.NugetsFile

open Types
open FSharp.Data
open System.IO

let private downloadInfoAsJsonValue p =
    [|
      "fileName", JsonValue.String p.FileName
      "sha512", JsonValue.String p.Sha512
      "url", JsonValue.String p.SourceUrl
    |] 
    |> JsonValue.Record
    
        
/// Writes nugets.json.
let writeNugets downloadInfos =
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
        Ok "File written"
    with
        | e -> Error e.Message
        

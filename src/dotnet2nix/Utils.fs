module dotnet2nix.Utils

open System
open System.Diagnostics
open System.IO
open System.Threading.Tasks

/// Combines two parts of a path.
let (+/) p1 p2 = IO.Path.Combine(p1, p2)

/// Converts a base64 string to a lowercase hexadecimal representation.
let b64ToHex s = (s |> Convert.FromBase64String |> BitConverter.ToString).Replace("-", "").ToLowerInvariant()

let private getStreamOutput (stream: StreamReader) =
    //Read output in separate task to avoid deadlocks
    let outputReadTask = Task.Run(fun () -> stream.ReadToEnd())
    outputReadTask.Result

let runProgram program args =
    // Start the child process.
     let p = new Process()
     // Redirect the output stream of the child process.
     p.StartInfo.UseShellExecute <- false
     p.StartInfo.RedirectStandardOutput <- true
     p.StartInfo.RedirectStandardError <- true
     p.StartInfo.FileName <- program
     args
     |> List.iter p.StartInfo.ArgumentList.Add
     p.Start() |> ignore
     // I guess this works because of compiler optimization
     let output = getStreamOutput p.StandardOutput
     let outputErr = getStreamOutput p.StandardError
     p.WaitForExit()
     match p.ExitCode with
     | 0 -> Ok output
     | _ -> Error (p.ExitCode, outputErr)

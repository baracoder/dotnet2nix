open Argu
open System
open System.IO

open dotnet2nix
open dotnet2nix.Types
open dotnet2nix.Utils


type CLIArguments =
    | [<AltCommandLine("-f")>] ReplaceBuilder
    | [<AltCommandLine("-n")>] NoCache
    | [<AltCommandLine("-p")>] PackagesFolder
    | [<AltCommandLine("-l")>] LockFilePath of path:string
    | NoParallel
    | [<MainCommand>] TargetPath of target:string
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ReplaceBuilder -> "Replace build-dotnet.nix with default one."
            | NoCache -> "Use NoCache while getting metadata (Not sure, but probably only http level)."
            | LockFilePath _ -> "Path to lockfile (project.assets.json)."
            | TargetPath _ -> "Optional path to a directory containing a lockfile (project.assets.json), solution (.sln), C# project (.csproj) or F# project (.fsproj) to analyze."
            | NoParallel -> "Don't make http requests in rarallel."
            | PackagesFolder -> "Folder where nuget packages are stored, defaults to NUGET_PACKAGES environment variable or else  ~/.nuget/packages."
    
        

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<CLIArguments>(programName = "dotnet2nix")
    let args = parser.Parse(argv)
    let replaceArg = args.Contains ReplaceBuilder
    let parallelRequests = if args.Contains NoParallel then 1 else 30
    use nugetCache = Nuget.getCache (args.Contains(NoCache))
    
    let targetPath =
        args.TryGetResult TargetPath
        |> Option.defaultValue Environment.CurrentDirectory
        |> Path.GetFullPath
    let targetDirectory = if File.Exists(targetPath) then Path.GetDirectoryName(targetPath) else targetPath
        
    let nugetSettings = NuGet.Configuration.Settings.LoadDefaultSettings (targetDirectory)
    let sources = Nuget.getSources nugetSettings
    
    let packagesDirectory =
        let env = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
        if env <> null && env <> String.Empty then
            env
        else
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +/ ".nuget" +/ "packages"
        
    // general flow: Target -> lockfile list -> PackageInfo list -> DownloadInfo list -> JsonValue
    // 1. discover project or solution and get configuration
    // 2. Collect package information: Name, Version, Framework, Sha512
    // 3. Get download urls
    // 4. Write file
    result {
        let! target = Nuget.discoverTarget targetPath
        let lockfiles = Nuget.getLockfilesForTarget packagesDirectory target
        let packageInfos =
            lockfiles
            |> Seq.collect Nuget.getNugetInfo
            |> Seq.distinct
            
        let! downloadInfos =
            Nuget.getDownloadInfos sources nugetCache parallelRequests packageInfos
            |> Result.map Seq.distinct
        let! writeFileResult = NugetsFile.writeNugets replaceArg downloadInfos
        return writeFileResult
    }
    |> function
    | Ok msg ->
        printfn "%s" msg
        0
    | Error msg ->
        eprintf "Error: %s" msg
        1
            

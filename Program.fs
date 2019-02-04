open Argu
open System
open System.IO
open System.Threading
open FSharp.Data
open NuGet.Common
open NuGet.Configuration
open NuGet.Frameworks
open NuGet.ProjectModel
open NuGet.Protocol
open NuGet.Protocol.Core.Types
open NuGet.Versioning


type PkgInfo = {
    Name: string
    Version: string
    Sha512: string
    SourceUrls: string list
    Frameworks: NuGetFramework list }


type CLIArguments =
    | [<AltCommandLine("-f")>] ReplaceBuilder
    | [<AltCommandLine("-n")>] NoCache
    | [<AltCommandLine("-l")>] LockFilePath of path:string
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ReplaceBuilder -> "Replace build-dotnet.nix with default one."
            | NoCache -> "Use NoCache while getting metadata (Not sure, but probably only http level)."
            | LockFilePath _ -> "Path to lockfile (project.assets.json)."

let (+/) p1 p2 = Path.Combine(p1, p2)
let b64ToBytes s = System.Convert.FromBase64String(s)
let b64ToHex s = (s |> Convert.FromBase64String |> BitConverter.ToString).Replace("-", "").ToLowerInvariant()


let getNugetInfo lockFilePath =
    
    let logger = NuGet.Common.NullLogger()
    let lockfile = LockFileUtilities.GetLockFile(lockFilePath, logger)
    let target = lockfile.Targets.Item 0
    
    lockfile.Libraries
    |> Seq.map (fun lib -> {
        Name = lib.Name
        Version = lib.Version.ToString()
        Sha512 = b64ToHex lib.Sha512
        SourceUrls = []
        Frameworks = lockfile.Targets |> Seq.map (fun t ->  t.TargetFramework) |> List.ofSeq })
    |> Array.ofSeq
    
    
let getSourceUrlForPackage sourceCacheContext package =
    printf "Getting url for package %s ...\n" package.Name
    let config = NuGet.Configuration.Settings.LoadDefaultSettings ("./")
    let sources =
        SettingsUtility.GetEnabledSources config
        |> Array.ofSeq
        |> Array.where (fun s -> not s.IsLocal)
    let frameworks = package.Frameworks
    
    let getUrlsForSource s =
        
        let prov = Repository.Provider.GetCoreV3()
        let sourceRepository = SourceRepository(s, prov)
        
        let depInfoResource = sourceRepository.GetResourceAsync<DependencyInfoResource>().Result
        
        depInfoResource.ResolvePackages(package.Name, frameworks.Item 0, sourceCacheContext, NullLogger.Instance, CancellationToken.None).Result
        |> Seq.where (fun r -> r.Version.Equals(SemanticVersion.Parse(package.Version)))
        |> Seq.map (fun i -> i.DownloadUri.AbsoluteUri)
        
    let urls = 
        sources
        |> Seq.collect getUrlsForSource
    { package with
        SourceUrls = urls |> List.ofSeq }
    

let pkgInfoAsJsonValue p =
    [|
      "name", JsonValue.String p.Name
      "version", JsonValue.String p.Version // just for debugging
      "sha512", JsonValue.String p.Sha512
      "url", p.SourceUrls |> List.item 0 |> JsonValue.String 
    |] 
    |> JsonValue.Record
    
    
let writeBuilderFile replace =
    let builderFilename = "build-dotnet.nix"
    if File.Exists builderFilename && not replace then 
        printf "%s exists, not replacing.\n" builderFilename
    else 
        File.Copy(
            AppDomain.CurrentDomain.BaseDirectory +/ builderFilename,
            Environment.CurrentDirectory +/ builderFilename, true)
        

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<CLIArguments>(programName = "dotnet2nix")
    let args = parser.Parse(argv)
    let lockFilePath =
        args.TryGetResult LockFilePath
        |> Option.defaultValue (Environment.CurrentDirectory +/ "obj/project.assets.json")
    
    let pkgInfos = getNugetInfo lockFilePath
    use sourceCacheContext = new SourceCacheContext(NoCache = args.Contains(NoCache))
    
    let packageDescriptions = 
        pkgInfos
        |> Array.Parallel.map (getSourceUrlForPackage sourceCacheContext)
        |> Array.map pkgInfoAsJsonValue
        |> JsonValue.Array
        
    use f = File.CreateText("nugets.json")
    packageDescriptions.WriteTo (f, JsonSaveOptions.None)
    
    let replace = args.Contains ReplaceBuilder
    writeBuilderFile replace
    0

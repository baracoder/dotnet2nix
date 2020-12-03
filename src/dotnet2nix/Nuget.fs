module dotnet2nix.Nuget

open FSharp.Collections.ParallelSeq
open FSharp.Data
open NuGet.Common
open NuGet.Configuration
open NuGet.Protocol
open NuGet.Frameworks
open Microsoft.Build.Construction
open NuGet.Protocol.Core.Types
open System.IO
open System.Threading
open System.Threading.Tasks
open NuGet.Versioning
open Types
open Utils


type ProjFile = XmlProvider<"""
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.2</TargetFramework>
  </PropertyGroup>
  <PropertyGroup>
    <TargetFramework>netcoreapp2.2</TargetFramework>
    <OutputType>exe</OutputType>
  </PropertyGroup>
  <PropertyGroup></PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.All" Version="2.2.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="2.2.0" />
  </ItemGroup>
  <ItemGroup></ItemGroup>
  <ItemGroup>
    <DotNetCliToolReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Tools" Version="1.0.1" />
    <DotNetCliToolReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Tools" Version="1.0.1" />
  </ItemGroup>
</Project>
""">

let getToolLockFiles packagesDirectory (toolId:string) (version:string) =
    printfn "%A %A %A" packagesDirectory toolId version
    let toolDirectory = packagesDirectory +/ ".tools" +/ (toolId.ToLowerInvariant())
    let versions = Directory.GetDirectories(toolDirectory)
    let getAssetFilesForVersion version =
        Directory.GetDirectories(version)
        |> Array.collect (fun framework -> Directory.GetFiles(framework, "project.assets.json"))
    versions
    |> Array.collect getAssetFilesForVersion
    
    
let getDotnetCliToolLockfiles packagesDirectory (projFile:string) =
    let proj = ProjFile.Load(projFile)
    let dotnetCliTools =
        proj.ItemGroups
        |> Array.collect (fun ig -> ig.DotNetCliToolReferences)
    dotnetCliTools
    |> Array.collect (fun cli -> getToolLockFiles packagesDirectory cli.Include cli.Version)
    |> List.ofArray
    
    
let getLockfilesOfProject packagesDirectory (path:string) =
    let directory = Path.GetDirectoryName(path)
    List.concat [
        [ directory +/ "packages.lock.json" ]
        //getDotnetCliToolLockfiles packagesDirectory path
    ]
    

let getLockfilesForTarget packagesDirectory = function
    | Solution s ->
        let solution = SolutionFile.Parse(s)
        solution.ProjectsInOrder
        |> Seq.toList
        |> List.where (fun p -> p.ProjectType  <> SolutionProjectType.Unknown && p.ProjectType <> SolutionProjectType.SolutionFolder )
        |> List.map (fun p -> p.AbsolutePath)
        |> List.collect (getLockfilesOfProject packagesDirectory)
    | Project p ->
        p |> (getLockfilesOfProject packagesDirectory)
    | LockFile l ->
        [ l ]


let getDependencyInfoResources source =
    let prov = Repository.Provider.GetCoreV3()
    let sourceRepository = SourceRepository(source, prov)
    sourceRepository.GetResource<DependencyInfoResource>()
   
type NugetPrintLogger() =
    interface ILogger with
        member x.LogDebug(data) = printfn "DEBUG: %s" data
        member x.LogVerbose(data) = printfn "VERB: %s" data
        member x.LogInformation(data) = printfn "I: %s" data
        member x.LogMinimal(data) = printfn "%ss" data
        member x.LogWarning(data) = printfn "%s" data
        member x.LogError(data) = printfn "%s" data
        member x.LogInformationSummary(data) = printfn "%s" data
        member x.Log(level: LogLevel, data) = printfn "%A %s" level data
        member x.Log(msg: ILogMessage) = printfn "%A" msg
        member x.LogAsync(level, data) =
            (x :> ILogger).Log(level, data)
            Task.CompletedTask
        member x.LogAsync(message) =
            (x :> ILogger).Log(message)
            Task.CompletedTask
        
    
let getUrlsForSource sourceCacheContext package (depInfoResource:DependencyInfoResource) =
    let cl = NullLogger.Instance
    //let cl = NugetPrintLogger()
    
    try
        let framework = NuGetFramework package.Framework
        depInfoResource.ResolvePackages(package.Name, framework, sourceCacheContext, cl, CancellationToken.None).Result
        |> Seq.where (fun r -> r.Version.Equals(NuGetVersion.Parse package.Version))
        |> Seq.map (fun i ->
            if i.PackageHash = null then
                eprintfn "Package metadata does not contain a hash: %s %s" package.Name i.DownloadUri.AbsoluteUri
            i)
        |> Seq.map (fun i ->
            // V3 feeds don't provide a hash :(
            let hash = if i.PackageHash <> null then  b64ToHex i.PackageHash else package.Sha512
            (i.DownloadUri.AbsoluteUri, hash ))
    with
    | e ->
        printf "Error: %s %s: %s" package.Name package.Version (e.ToString())
        Seq.empty
    
    
let getSourceUrlForPackage sources sourceCacheContext package =
    printf "Getting url for package %s %s ...\n" package.Name package.Version
    let sourceUrls =
        sources
        |> Seq.collect (getUrlsForSource sourceCacheContext package)
    if Seq.length sourceUrls = 0 then
        eprintfn "%s %A\n" package.Name sourceUrls
    let url, hash = Seq.item 0 sourceUrls
    if hash <> package.Sha512 then
        eprintfn "Warning: Package %s %s\n local hash\t%s\n remote hash\t%s" package.Name package.Version package.Sha512 hash
    { FileName = sprintf "%s.%s" package.Name package.Version
      Sha512 = hash
      SourceUrl = url }
    
    
let getDownloadInfos sources sourceCacheContext parallelRequests nugetInfos =
    try
        nugetInfos
        |> PSeq.withDegreeOfParallelism parallelRequests
        |> PSeq.map (getSourceUrlForPackage sources sourceCacheContext)
        |> Ok
    with
        | e -> Error e.Message

let getSources nugetSettings =
    SettingsUtility.GetEnabledSources nugetSettings
    |> Array.ofSeq
    |> Array.mapi (fun i s ->
        printf "Using source rank %d %s (%s)\n" i s.Name s.SourceUri.OriginalString
        s)
    |> Array.where (fun s -> not s.IsLocal)
    |> Array.map getDependencyInfoResources
    
let discoverTarget path :Result<Target, string> =
    match path with
    | p when File.Exists(p) ->
       match path:string with
       | p when p.EndsWith(".sln")
        ->  Solution p |> Ok
       | p when p.EndsWith(".csproj") || p.EndsWith(".fsproj")
        -> Ok <| Project p
       | p when p.EndsWith("project.assets.json")
        -> Ok <| LockFile p
       | _ -> Error "File must have "
    | p when Directory.Exists(p) ->
        let slnFiles = Directory.GetFiles(path, "*.sln")
        let projFiles =
            [
             Directory.GetFiles(path, "*.csproj")
             Directory.GetFiles(path, "*.fsproj")
            ]
            |> Array.concat
        let lockfile = Directory.GetFiles(path, "project.assets.json")
        if slnFiles.Length = 1 then
            (Array.item 0 slnFiles) |> Solution |> Ok
        elif projFiles.Length = 1 then
            Array.item 0 projFiles |> Project |> Ok
        elif lockfile.Length = 1 then
            lockfile |> Array.item 0 |> LockFile |> Ok
        else
            Error "Neither solution, project or lock file found."
     | _ -> Error "No matching files found."

let getCache noCache =
    new SourceCacheContext(NoCache = noCache)

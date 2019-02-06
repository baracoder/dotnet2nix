module dotnet2nix.Nuget

open FSharp.Collections.ParallelSeq
open FSharp.Data
open NuGet.Common
open NuGet.Configuration
open NuGet.Protocol
open NuGet.Frameworks
open Microsoft.Build.Construction
open NuGet.ProjectModel
open NuGet.Protocol.Core.Types
open NuGet.Versioning
open System.IO
open System.Threading
open dotnet2nix.Types
open dotnet2nix.Utils


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
    let versions = System.IO.Directory.GetDirectories(toolDirectory)
    let getAssetFilesForVersion version =
        System.IO.Directory.GetDirectories(version)
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
        [ directory +/ "obj" +/ "project.assets.json" ]
        getDotnetCliToolLockfiles packagesDirectory path
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



let getNugetInfo lockFilePath =
    eprintf "Parsing lockfile %s\n" lockFilePath
    let lockfile = LockFileUtilities.GetLockFile(lockFilePath, NullLogger.Instance)
    
    lockfile.Libraries
    |> Seq.where (fun lib -> lib.Sha512 <> null) // TODO there must be a better indicator
    |> Seq.map (fun lib -> {
        Name = lib.Name
        Version = lib.Version
        Sha512 = b64ToHex lib.Sha512 })
    |> Array.ofSeq
    
    
let getDependencyInfoResources source =
    let prov = Repository.Provider.GetCoreV3()
    let sourceRepository = SourceRepository(source, prov)
    sourceRepository.GetResource<DependencyInfoResource>()
    
    
let getUrlsForSource sourceCacheContext package (depInfoResource:DependencyInfoResource) =
    try
        let framework = NuGetFramework.AnyFramework
        depInfoResource.ResolvePackages(package.Name, framework, sourceCacheContext, NullLogger.Instance, CancellationToken.None).Result
        |> Seq.where (fun r -> r.Version.Equals(package.Version))
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
        printf "Error: %s %s: %s" package.Name package.Version.OriginalVersion (e.ToString())
        Seq.empty
    
    
let getSourceUrlForPackage sources sourceCacheContext package =
    printf "Getting url for package %s %s ...\n" package.Name package.Version.OriginalVersion
    let sourceUrls =
        sources
        |> Seq.collect (getUrlsForSource sourceCacheContext package)
    if Seq.length sourceUrls = 0 then
        eprintfn "%s %A\n" package.Name sourceUrls
    let url, hash = Seq.item 0 sourceUrls
    if hash <> package.Sha512 then
        eprintfn "Warning: Package %s %s\n local hash\t%s\n remote hash\t%s" package.Name package.Version.OriginalVersion package.Sha512 hash
    { FileName = sprintf "%s.%s" package.Name package.Version.OriginalVersion
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
    | p when System.IO.File.Exists(p) ->
       match path:string with
       | p when p.EndsWith(".sln")
        ->  Solution p |> Ok
       | p when p.EndsWith(".csproj") || p.EndsWith(".fsproj")
        -> Ok <| Project p
       | p when p.EndsWith("project.assets.json")
        -> Ok <| dotnet2nix.Types.LockFile p
       | _ -> Error "File must have "
    | p when System.IO.Directory.Exists(p) ->
        let slnFiles = System.IO.Directory.GetFiles(path, "*.sln")
        let projFiles =
            [
             System.IO.Directory.GetFiles(path, "*.csproj")
             System.IO.Directory.GetFiles(path, "*.fsproj")
            ]
            |> Array.concat
        let lockfile = System.IO.Directory.GetFiles(path, "project.assets.json")
        if slnFiles.Length = 1 then
            (Array.item 0 slnFiles) |> Solution |> Ok
        elif projFiles.Length = 1 then
            Array.item 0 projFiles |> Project |> Ok
        elif lockfile.Length = 1 then
            lockfile |> Array.item 0 |> dotnet2nix.Types.LockFile |> Ok
        else
            Error "Neither solution, project or lock file found."
     | _ -> Error "No matching files found."

let getCache noCache =
    new SourceCacheContext(NoCache = noCache)

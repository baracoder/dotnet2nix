module dotnet2nix.Types
    
open NuGet.Versioning
    
type PkgInfo = {
    Name: string
    Version: NuGetVersion
    Sha512: string }

type DownloadInfo = {
    FileName: string
    Sha512: string
    SourceUrl: string }

type Target =
    | Solution of path:string
    | Project of path:string
    | LockFile of path:string
    
type ResultBuilder() =
    member this.Bind(m, f) =
        match m with
        | Error e -> Error e
        | Ok a -> f a
    member this.Return(x) = Ok x
    
let result = ResultBuilder()

module dotnet2nix.Types
    
type PkgInfo = {
    Framework: string
    Name: string
    Version: string
    Sha512: string }

type DownloadInfo = {
    FileName: string
    Sha512: string
    SourceUrl: string }

type Target =
    | Solution of path:string
    | Project of path:string
    | LockFile of path:string
    

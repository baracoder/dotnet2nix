module dotnet2nix.ResultBuilder

type ResultBuilder() =
    member this.Bind(m, f) =
        match m with
        | Error e -> Error e
        | Ok a -> f a
    member this.Return(x) = Ok x
    
let result = ResultBuilder()

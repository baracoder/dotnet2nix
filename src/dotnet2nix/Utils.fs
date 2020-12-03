module dotnet2nix.Utils

open System

/// Combines two parts of a path.
let (+/) p1 p2 = IO.Path.Combine(p1, p2)

/// Converts a base64 string to a lowercase hexadecimal representation.
let b64ToHex s = (s |> Convert.FromBase64String |> BitConverter.ToString).Replace("-", "").ToLowerInvariant()

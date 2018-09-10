
                { fetchNuGet } : {
                
            FSharpCore = fetchNuGet { 
              baseName = "FSharp.Core";
              version = "4.5.2";
              sha512 = "mQlCpyv/wNZAihLBUbBu8gS4NpXpBCTw/eMQqujn8sGBukxSYH3CoE0vPABACNQEv7HoAX7CJt5q7l6Yb2E2hg==";
              outputFiles = [ "*" ];
            };
            

            FSharpData = fetchNuGet { 
              baseName = "FSharp.Data";
              version = "3.0.0-beta4";
              sha512 = "6w1wNkH3sJjkg9RNPA2q7EVN1KQ7t8jvL2bv3Z2W7gnsj3JI9utVOFdvLeZnH87bF4QaiOw5IIUksb194fO5HQ==";
              outputFiles = [ "*" ];
            };
            

            MicrosoftNETCoreApp = fetchNuGet { 
              baseName = "Microsoft.NETCore.App";
              version = "2.1.0";
              sha512 = "AvT774nTFgU8cYcGO9j1EMwuayKslxqYTurg32HGpWa2hEYNuW2+XgYVVNcZe6Ndbr84QX6fwaOZfd5n+1m2OA==";
              outputFiles = [ "*" ];
            };
            

            MicrosoftNETCoreDotNetAppHost = fetchNuGet { 
              baseName = "Microsoft.NETCore.DotNetAppHost";
              version = "2.1.0";
              sha512 = "f/47I60Wg3SrveTvnecCQhCZCAMYlUujWF15EQ/AZTqF/54qeEJjbCIAxKcZI8ToUYzSg6JdfrHggsgjCyCE9Q==";
              outputFiles = [ "*" ];
            };
            

            MicrosoftNETCoreDotNetHostPolicy = fetchNuGet { 
              baseName = "Microsoft.NETCore.DotNetHostPolicy";
              version = "2.1.0";
              sha512 = "p50yZYKzhH64lmArJgoKjtvsNehECa+/sAuOQzZh5uDNBTbRKxjN8IXP1e517xdVsgrFcSNxSEVDKZIOWVjGcQ==";
              outputFiles = [ "*" ];
            };
            

            MicrosoftNETCoreDotNetHostResolver = fetchNuGet { 
              baseName = "Microsoft.NETCore.DotNetHostResolver";
              version = "2.1.0";
              sha512 = "fS9D8a+y55n6mHMbNqgHXaPGkjmpVH9h97OyrBxsCuo3Z8aQaFMJ5xIfmzji2ntUd/3truhMbSgSfIelHOkQpg==";
              outputFiles = [ "*" ];
            };
            

            MicrosoftNETCorePlatforms = fetchNuGet { 
              baseName = "Microsoft.NETCore.Platforms";
              version = "2.1.0";
              sha512 = "TT+QCi9LcxGTjBssH7S7n5+8DVcwfG4DYgXX7Dk7+BfZ4oVHj8Q0CbYk9glzAlHLsSt3bYzol+fOdra2iu6GOw==";
              outputFiles = [ "*" ];
            };
            

            MicrosoftNETCoreTargets = fetchNuGet { 
              baseName = "Microsoft.NETCore.Targets";
              version = "2.1.0";
              sha512 = "etaYwrLZQUS+b3UWTpCnUggd6SQ/ZIkZ5pHnoR7+dIWt/wp2Rv3CvMKOZISsrt7FYCHKwCxfcepuuyEWkQxADg==";
              outputFiles = [ "*" ];
            };
            

            NETStandardLibrary = fetchNuGet { 
              baseName = "NETStandard.Library";
              version = "2.0.3";
              sha512 = "st47PosZSHrjECdjeIzZQbzivYBJFv6P2nv4cj2ypdI204DO+vZ7l5raGMiX4eXMJ53RfOIg+/s4DHVZ54Nu2A==";
              outputFiles = [ "*" ];
            };
            
                }
                
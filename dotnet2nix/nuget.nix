{ fetchNuGet } : { 
FSharpCore = fetchNuGet {
  baseName = "FSharp.Core";
  version = "4.5.2";
  sha512 = "233cqbgk1gfwsny4v17w0g8n6zh9m088003qbsdl317sq2j9jx83hgjwzlal473zpq28179jlvbh17jdsq53h8ji90ddh7z5fkl42cr";
  outputFiles = [ "*" ];
};

FSharpData = fetchNuGet {
  baseName = "FSharp.Data";
  version = "3.0.0-beta4";
  sha512 = "0fvkwz1gnyv294540wyr20shhbxpkhzczk2svsp71aypxj8fa7yq2gfjsfxvvv65zpwidrvlka4sigcm86kqkflhgj9ic7p84v703gb";
  outputFiles = [ "*" ];
};

MicrosoftNETCoreApp = fetchNuGet {
  baseName = "Microsoft.NETCore.App";
  version = "2.1.0";
  sha512 = "0wbcngvczg7v6d3q6gpwh9qpxp5v8vv37bm8586bsz6vf8d8s2bcrm5qrhxzq7a9sc1m5xc49mjxk0hypc3n1l7f4y0a5nki7pzpx02";
  outputFiles = [ "*" ];
};

MicrosoftNETCoreDotNetAppHost = fetchNuGet {
  baseName = "Microsoft.NETCore.DotNetAppHost";
  version = "2.1.0";
  sha512 = "3sq880b4g485q5igrfs50yjii8yii1336kw8012diil4y1akvzqafk5q07i2yaxb2ilp58q0c49j4220bkrvvz4pnmp90qnmlikpzkz";
  outputFiles = [ "*" ];
};

MicrosoftNETCoreDotNetHostPolicy = fetchNuGet {
  baseName = "Microsoft.NETCore.DotNetHostPolicy";
  version = "2.1.0";
  sha512 = "1qwcn2r1s92jhs591qj6wf51ar5a5zgfppdbkw5y36ihayi6q2wvq76c4v473hbn2zsy2a4x0syrnwf1852cav0jsw7x15kh9jk57d7";
  outputFiles = [ "*" ];
};

MicrosoftNETCoreDotNetHostResolver = fetchNuGet {
  baseName = "Microsoft.NETCore.DotNetHostResolver";
  version = "2.1.0";
  sha512 = "2k11s8wln3pq4i8dm6fibpdzmvm8yysw8w9n7qjww4m6s4hqrkkgshadhfarcmkyxhpym59769cd8sx0yl3c6vkk3x9krxjmzql6bvx";
  outputFiles = [ "*" ];
};

MicrosoftNETCorePlatforms = fetchNuGet {
  baseName = "Microsoft.NETCore.Platforms";
  version = "2.1.0";
  sha512 = "0jjg980s96gdg5rmxlwh0vjww0pc8ysijyh6rhqkkpj0b0c8lcknxw3g8qxrv0x7bppy0g8x4lcbf61zkljvyaj86i895hdvqpwy939";
  outputFiles = [ "*" ];
};

MicrosoftNETCoreTargets = fetchNuGet {
  baseName = "Microsoft.NETCore.Targets";
  version = "2.1.0";
  sha512 = "074034i2qhvnvpaf5gjrh6a45hcbpmfmj2693n2pk1gsikn1bzsv1blzqga3rwiwqcqjr1z4klis22jly84w5kmdyz48hfrnb19imks";
  outputFiles = [ "*" ];
};

NETStandardLibrary = fetchNuGet {
  baseName = "NETStandard.Library";
  version = "2.0.3";
  sha512 = "3c6x0z7b5shqf7vzchf4z6iklkwrrg1jz41inlsjxxzdynfh39kdlm5n8yp5y3vva7zw5j9h2yy5g21v667hqr723iplj0ricz3ppmj";
  outputFiles = [ "*" ];
}; }
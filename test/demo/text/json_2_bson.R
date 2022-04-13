imports "JSON" from "R.base";

let json as string = ?"--json" || stop("no json file provided!");
let bson as string = ?"--bson" || `${dirname(json)}/{basename(json)}.bson`;

json
:> readText
:> parseJSON
:> write.bson(file = bson)
;
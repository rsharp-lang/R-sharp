require(JSON);
require(clustering);

setwd(@dir);

let rawdata = read.csv("./feature_regions.csv", row.names = 1, check.names = FALSE);
let tracer = "./traceback.json"
|> readText()
|> JSON::json_decode()
|> getTraceback()
;

rawdata[, "Cluster"] = NULL;

str(tracer);

print(rawdata);
require(clustering);
require(JSON);

setwd(@dir);

let rawdata = read.csv("./feature_regions.csv", row.names = 1, check.names = FALSE);

rawdata[, "Cluster"] = NULL;
rawdata[, "x"] = as.numeric(rawdata$x);
rawdata[, "y"] = as.numeric(rawdata$y);

print(rawdata, max.print = 13);

let result2 = kmeans(rawdata, centers = 18, bisecting = TRUE, traceback = TRUE);
let debug = getTraceback() |> as.list();

print("kmeans clustering algorithm traceback:");
str(debug);

debug 
|> JSON::json_encode()
|> writeLines(con = "./traceback.json")
;
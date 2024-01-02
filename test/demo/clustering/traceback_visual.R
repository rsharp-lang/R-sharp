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
rawdata[, "x"] = as.numeric(rawdata$x);
rawdata[, "y"] = as.numeric(rawdata$y);

print(rawdata, max.print = 13);
# str(traceback);

# const v = function(id, offset) {
#     let v = traceback[[id]];
#     v[offset];
# }
const x = rawdata$x;
const y = rawdata$y;

for(i in 1:18) {
    let labels = tracer(i, sort = rownames(rawdata));

    print(labels);

    bitmap(file = `./traceback/${str_pad(i,3, pad = "0")}.png`) {
        plot(x, y, 
            class     = labels, 
            grid.fill = "white",
            padding   = "padding: 125px 300px 200px 200px;",
            colorSet  = "paper"
        );
    }
}
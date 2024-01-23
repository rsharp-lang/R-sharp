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

let score = silhouette_score(rawdata, traceback = tracer);

print(score);

write.csv(score, file = `${@dir}/traceback_scores.csv`);

for(name in colnames(score)) {
    bitmap(file = `./silhouette_score/${name}.png`) {
        plot(score[, name],  
            grid.fill = "white",
            padding   = "padding: 125px 300px 200px 200px;"            
        );
    }
}
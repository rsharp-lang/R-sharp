require(REnv);
imports "clustering" from "MLkit";

setwd(@dir);

bezdekIris = read.csv(file = system.file("data/bezdekIris.csv", package = "REnv"), row.names = "class");
rownames(bezdekIris) = unique.names(rownames(bezdekIris));

print(bezdekIris, max.print = 6);

let gmm = gmm(bezdekIris, 3);

print(gmm.predict_proba(gmm), max.print = 13);

let x = gmm.predict_proba(gmm);

write.csv(x, file = "./cluster-bezdekIris.csv", row.names = TRUE);

# require(ggplot);

# bitmap(file = "./cluster-bezdekIris.png") {
#     let xy = rownames(x) |> strsplit(",") |> lapply(i -> as.numeric(i));
#     let xi = sapply(xy, i -> i[1]);
#     let yi = sapply(xy, i -> i[2]); 

#     plot(xi, yi, class = x$max);
# }

# bitmap(file = "./cluster-bezdekIris-scatter.png") {
#     plot(sx, sy, class = x$max);
# }
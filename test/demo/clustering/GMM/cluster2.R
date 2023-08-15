imports "clustering" from "MLkit";

setwd(@dir);

let x = read.csv("../feature_regions.csv", row.names = 1, check.names = FALSE);
let sx = as.numeric(x$x);
let sy = as.numeric(x$y);

x[,"Cluster"] = NULL;

print(x);

set.seed(1);

let gmm = gmm(x, 4);

print(gmm.predict_proba(gmm));

x = gmm.predict_proba(gmm);

write.csv(x, file = "./cluster2.csv", row.names = TRUE);

require(ggplot);

bitmap(file = "./cluster2.png") {
    let xy = rownames(x) |> strsplit(",") |> lapply(i -> as.numeric(i));
    let xi = sapply(xy, i -> i[1]);
    let yi = sapply(xy, i -> i[2]); 

    plot(xi, yi, class = x$max);
}

bitmap(file = "./cluster2-scatter.png") {
    plot(sx, sy, class = x$max);
}
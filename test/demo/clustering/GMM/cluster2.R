imports "clustering" from "MLkit";

setwd(@dir);

let x = read.csv("../feature_regions.csv", row.names = 1, check.names = FALSE);

x[,"Cluster"] = NULL;

print(x);

let gmm = gmm(x, 5);

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

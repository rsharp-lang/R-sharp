setwd(@dir);

options(n_thread = 8);

let rawdata = read.csv("D:\GCModeller\src\R-sharp\REnv\data\wine.csv", row.names = 1, check.names = FALSE);
let pca = prcomp(rawdata, pc = 3);

print(pca);

require(ggplot);

write.csv(pca$score, file = "./wine_PCA_score.csv", row.names = TRUE);

bitmap(file = "./wine_PCA.png") {
    ggplot(pca$score, aes(x="PC1", y = "PC2", label = rownames(pca$score)))
    + geom_point(
        size = 9
    )
    + geom_text()
    ;
}


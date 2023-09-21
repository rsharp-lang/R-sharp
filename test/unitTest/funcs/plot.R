setwd(@dir);

let pca_score = read.csv("./wine_PCA_score.csv", row.names = 1, check.names = FALSE);

require(ggplot);

bitmap(file = "./wine_PCA.png") {
    ggplot(pca_score, aes(x="PC1", y = "PC2", label = rownames(pca_score)))
    + geom_point(
        size = 9
    )
    + geom_text()
    ;
}

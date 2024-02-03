require(ggplot);

setwd(@dir);

let pca_score = read.csv("./wine_PCA_score.csv", row.names = 1, check.names = FALSE);


print(pca_score);

pca_score[, "class"] = unlist($"class\d+"(rownames(pca_score)));

bitmap(file = "./wine_PCA.png", size = [3100, 2400]) {
    ggplot(pca_score, aes(x="PC1", y = "PC2", color = "class", label = rownames(pca_score)))
    + geom_point(
        size = 9
    )
    + geom_text(size = 6)
    + stat_ellipse()
    ;
}

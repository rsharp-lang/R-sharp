require(ggplot);

setwd(@dir);

let pca_score = read.csv("./wine_PCA_score.csv", row.names = 1, check.names = FALSE);


print(pca_score);

pca_score[, "class"] = unlist($"class\d+"(rownames(pca_score)));

bitmap(file = "./wine_PCA.png", size = [3100, 2400]) {
    ggplot(pca_score, aes(x="PC1", y = "PC2", color = "class", label = rownames(pca_score)), 
                padding = [100 300 200 200])
    + stat_ellipse()
    + geom_point(
        size = 20
    )
    + geom_text(size = 14, check_overlap = FALSE)    
    ;
}

svg(file = "./wine_PCA.svg", size = [3100, 2400]) {
    ggplot(pca_score, aes(x="PC1", y = "PC2", color = "class", label = rownames(pca_score)))
    + geom_point(
        size = 20
    )
    + geom_text(size = 14)
    + stat_ellipse()
    ;
}

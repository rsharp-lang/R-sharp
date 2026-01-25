require(REnv);

let pca = function(df, dirsave = "./") {
    library(ggplot2);
    library(dplyr);
    library(tidyverse);
    library(ggthemes);
    library(ggsignif);  # 用于添加显著性标记
    library(rstatix);   # 用于添加统计检验结果
    library(showtext);

    # fix for the zh-cn chars display problem in pdf drawing
    showtext_auto(enable = TRUE);

    # ====== PCA计算核心步骤 ======
    # 1. 数据预处理：分离分类标签和数值数据
    let class_labels <- df$class;  # 保存分类标签
    let pca_data <- select(df, -class);  # 移除分类标签列

    # 2. 数据标准化（重要步骤）
    let scaled_data <- scale(pca_data);  # 中心化+标准化

    # 3. 执行PCA分析
    let pca_result <- prcomp(scaled_data, center = TRUE, scale. = TRUE);

    # 4. 提取主成分得分和方差解释率
    let pc_scores <- as.data.frame(pca_result$x[, 1:2]);  # 取前两个主成分

    pc_scores$class <- class_labels;  # 添加分类标签
    let variance <- summary(pca_result)$importance[2, 1:2] * 100;  # 计算方差贡献率

    # ====== 可视化 ======
    # 5. 绘制PCA得分图
    ggplot(pc_scores, aes(x = PC1, y = PC2, color = class)) +
        geom_point(size = 3, alpha = 0.8) +  # 绘制样本点
        stat_ellipse(level = 0.95, linewidth = 1) +  # 添加95%置信椭圆
        labs(title = "PCA Score Plot",
            x = paste0("PC1 (", round(variance[1], 1), "%)"),
            y = paste0("PC2 (", round(variance[2], 1), "%)")) +
        scale_color_manual(values = c("#E41A1C", "#377EB8", "#4DAF4A")) +  # 自定义颜色
        theme_minimal(base_size = 12) +
        theme(
            panel.grid.major = element_line(color = "grey90", linewidth = 0.25),
            panel.grid.minor = element_blank(),
            panel.border = element_rect(fill = NA, color = "black", linewidth = 0.8),
            legend.position = "right",
            plot.title = element_text(hjust = 0.5, face = "bold")
        ) +
        geom_hline(yintercept = 0, linetype = "dashed", color = "grey50") +
        geom_vline(xintercept = 0, linetype = "dashed", color = "grey50")
        ;

    dir.create(dirsave);

    # 6. 保存结果（可选）
    ggsave(file.path(dirsave, "PCA_plot.png"), width = 4, height = 3, dpi = 300);
    ggsave(file.path(dirsave, "PCA_plot.pdf"), width = 8, height = 6);

    writeLines(as.character( variance), con = file.path(dirsave,"PCA_importance.txt"));
    write.csv(pc_scores, file = file.path(dirsave,"PCA_scores.csv"));
}

data(bezdekIris);

native_r(pca, list(df = bezdekIris, dirsave = relative_work()));
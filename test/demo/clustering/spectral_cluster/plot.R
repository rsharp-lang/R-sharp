bitmap(file = file.path(@dir, "plot_clusters.png"), fill = "white") {
    let data = read.csv(file.path(@dir, "spirals_out.csv"), row.names = NULL);

    plot(as.numeric(data$x),as.numeric(data$y),class = data$cluster_id);
}
setwd(@dir);

let data = read.csv("./embedding.csv", row.names = 1, check.names = FALSE);
let class_id = rownames(data);

class_id = strsplit(class_id, "-", fixed = TRUE);

data[, "class"] = class_id@{1};

print(data);

bitmap(file = "./embedding.png") {
    plot(as.numeric(data$dim_1), as.numeric(data$dim_2), 
        colors = "paper", class = class_id);
}
setwd(@dir);

let data = read.csv("./embedding.csv", row.names = 1, check.names = FALSE);
let class_id = rownames(data);

class_id = strsplit(class_id, "-", fixed = TRUE);
class_id = class_id@{1};
class_id = `class_${class_id}`;

data[, "class"] = class_id;

print(data);

bitmap(file = "./embedding.png") {
    plot(as.numeric(data$dim_1), as.numeric(data$dim_2), 
        colors = "paper", class = class_id);
}
require(JSON);
require(charts);

setwd(@dir);

let scatter = "./region_spots.json"
|> readText()
|> JSON::json_decode()
;

str(scatter);

let x = [];
let y = [];
let class = [];
let xy = [];

for(tag in names(scatter)) {
    xy = scatter[[tag]];
    class = append(class, rep(tag, length(xy)));
    xy = strsplit(xy, ",");
    x = append(x, sapply(xy, i -> i[1]));
    y = append(y, sapply(xy, i -> i[2]));

    str(tag);
}

print(x);
print(y);
print(class);

bitmap(file = "./Rplot.png", size = [3000, 3600], dpi = 300) {
plot(as.numeric(x), y= as.numeric(y), class = class, point.size = 17, 
shape = "rect",reverse = TRUE, colorSet = "paper", grid.fill = "white" ,
# xlim = 256, ylim = 373, 
padding = "padding: 150px 450px 250px 300px;"
);
}







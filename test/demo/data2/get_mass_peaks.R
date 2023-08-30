setwd(@dir);

let data = read.csv("./mass_index.csv", row.names = "name", check.names = FALSE);

print(data);

imports "clustering" from "MLkit";

print(sort(data$rt));

let gmz = gmm(data$rt, components = 20);
setwd(@dir);

let empty = read.csv("./empty_table.csv", row.names = NULL);

print(empty);

empty = as.list(empty, byrow = TRUE);

str(empty);

for(i in tqdm(empty)) {
    str(i);
}
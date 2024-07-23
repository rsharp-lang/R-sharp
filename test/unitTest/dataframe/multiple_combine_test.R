let a = data.frame(a = 1:3, b = TRUE, c = "aaaaa");
let b = data.frame(a = 5:9, b = [T T T F F], c = "ccccc");
let c = data.frame(a = c(6,6,6), b = FALSE, c = ["999" "gggg" "ssss"]);

print(bind_rows(a,b,c, .id = "a"));

let list = list(a,b,c);

print(bind_rows(list, .id = "a"));
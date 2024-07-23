let a = data.frame(a = 1:3, b = TRUE, c = "aaaaa", d= 1);
let b = data.frame(a = 5:9, b = [TRUE TRUE TRUE FALSE FALSE], c = "ccccc", d= 2);
let c = data.frame(a = c(6,6,6), b = FALSE, c = ["999" "gggg" "ssss"]);

print(bind_rows(a,b,c, .id = "a"));

let list = list(a,b,c);

print(bind_rows(list, .id = "a"));
# create two data sequence object
let x = 1:100;
let word = ["world", "R#"];
let l = list(a = [1,2,3,4,5], b = FALSE, c = ["hello", "world"]);

# and then save these two sequence object to disk file
save(x, word, l, file = "./R#save.rda");
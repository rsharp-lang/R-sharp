# create two data sequence object
let x = 1:100;
let word = ["world", "R#"];

# and then save these two sequence object to disk file
save(x, word, file = "./R#save.rda");
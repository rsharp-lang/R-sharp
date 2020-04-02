setwd(!script$dir);

# create two data sequence object
let x = 1:100;
let word = ["world", "R#"];
let l = list();
let d = data.frame();

d[, "a"] <- 9;
d[, "b"] <- [FALSE,FALSE,FALSE,TRUE,TRUE,TRUE];

l$a = [1,2,3,4,5];
l$b = FALSE;
l$c = ["hello", "world"];

# and then save these two sequence object to disk file
save(x, word, l, d, file = "./R#save.rda");
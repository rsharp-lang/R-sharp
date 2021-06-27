let a = [1,2,3,4,4,4,33334];
let b = [TRUE,TRUE,TRUE,TRUE,TRUE,FALSE,TRUE];
let C = "single string";

let d = data.frame(a1 = (a + 6) / 2, cc =b, dd = C, index = 1:length(a));

rownames(d) = ["a","b","c","d","e","f","g"];

print(d);

let l = d[2, , drop = TRUE];

str(l);

str(d["b",, drop = TRUE]);

let echo = x -> print(x);

d :> echo;

write.csv(d, file = `${!script$dir}/dataframe.csv`, row.names = FALSE);
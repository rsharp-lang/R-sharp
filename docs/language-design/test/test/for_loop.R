let x <- {};

for(i in 1:100) {
    x.append(i);
}

let y <- {};

for(i in [80:100,0.25]) {
   y.append(i);
}

str(x);
# integer [1:100] 1 2 3 4 5 6 7 8 9 10 ...

let g <- which x in y;

str(g);
# integer [1:21] 80 81 82 83 84 85 86 87 88 89 ...

g <- which not x in [10:100];
# integer [1:9] 1 2 3 4 5 6 7 8 9

create.table <- function(min as integer, max as double) {
    var x <- [min:max,0.1];
    var y <- x ^ 2;
    var z <- -y;
    
    return data.frame(x, y, z);
}

for([a as "x", b as "y", z] in create.table(1, 5)) {
    print({a, b, z} | average);
}

let [x1, y1, c1 as "z"] <- create.table(1, 5);

str(x1);
str(y1);
str(c1);

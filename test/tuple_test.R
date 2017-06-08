tuple.create <- function(a, b, c) {
    return [a:ToString("F5"), b, c, {a, b, c} | sum];
}

var x <- tuple.create(1, 2, 3);

x$X1;
# [1] 1.00000

x$X2;
# [1] 2

x$X3;
# [1] 3

x$X4:ToString("F2");
# [1] 6.00

var [i, j, c, s] <- tuple.create(2, 3, 3);

i;
# [1] 2.00000

j;
# [1] 3

c;
# [1] 3

str(s);
# numeric 8

var obj <- list() with {
    $a <- 123;
    $b <- "666";
}

str(obj);
# List of 2
#  $a: integer 123
#  $b: string "666"

var [int, text] <- obj;

str(int);
# integer 123

str(text);
# string "666"

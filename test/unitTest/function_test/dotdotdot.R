let f1 <- function(x, ...) {
  f2(...);
  str(names(list(...)));
}

let f2 <- function(y) {
  print(y);
}

f1(55, z = 1, y = 2);
# [1] 2
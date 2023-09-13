let f1 <- function(x, ...) {
  f2(...);
}

let f2 <- function(y) {
  print(y);
}

f1(x = 1, y = 2);
# [1] 2
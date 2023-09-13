let f1 <- function(x, ...) {
  f2(...);
  str(names(list(...)));
  str(list(...));
}

let f2 <- function(y) {
  print(y);
}

f1(55, z = 1, y = 2);
# [1]     2
# chr [1:2] "z" "y"
# List of 2
#  $ z : int 1
#  $ y : int 2
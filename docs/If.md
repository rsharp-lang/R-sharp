## ``=>`` If shortcut

```R
# if (TRUE) action()
TRUE => action();

var ensure.arguments <- function(args) {

	if(empty(args)) args <- list();

    empty(args$param1) => args$param1 <- default1;
    empty(args$param2) => args$param2 <- default2;
    empty(args$param3) => args$param3 <- default3;
    empty(args$param4) => args$param4 <- default4;
    empty(args$param5) => args$param5 <- default5;

	args;
}
```
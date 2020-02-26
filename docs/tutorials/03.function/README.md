# Declare ``R#`` function

## Declare a function in R

```R
a <- function(...) {
   # ...
}
```

## Declare a function in ``R#``

```R
let a <- function(...) {

}

let a as function(...) {

}
```

## anonymous function in ``R#``

```R
a <- function(...) {
    # ...
}

a <- x => ...;
```

### difference between lambda function and the closure function
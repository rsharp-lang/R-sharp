# Declare ``R#`` function

## Declare a function in R

The function is kind of data type in R language, so you can declare a new function by assign the function object value to a symbol in R language, example as:

```R
a <- function(...) {
   # ...
}
```

But due to the reason of one symbol can not have multiple value at the same time, so that you can not doing the function overloads in R language like other language it does.

## Declare a function in ``R#``

The function is also a kind of data type in ``R#`` language, so you can declare a new function by assign the function object value to a symbol in ``R#`` language, example as:

```R
let a <- function(...) {
    # assign the function object instance
    # to a specific symbol
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
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
let a <- function(x) {
    # assign the function object instance
    # to a specific symbol
    x + 1;
}

# a(1);
# [1] 2

let a as function(x) {
    # declare a new function object instance
    x + 1;
}

# a(1);
# [1] 2
```

The two function declaration demo that represent above have the same effect about declare a function. But there is a little difference between these two demo:

1. In first demo, we declare a new anonymous function and then assign its object instance to a symbol which is named ``a``. If we print the value of the ``a`` symbol, then we could found that the function name is strange liked ``<anonymous...>``.

2. In the second demo code, we declare a new function object instance which it have a symbol name ``a``. If we print the value of the ``a`` symbol, then we could found that the function name is the symbol name ``a``.

## anonymous function in ``R#``

```R
a <- function(...) {
    # ...
}

a <- x => ...;
```

### difference between lambda function and the closure function
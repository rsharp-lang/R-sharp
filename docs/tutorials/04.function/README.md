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

The lambda function object it contains no environment information, so it just some kind of ``formula`` to finish some kind of job. here is a example for examine the difference between the lambda function and the closure function:

```R
let y = 99;
let createLambda as function() {
    # due to the reason of lambda function contains no 
    # associated environment information, so that the 
    # non-paramenter symbol y its value is depends of 
    # the context when we call this lambda function
    x -> x + y + 1;
} 
let createClosure as function(y) {
    function(x) x + y + 1;
}

let fun1 = createLambda();
let fun2 = createClosure(-99); 

fun1(0);
# [1] 100
fun2(0);
# [1] -98
```

due to the reason of lambda function contains no associated environment information, so that the non-paramenter symbol y its value is depends of the context when we call this lambda function. so the ``y`` value in ``fun1`` is reference to the symbol ``y`` in the current global environment context, so its value expression is ``0 + 99 + 1 = 100``. for function ``fun2``, its ``y`` value is reference to the inner closure environment of the ``createClosure`` function instance, so that as we invoke the ``createClosure`` with y parameter value -99, not the y symbol in global environment context, so its value expression is ``0 + -99 + 1 = -98``. 
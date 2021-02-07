# Special Closures in R#

## introduction: closure expression

A closure expression is a kind of expression group that could produce values, here is a closure expression example:

```R
{
    function1();
    function2();
    function3();
}
```

As the result value of the closure expression is the value of the last expression, so you can assign the closure its expression value to a ``R#`` symbol like:

```R
# you can apply the closure expression syntax for divide your code 
# into different code block in a more logical way. 
let symbol as string = {
    function1();
    function2();

    # the result value of the function3 invocation will be 
    # assigned to the target symbol.
    function3();
}
```

> NOTE: please note that, the closure is an expression, even if the closure body it contains multiple expressions.

There are sevral special closure types in R# language

## ``using`` closure

The ``using`` closure can apply for the automatically operation like write file, send data automatically when finish the operations, example as: 

```R
using file as data.frame() :> auto(table -> table :> write.csv(file = "...")) {
    # codes for modify the file data frame object
    # ...
}
```

if we restore the the using closure code as the original R code, then you can found out the secret of how we implements the ``using`` closure:

```R
let file = data.frame();

# codes for modify the file data frame object
# ...

write.csv(file, file = "...")
```

## ``acceptor`` closure

The ``acceptor`` closure is a kind of extension function syntax. By using the acceptor closure syntax that we can divide the code into different code block in a more logical way. For example, draw bitmap in the original R language syntax could be:

```R
# code for charting...
plot(...)
# more charting code 
# ...
:> bitmap(...);
```

But with the new ``acceptor`` closure syntax, that you can divide your code into blocks by different logical function implementation, example like: 

```R
bitmap(...) {
    # code for charting...
    # ...
    plot(...);
}
```

the ``acceptor`` closure syntax is a kind of syntax variation of extension function, example like the extension function:

```R
# the pipeline code produce the input table value for write.csv function
get_source
:> func1()
:> func2()
:> func3()
:> func4()
:> write.csv(file = "...");
```

the extension function syntax code that show above is equivalent to the ``acceptor`` syntax, example like:

```R
write.csv(file = "...") {
    # the pipeline code produce the input table value for write.csv function
    get_source
    :> func1()
    :> func2()
    :> func3()
    :> func4();
}
```

if we rewrite the above code into the original R syntax, then you will find the secret of how to implements such so called ``acceptor`` syntax:

```R
write.csv({
    # the pipeline code produce the input table value for write.csv function
    get_source
    :> func1()
    :> func2()
    :> func3()
    :> func4();
}, file = "...");
```
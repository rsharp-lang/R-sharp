# Special Closures in R#

There are sevral closure type in R# language

## ``using`` closure

The ``using`` closure can apply for the automatically operation like write file, send data automatically when finish the operations, example as: 

```R
using file as data.frame() :> auto(table -> table :> write.csv(file = "...")) {

}
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
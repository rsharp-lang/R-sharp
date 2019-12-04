# R# programming language

> The ``R# language`` its language syntax is derived from the ``R language``.

## Hello world!

For create a new kind of elegant R programming language, the resulting R# its language syntax is comes from the R language hybrid with VisualBasic.NET language and TypeScript language. 

```R
# declare a variable
# please notice that, the R# language is a kind of vectorization programming
# language, all of the primitive type in R# is a vector
# So that the data type 'string' means a string vector in R#, not only a single 
# string in VB.NET or typescript language.
let word as string = ['world', 'R# user', 'GCModeller user'];

# declare a function
let echo as function(words) {
    print( `Hello ${ words }!` );
}

# or declare a lambda function
let echo.lambda = words -> print( `Hello ${ words }!` );

# and then invoke function via pipeline operator
word :> echo;
# [3] "Hello world!" "Hello R# user!" "Hello GCModeller user!"
word :> echo.lambda;
# [3] "Hello world!" "Hello R# user!" "Hello GCModeller user!"
```


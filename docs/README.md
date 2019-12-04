# R# programming language

<img src="images/R-sharp.png" width="450px" />

> Art work: http://www.clipartbest.com/clipart-di85MqodT

## Introduction to R

R# is a language and environment for GCModeller scripting and data science chart plot graphics. It is an Open source project which is similar to the R language and environment which was developed at R&amp;D laboratory from BioNovogene corporation by Xie.Guigang. The ``R# language`` its language syntax is derived from the ``R language``, and R# can be considered as a part of implementation of R on ``Microsoft .NET Framework`` environment. Although there are too many important differences between R# and R, but much code written for R could runs unaltered under R#.

Unlike the R Project, R# language is not focus on the statistical computing, R# try to combine the numeric computing with the .NET library programming which is comes from the GCModeller on the contrary. So in this way, R# provides a wide variety of bioinformatics analysis toolkit from GCModeller and graphical techniques, and is highly extensible.

## Hello world!

For create a new kind of elegant R programming language, the resulting R# its language syntax is comes from the R language hybrid with VisualBasic.NET language and TypeScript language. Here is a R# demo code example for say hello world as routine:

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

1. For learning more about the R# programming language please read this help document: The *[&lt;R# language design>](language-design/language-design.md)* document.
2. Learn details information about the R# environment development, you could read this document: The *[&lt;R# system>](R-system/)*.
3. Learn R# with tutorials code at here: The *[&lt;R# Tutorials>](tutorials)*.
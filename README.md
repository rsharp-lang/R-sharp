> [WARNING] This project is a work in progress and is not recommended for production use.

<img src="docs/images/R-sharp.png" width="450px" />

> Art work: http://www.clipartbest.com/clipart-di85MqodT

> The latest [sciBASIC.NET Framework](https://github.com/xieguigang/sciBASIC) runtime is also required

The ``R#`` language its syntax is original derived from the ``R`` language, but with more modernized programming styles. The ``R#`` language its interpreter and .NET compiler is original writen in VisualBasic language, with native support for the .NET runtime.

The ``R#`` language is not designed for the general data analysis purpose, but it is specialize designed for my works in the company, implements the bioinformatics data analysis system based on the GCModeller platform, for building the bioinformatics data science stack with R and VisualBasic language.

#### Directory structure

+ [``R#``](./R#) The R# language core runtime and scripting engine
+ [``Library``](./Library) The fundation library in R# scripting system
+ [``Rscript``](./Rscript) The R# scripting host
+ [``R-terminal``](./studio/R-terminal) The R# shell program  
+ [``Rsharp_kit``](./studio/Rsharp_kit) The R-sharp toolkit

#### Demo R# code

```R
# declare a variable
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

Used in VisualBasic.NET programming:

```vbnet
Dim R As New RInterpreter()

' Run script by invoke method
Call R.Evaluate("
    # test script
    let word as string = ['world', 'R# user', 'GCModeller user'];
    let echo as function(words) {
        print( `Hello ${ words }!` );
    }

    word :> echo;
")

' or assign variable
Call R.Add("word", {"world", "R# user", "GCModeller user"})

' then declare R function throught script
Call R.Add("echo", 
    Function(words As String()) As String()
        Return Internal.print(words)
    End Function)

' at last, invoke R function throught Invoke method
Call R.Invoke("echo", R!word)
```

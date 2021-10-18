# ``R#`` language

R# language is a kind of R liked language implements on .NET environment for GCModeller scripting.

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

## Related ``R#`` Package Resource

Packages that developed for the R# programming environment:

+ [ggplot](https://github.com/rsharp-lang/ggplot) package is a R environment ggplot2 package liked data visualization package for R# language.  
+ [mzkit](https://github.com/xieguigang/mzkit) is a project developed for R# language for run data analysis of the mass spectrum raw data.
+ [ms-imaging](https://github.com/xieguigang/ms-imaging) is a R# package for rendering the MSImaging based on the libraries from mzkit and ggplot packages.


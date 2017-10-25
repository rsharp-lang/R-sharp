By default, one ``R#`` script equivalent to a small program in function **main**, like:

example R script file:

```R
# program.R

var s as string <- "Hello world!";
var expand <- function() {
    s <- "$s message from R#";
}

expand();
println(s);
# Hello world! message from R#
```

equivalent to the VisualBasic program:

```vbnet
' program.vb

Module Program

    Sub Main()
    
        Dim s As String = "Hello world!"
        Dim expand = Function()
                         s = $"{s} message from VisualBasic"
                     End Function
        ' Hello world! message from VisualBasic
        Call expand()
        Call println(s)
    End Sub
End Module
```

The you can using the ``R#`` compiler for compile you R script as a standard .NET program, using commandline:

```bash
"/home/xieguigang/Rsharp/rc.exe" /compile --file "/home/xieguigang/test/program.R" --out "./hello.exe"

./hello.exe
# Hello world! message from R#
```

## How it works?

```
R# script -> Syntax Tree -> VisualBasic CodeDOM -> .NET assembly
```

### Solving name conflicts

### Solving closure value

### 
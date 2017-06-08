By default, one ``R#`` script equivalent to a small program in sub **main**, like:

example R script file:

```R
# program.R

var s as string <- "Hello world!";
var expand <- function() {
    s <- "{s} message from R#";
}

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
        Call println(s)
    End Sub
End Module
```

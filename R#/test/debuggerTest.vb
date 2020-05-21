Imports SMRUCC.Rsharp.Interpreter

Module debuggerTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()
        R.Evaluate("imports 'diagnostics' from 'Rstudio';")
        R.Evaluate(" view(list(a=1,b=[1,2,3,4]))")

        Pause()
    End Sub
End Module

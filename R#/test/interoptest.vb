Imports SMRUCC.Rsharp.Interpreter

Module interoptest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()

        Call R.Add("x", New TestContainer)
        Call R.Evaluate("x <- as.object(x)")
        Call R.Print("x")


        Pause()
    End Sub
End Module

Public Class TestContainer

End Class
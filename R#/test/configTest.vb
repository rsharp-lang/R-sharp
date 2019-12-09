Imports SMRUCC.Rsharp.Interpreter

Module configTest

    Dim R As New RInterpreter

    Sub Main()
        Call printTest()
    End Sub

    Sub printTest()

        Call R.Evaluate("options(max.print = 100)")
        Call R.Print("getOption(""max.print"")")
        Call R.Print("options([""max.print"", ""lib""])")
        Call R.Print("1:500 step 0.125")

        Pause()
    End Sub
End Module

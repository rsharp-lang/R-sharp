Imports SMRUCC.Rsharp.Interpreter

Module interpreterTest

    Dim R As New RInterpreter

    Sub Main()

        Call R.Evaluate("let x as integer = [999, 888, 777, 666];")

        Pause()
    End Sub
End Module

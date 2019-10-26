Imports SMRUCC.Rsharp.Interpreter

Module interpreterTest

    Dim R As New RInterpreter

    Sub Main()

        Call R.Evaluate("let x as integer = [999, 888, 777, 666];")
        Call R.Evaluate("let y as integer = $;")


        Call R.Evaluate("x <- 1:99,1.5;")
        Call R.Evaluate("let str as  string =`hello world!`;")


        Pause()
    End Sub
End Module

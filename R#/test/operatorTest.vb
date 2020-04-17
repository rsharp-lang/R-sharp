Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Module operatorTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()

        R.Add("x", {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, TypeCodes.integer)
        R.Add("y", {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, TypeCodes.integer)

        R.Print("x+y")
        R.Print("x *2")
        R.Print("x*2.0")
        R.Print("typeof (1+1)")
        R.Print("typeof (x*2.0)")

        R.Print("x %y")

        R.Print("TRUE || ![TRUE, FALSE,FALSE,FALSE,TRUE]")

        Pause()
    End Sub
End Module

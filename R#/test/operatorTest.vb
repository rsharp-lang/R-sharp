Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Module operatorTest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()

        R.Add("x", {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, TypeCodes.integer)
        R.Add("y", {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, TypeCodes.integer)

        R.Print("x+y")
        R.Print("x *2")


        Pause()
    End Sub
End Module

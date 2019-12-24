Imports SMRUCC.Rsharp.Interpreter

Module interoptest

    Dim R As New RInterpreter With {.debug = False}

    Sub Main()

        Call R.Add("x", New TestContainer)
        Call R.Evaluate("x <- as.object(x)")
        Call R.Print("x")

        Call R.Evaluate("x$name <- '9999'")
        Call R.Print("`name value of x is ${x$name}.`")
        Call R.Evaluate("x[['name']] <- '8848'")
        Call R.Print("`name value of x is ${x$name}.`")
        Call R.Print("x")

        ' Call R.Print("do.call")
        Call R.Print("x$setName")

        Call R.Evaluate("x :> do.call(calls = 'setName', newName = 'ABCCCCD')")
        Call R.Print("`name value of x is ${x[['name']]}.`")

        Pause()
    End Sub
End Module

Public Class TestContainer

    Public Property name As String

    ''' <summary>
    ''' # Test for R# interop
    ''' </summary>
    ''' <param name="newName">function parameter for set name property value</param>
    ''' <returns>The new name</returns>
    Public Function setName(newName As String) As String
        name = newName
        Return newName
    End Function

    Public Overrides Function ToString() As String
        Return $"Container: {name}"
    End Function

End Class
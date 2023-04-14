Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Public Module convertTest

    Dim ints As Integer() = Enumerable.Range(0, 10000).ToArray
    Dim intList As list = New list With {.slots = ints.ToDictionary(Function(i) i.ToString, Function(i) CObj(i))}
    Dim intStr As String() = ints.Select(Function(a) a.ToString).ToArray

    Sub Main()
        Dim env As New RInterpreter

        Call env.Print(ints)
        Call env.Inspect(intList)
        Call env.Print(intStr)

        Pause()
    End Sub
End Module

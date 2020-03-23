Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter

<Package("makeObject")>
Module makeObjecttest

    Dim R As New RInterpreter With {.debug = True}

    Sub Main()
        Call R.LoadLibrary(GetType(makeObjecttest))

        Call R.Print("tuple")
        Call R.Evaluate("tuple(list(a=1,b= '99999', c=TRUE,d = [599,3.3,9987.01]))")

        Pause()
    End Sub

    <ExportAPI("debug_echo")>
    Public Sub createArguments(args As arguments)
        Call Console.WriteLine(args.GetJson)
        Call Console.WriteLine(args.GetType.FullName)
    End Sub

    <ExportAPI("tuple")>
    Public Sub tupleTest(x As (a As Integer, b As String(), c As Boolean, d As Double()))
        Call Console.WriteLine(x.a)
        Call Console.WriteLine(x.b.GetJson)
        Call Console.WriteLine(x.c)
        Call Console.WriteLine(x.d.GetJson)
    End Sub
End Module

Public Class arguments
    Public Property a As String
    Public Property b As Integer()
    Public Property c As Boolean()
End Class
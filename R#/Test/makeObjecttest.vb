Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON

<Package("makeObject")>
Module makeObjecttest

    <ExportAPI("debug_echo")>
    Public Sub createArguments(args As arguments)
        Call Console.WriteLine(args.GetJson)
    End Sub
End Module

Public Class arguments
    Public Property a As String
    Public Property b As Integer()
    Public Property c As Boolean()
End Class
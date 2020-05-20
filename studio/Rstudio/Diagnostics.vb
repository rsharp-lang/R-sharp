Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("diagnostics")>
Module Diagnostics

    <ExportAPI("view")>
    Public Sub view(symbol As Object, Optional env As Environment = Nothing)
        Call env.globalEnvironment.stdout.Write(JSONSerializer.GetJson(symbol.GetType(), symbol, New JSONSerializerOptions), "application/json")
    End Sub

    <ExportAPI("help")>
    Public Function help(symbol As Object, Optional env As Environment = Nothing) As Message
        If TypeOf symbol Is String Then
            symbol = env.FindSymbol(symbol)?.value
        End If

        If symbol Is Nothing Then
            Return debug.stop("symbol object can not be nothing!", env)
        ElseIf Not TypeOf symbol Is RMethodInfo Then
            Return debug.stop("unsupport symbol object type!", env)
        End If


    End Function
End Module

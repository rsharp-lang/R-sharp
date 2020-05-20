Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

<Package("debug")>
Module debugger

    <ExportAPI("view")>
    Public Sub view(symbol As Object, Optional env As Environment = Nothing)

    End Sub
End Module

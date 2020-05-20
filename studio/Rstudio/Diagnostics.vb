Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

<Package("diagnostics")>
Module Diagnostics

    <ExportAPI("view")>
    Public Sub view(symbol As Object, Optional env As Environment = Nothing)

    End Sub
End Module

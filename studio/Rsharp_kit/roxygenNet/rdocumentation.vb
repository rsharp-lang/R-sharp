
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface

<Package("rdocumentation")>
Public Module rdocumentation

    <ExportAPI("documentation")>
    Public Function rdocumentation(func As RFunction, Optional env As Environment = Nothing) As String
        Return New [function]().createHtml(func, env)
    End Function
End Module

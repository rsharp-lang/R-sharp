
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("hiddenMarkov")>
Module hiddenMarkov

    <ExportAPI("")>
    Public Function statesMatrix(<RRawVectorArgument> matrix As Object, Optional env As Environment = Nothing) As Object

    End Function

End Module

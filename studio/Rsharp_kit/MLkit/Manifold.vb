Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("umap")>
Module Manifold

    Public Function umap(<RRawVectorArgument> data As Object, Optional env As Environment = Nothing) As Object

    End Function

    <ExportAPI("as.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function asGraph(umap As Umap, <RRawVectorArgument> labels As Object,
                            Optional env As Environment = Nothing) As Object

        Dim labelList As String() = REnv.asVector(Of String)(labels)
        Dim g As NetworkGraph = umap.CreateGraph(labelList)

        Return g
    End Function
End Module

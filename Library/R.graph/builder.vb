Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("igraph.builder")>
Module builder

    <ExportAPI("correlation.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function FromCorrelations(x As DistanceMatrix, Optional threshold As Double = 0.65, Optional env As Environment = Nothing) As Object
        If x.is_dist Then
            Return Internal.debug.stop({"it seems that a distance matrix is given, a similarity matrix is required!"}, env)
        Else
            Return x.BuildNetwork(threshold).Item1
        End If
    End Function
End Module

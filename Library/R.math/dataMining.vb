Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("base.dataMining")>
Module dataMining

    Sub New()
        Call REnv.Internal.generic.add("summary", GetType(EntityClusterModel()), AddressOf clusterSummary)
    End Sub

    Public Function clusterSummary(result As Object, args As list, env As Environment) As Object
        If TypeOf result Is EntityClusterModel() Then
            Return DirectCast(result, EntityClusterModel()) _
                .GroupBy(Function(d) d.Cluster) _
                .ToDictionary(Function(d) d.Key,
                              Function(cluster) As Object
                                  Return cluster.Select(Function(d) d.ID).ToArray
                              End Function) _
                .DoCall(Function(slots)
                            Return New list With {
                                .slots = slots
                            }
                        End Function)
        Else
            Throw New NotImplementedException
        End If
    End Function

    <ExportAPI("kmeans")>
    <RApiReturn(GetType(EntityClusterModel()))>
    Public Function Kmeans(<RRawVectorArgument>
                           dataset As Object,
                           Optional expected% = 3,
                           Optional parallel As Boolean = True,
                           Optional debug As Boolean = False,
                           Optional env As Environment = Nothing) As Object

        Dim model As EntityClusterModel()

        If dataset Is Nothing Then
            Return Nothing
        ElseIf dataset.GetType.IsArray Then
            Select Case REnv.MeasureArrayElementType(dataset)
                Case GetType(DataSet)
                    model = DirectCast(REnv.asVector(Of DataSet)(dataset), DataSet()).ToKMeansModels
                Case GetType(EntityClusterModel)
                    model = REnv.asVector(Of EntityClusterModel)(dataset)
                Case Else
                    Return REnv.Internal.debug.stop(New InvalidProgramException(dataset.GetType.FullName), env)
            End Select
        Else
            Return REnv.Internal.debug.stop(New InvalidProgramException(dataset.GetType.FullName), env)
        End If

        Return model.Kmeans(expected, debug, parallel).ToArray
    End Function
End Module

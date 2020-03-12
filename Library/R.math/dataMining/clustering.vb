Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# data clustering tools
''' </summary>
<Package("stats.clustering")>
Module clustering

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

    ''' <summary>
    ''' K-Means Clustering
    ''' </summary>
    ''' <param name="dataset">
    ''' numeric matrix of data, or an object that can be coerced 
    ''' to such a matrix (such as a numeric vector or a data 
    ''' frame with all numeric columns).
    ''' </param>
    ''' <param name="centers">
    ''' either the number of clusters, say k, or a set of initial 
    ''' (distinct) cluster centres. If a number, a random set of 
    ''' (distinct) rows in x is chosen as the initial centres.
    ''' </param>
    ''' <param name="parallel"></param>
    ''' <param name="debug"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("kmeans")>
    <RApiReturn(GetType(EntityClusterModel()))>
    Public Function Kmeans(<RRawVectorArgument>
                           dataset As Object,
                           Optional centers% = 3,
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

        Return model.Kmeans(centers, debug, parallel).ToArray
    End Function
End Module

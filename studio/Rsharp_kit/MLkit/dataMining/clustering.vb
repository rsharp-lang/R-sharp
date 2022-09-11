#Region "Microsoft.VisualBasic::d8a159f0d4f2e614636ee75a004df010, R-sharp\studio\Rsharp_kit\MLkit\dataMining\clustering.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 780
    '    Code Lines: 529
    ' Comment Lines: 165
    '   Blank Lines: 86
    '     File Size: 33.10 KB


    ' Module clustering
    ' 
    '     Function: btreeClusterFUN, clusterGroups, clusterResultDataFrame, clusterSummary, cmeansSummary
    '               dbscan, dbscan_objects, densityA, ensureNotIsDistance, fuzzyCMeans
    '               hclust, hleaf, hnode, Kmeans, showHclust
    '               ToHClust
    ' 
    '     Sub: Main
    '     Class point2DReader
    ' 
    '         Function: activate, getByDimension, GetDimensions, metric, nodeIs
    ' 
    '         Sub: setByDimensin
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.BinaryTree
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.DBSCAN
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Distance = Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy.Distance
Imports Point2D = System.Drawing.Point
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# data clustering tools
''' </summary>
<Package("clustering", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@live.com")>
Module clustering

    Friend Sub Main()
        Call REnv.Internal.generic.add("summary", GetType(EntityClusterModel()), AddressOf clusterSummary)

        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(EntityClusterModel()), AddressOf clusterResultDataFrame)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(FuzzyCMeansEntity()), AddressOf cmeansSummary)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(
            type:=GetType(dbscanResult),
            handler:=Function(result, args, env)
                         Return DirectCast(result, dbscanResult).cluster.clusterResultDataFrame(args, env)
                     End Function)

        Call REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of Cluster)(AddressOf showHclust)
    End Sub

    Private Function showHclust(cluster As Cluster) As String
        Return cluster.ToConsoleLine
    End Function

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

    Public Function cmeansSummary(cmeans As FuzzyCMeansEntity(), args As list, env As Environment) As Rdataframe
        Dim summary As New Rdataframe With {
            .rownames = cmeans.Keys,
            .columns = New Dictionary(Of String, Array) From {
                {"cluster", cmeans.Select(Function(e) e.probablyMembership).ToArray}
            }
        }

        For Each i As Integer In cmeans(Scan0).memberships.Keys
            summary.columns.Add("cluster" & i, cmeans.Select(Function(e) e.memberships(i)).ToArray)
        Next

        Return summary
    End Function

    <Extension>
    Public Function clusterResultDataFrame(data As EntityClusterModel(), args As list, env As Environment) As Rdataframe
        Dim table As File = data.ToCsvDoc
        Dim matrix As New Rdataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        For Each column As String() In table.Columns
            matrix.columns.Add(column(Scan0), column.Skip(1).ToArray)
        Next

        Return matrix
    End Function

    ''' <summary>
    ''' ### the cmeans algorithm module
    ''' 
    ''' **Fuzzy clustering** (also referred to as **soft clustering**) is a form of clustering in which 
    ''' each data point can belong to more than one cluster.
    '''
    ''' Clustering Or cluster analysis involves assigning data points to clusters (also called buckets, 
    ''' bins, Or classes), Or homogeneous classes, such that items in the same class Or cluster are as 
    ''' similar as possible, while items belonging to different classes are as dissimilar as possible. 
    ''' Clusters are identified via similarity measures. These similarity measures include distance, 
    ''' connectivity, And intensity. Different similarity measures may be chosen based on the data Or 
    ''' the application.
    ''' 
    ''' > https://en.wikipedia.org/wiki/Fuzzy_clustering
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <param name="centers"></param>
    ''' <param name="fuzzification"></param>
    ''' <param name="threshold"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("cmeans")>
    <RApiReturn(GetType(Classify))>
    Public Function fuzzyCMeans(<RRawVectorArgument>
                                dataset As Object,
                                Optional centers% = 3,
                                Optional fuzzification# = 2,
                                Optional threshold# = 0.001,
                                Optional env As Environment = Nothing) As Object

        Dim data As pipeline = pipeline.TryCreatePipeline(Of DataSet)(dataset, env)

        If data.isError Then
            Return data.getError
        End If

        Dim raw = data.populates(Of DataSet)(env).ToArray
        Dim propertyNames As String() = raw.PropertyNames
        Dim entities As ClusterEntity() = raw _
            .Select(Function(d)
                        Return New ClusterEntity With {
                            .entityVector = d(propertyNames),
                            .uid = d.ID
                        }
                    End Function) _
            .ToArray
        Dim classes As Classify() = entities.CMeans(
            classCount:=centers,
            fuzzification:=fuzzification,
            threshold:=threshold
        )

        Return classes
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
    <RApiReturn(GetType(EntityClusterModel))>
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
        ElseIf TypeOf dataset Is Rdataframe Then
            Dim colNames As String() = DirectCast(dataset, Rdataframe).columns _
                .Keys _
                .ToArray

            model = DirectCast(dataset, Rdataframe) _
                .forEachRow _
                .Select(Function(r)
                            Return New EntityClusterModel With {
                                .ID = r.name,
                                .Properties = colNames _
                                    .SeqIterator _
                                    .ToDictionary(Function(c) c.value,
                                                  Function(i)
                                                      Return CType(r(i), Double)
                                                  End Function)
                            }
                        End Function) _
                .ToArray
        Else
            Return REnv.Internal.debug.stop(New InvalidProgramException(dataset.GetType.FullName), env)
        End If

        Return model.Kmeans(centers, debug, parallel).ToArray
    End Function

    ''' <summary>
    ''' Hierarchical Clustering
    ''' 
    ''' Hierarchical cluster analysis on a set of dissimilarities and methods for analyzing it.
    ''' </summary>
    ''' <param name="d">a dissimilarity structure as produced by dist.</param>
    ''' <param name="method">
    ''' the agglomeration method to be used. This should be (an unambiguous abbreviation of) 
    ''' one of "ward.D", "ward.D2", "single", "complete", "average" (= UPGMA), "mcquitty" (= WPGMA), 
    ''' "median" (= WPGMC) or "centroid" (= UPGMC).
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' This function performs a hierarchical cluster analysis using a set of dissimilarities for 
    ''' the n objects being clustered. Initially, each object is assigned to its own cluster and 
    ''' then the algorithm proceeds iteratively, at each stage joining the two most similar clusters, 
    ''' continuing until there is just a single cluster. At each stage distances between clusters 
    ''' are recomputed by the Lance–Williams dissimilarity update formula according to the particular 
    ''' clustering method being used.
    '''
    ''' A number Of different clustering methods are provided. Ward's minimum variance method aims 
    ''' at finding compact, spherical clusters. The complete linkage method finds similar clusters. 
    ''' The single linkage method (which is closely related to the minimal spanning tree) adopts a 
    ''' ‘friends of friends’ clustering strategy. The other methods can be regarded as aiming for 
    ''' clusters with characteristics somewhere between the single and complete link methods. 
    ''' Note however, that methods "median" and "centroid" are not leading to a monotone distance 
    ''' measure, or equivalently the resulting dendrograms can have so called inversions or reversals 
    ''' which are hard to interpret, but note the trichotomies in Legendre and Legendre (2012).
    '''
    ''' Two different algorithms are found In the literature For Ward clustering. The one used by 
    ''' Option "ward.D" (equivalent To the only Ward Option "ward" In R versions &lt;= 3.0.3) does 
    ''' Not implement Ward's (1963) clustering criterion, whereas option "ward.D2" implements that 
    ''' criterion (Murtagh and Legendre 2014). With the latter, the dissimilarities are squared before 
    ''' cluster updating. Note that agnes(*, method="ward") corresponds to hclust(*, "ward.D2").
    '''
    ''' If members!= NULL, Then d Is taken To be a dissimilarity matrix between clusters instead 
    ''' Of dissimilarities between singletons And members gives the number Of observations per cluster. 
    ''' This way the hierarchical cluster algorithm can be 'started in the middle of the dendrogram’, 
    ''' e.g., in order to reconstruct the part of the tree above a cut (see examples). Dissimilarities 
    ''' between clusters can be efficiently computed (i.e., without hclust itself) only for a limited 
    ''' number of distance/linkage combinations, the simplest one being squared Euclidean distance 
    ''' and centroid linkage. In this case the dissimilarities between the clusters are the squared 
    ''' Euclidean distances between cluster means.
    '''
    ''' In hierarchical cluster displays, a decision Is needed at each merge to specify which subtree 
    ''' should go on the left And which on the right. Since, for n observations there are n-1 merges, 
    ''' there are 2^{(n-1)} possible orderings for the leaves in a cluster tree, Or dendrogram. The 
    ''' algorithm used in hclust Is to order the subtree so that the tighter cluster Is on the left 
    ''' (the last, i.e., most recent, merge of the left subtree Is at a lower value than the last 
    ''' merge of the right subtree). Single observations are the tightest clusters possible, And 
    ''' merges involving two observations place them in order by their observation sequence number.
    ''' </remarks>
    <ExportAPI("hclust")>
    <RApiReturn(GetType(Cluster))>
    Public Function hclust(d As DistanceMatrix,
                           Optional method$ = "complete",
                           Optional debug As Boolean = False,
                           Optional env As Environment = Nothing) As Object

        If d Is Nothing Then
            Return Internal.debug.stop(New NullReferenceException("the given distance matrix object can not be nothing!"), env)
        End If

        Dim alg As ClusteringAlgorithm = New DefaultClusteringAlgorithm With {.debug = debug}
        Dim matrix As Double()() = d.PopulateRows _
            .Select(Function(a) a.ToArray) _
            .ToArray
        Dim cluster As Cluster = alg.performClustering(matrix, d.keys, New AverageLinkageStrategy)

        Return cluster
    End Function

    <Extension>
    Private Function ensureNotIsDistance(d As DistanceMatrix) As DistanceMatrix
        Dim distRows As Double()() = d.PopulateRows.Select(Function(a) a.ToArray).ToArray
        Dim names As String() = d.keys
        Dim q As Double() = distRows.IteratesALL.QuantileLevels(fast:=distRows.Length >= 200)

        distRows = q _
            .Split(names.Length) _
            .Select(Function(v)
                        Return v.Select(Function(a) 1 - a).ToArray
                    End Function) _
            .ToArray

        Return New DistanceMatrix(names.Indexing, distRows, False)
    End Function

    ''' <summary>
    ''' do btree clustering
    ''' </summary>
    ''' <param name="d"></param>
    ''' <param name="equals"></param>
    ''' <param name="gt"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("btree")>
    <RApiReturn(GetType(BTreeCluster), GetType(Cluster))>
    Public Function btreeClusterFUN(<RRawVectorArgument> d As Object,
                                    Optional equals As Double = 0.9,
                                    Optional gt As Double = 0.7,
                                    Optional hclust As Boolean = False,
                                    Optional env As Environment = Nothing) As Object
        Dim cluster As BTreeCluster

        If d Is Nothing Then
            Return Internal.debug.stop(New NullReferenceException("the given distance matrix object can not be nothing!"), env)
        End If

        If TypeOf d Is DistanceMatrix Then
            cluster = DirectCast(d, DistanceMatrix).ensureNotIsDistance.BTreeCluster(equals, gt)
        Else
            Dim data As pipeline = pipeline.TryCreatePipeline(Of DataSet)(d, env)

            If data.isError Then
                Return data.getError
            Else
                cluster = data _
                    .populates(Of DataSet)(env) _
                    .BTreeCluster(equals, gt)
            End If
        End If

        If hclust Then
            Return cluster.ToHClust
        Else
            Return cluster
        End If
    End Function

    ''' <summary>
    ''' leaf distance value is ZERO always
    ''' </summary>
    ''' <returns></returns>
    <Extension>
    Private Function ToHClust(btree As BTreeCluster) As Cluster
        If btree.left Is Nothing AndAlso btree.right Is Nothing Then
            Return btree.hleaf
        Else
            Return btree.hnode
        End If
    End Function

    <Extension>
    Private Function hleaf(btree As BTreeCluster) As Cluster
        ' 是一个叶子节点
        If btree.members.IsNullOrEmpty Then
            Return New Cluster(btree.uuid) With {
                .Distance = New Distance(0, 1)
            }
        Else
            Dim leafTree As New Cluster($"leaf-{btree.uuid}") With {
                .Distance = New Distance(0, btree.members.Length)
            }

            For Each key As String In btree.members
                Call New Cluster(key) With {
                    .Distance = New Distance(0, 1)
                }.DoCall(AddressOf leafTree.AddChild)
            Next

            Return leafTree
        End If
    End Function

    <Extension>
    Private Function hnode(btree As BTreeCluster) As Cluster
        Dim node As New Cluster($"node-{btree.uuid}")
        Dim distance As Double
        Dim cl As Cluster

        If Not btree.left Is Nothing Then
            cl = btree.left.ToHClust
            node.AddChild(cl)
            distance += cl.DistanceValue + 1
        End If
        If Not btree.right Is Nothing Then
            cl = btree.right.ToHClust
            node.AddChild(cl)
            distance += cl.DistanceValue + 1
        End If

        For Each key As String In btree.members
            Call New Cluster(key) With {
                .Distance = New Distance(0, 1)
            }.DoCall(AddressOf node.AddChild)
        Next

        node.Distance = New Distance(distance)

        Return node
    End Function

    ''' <summary>
    ''' evaluate density of the raw data
    ''' </summary>
    ''' <param name="data">
    ''' dataset with any number of dimensions.
    ''' </param>
    ''' <param name="k"></param>
    ''' <returns></returns>
    <ExportAPI("density")>
    Public Function densityA(data As Rdataframe, Optional k As Integer = 6) As Double()
        Dim rows As ClusterEntity() = data.forEachRow _
            .Select(Function(d)
                        Return New ClusterEntity With {
                            .uid = d.name,
                            .entityVector = d.value.Select(Function(x) CDbl(x)).ToArray
                        }
                    End Function) _
            .ToArray
        Dim idOrder As Index(Of String) = rows.Select(Function(r) r.uid).Indexing

        Return Density.GetDensity(rows, k) _
            .OrderBy(Function(v)
                         Return idOrder.IndexOf(v.Name)
                     End Function) _
            .Select(Function(v) v.Value) _
            .ToArray
    End Function

    ''' <summary>
    ''' ### get cluster result data
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="labels"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("cluster.groups")>
    <RApiReturn(GetType(String), GetType(list))>
    Public Function clusterGroups(<RRawVectorArgument> x As Object,
                                  <RRawVectorArgument>
                                  Optional labels As Object = Nothing,
                                  Optional labelclass_tuple As Boolean = False,
                                  Optional env As Environment = Nothing) As Object

        Dim rawInputs As pipeline = pipeline.TryCreatePipeline(Of EntityClusterModel)(x, env)

        If rawInputs.isError Then
            Return rawInputs.getError
        ElseIf labelclass_tuple Then
            Return New list With {
                .slots = rawInputs _
                    .populates(Of EntityClusterModel)(env) _
                    .ToDictionary(Function(a) a.ID,
                                  Function(a)
                                      Return CObj(a.Cluster)
                                  End Function)
            }
        End If

        Dim labelList As String() = REnv.asVector(Of String)(labels)

        If labelList.IsNullOrEmpty Then
            Dim groups As New list With {
                .slots = New Dictionary(Of String, Object)
            }

            For Each group In rawInputs _
                .populates(Of EntityClusterModel)(env) _
                .GroupBy(Function(a)
                             Return a.Cluster
                         End Function)

                groups.slots(group.Key) = group.Keys.ToArray
            Next

            Return groups
        Else
            Dim index As Dictionary(Of String, String) = rawInputs _
                .populates(Of EntityClusterModel)(env) _
                .ToDictionary(Function(a) a.ID,
                              Function(a)
                                  Return a.Cluster
                              End Function)

            Return labelList _
                .Select(Function(label)
                            Return index.TryGetValue(label, [default]:="n/a")
                        End Function) _
                .ToArray
        End If
    End Function

    ''' <summary>
    ''' find objects from a given set of 2d points
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="sampleSize">
    ''' sample size for auto check best distance 
    ''' threshold value for the object detection.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("dbscan_objects")>
    Public Function dbscan_objects(<RRawVectorArgument>
                                   points As Object,
                                   Optional sampleSize As Integer = 50,
                                   Optional env As Environment = Nothing) As Object

        Dim ptList As pipeline = pipeline.TryCreatePipeline(Of PointF)(points, env, suppress:=True)
        Dim pixels As DataSet()
        Dim println = env.WriteLineHandler

        If ptList.isError Then
            ptList = pipeline.TryCreatePipeline(Of Point2D)(points, env)

            If ptList.isError Then
                Return ptList.getError
            Else
                pixels = ptList _
                    .populates(Of Point2D)(env) _
                    .Select(Function(p)
                                Return New DataSet With {
                                    .ID = $"[{p.X},{p.Y}]",
                                    .Properties = New Dictionary(Of String, Double) From {
                                        {"x", CDbl(p.X)}, {"y", CDbl(p.Y)}
                                    }
                                }
                            End Function) _
                    .ToArray
            End If
        Else
            pixels = ptList _
                .populates(Of PointF)(env) _
                .Select(Function(p)
                            Return New DataSet With {
                                .ID = $"[{p.X},{p.Y}]",
                                .Properties = New Dictionary(Of String, Double) From {
                                    {"x", CDbl(p.X)}, {"y", CDbl(p.Y)}
                                }
                            }
                        End Function) _
                .ToArray
        End If

        Dim uniqueId As String() = pixels _
            .Select(Function(d) d.ID) _
            .uniqueNames

        For i As Integer = 0 To uniqueId.Length - 1
            pixels(i).ID = uniqueId(i)
        Next

        Dim kd As New KdTree(Of DataSet)(pixels, New point2DReader())
        Dim k As Integer = 60
        Dim averageDist = Enumerable _
            .Range(0, sampleSize) _
            .AsParallel _
            .Select(Function(any)
                        Dim knn = kd _
                            .nearest(kd.GetPointSample(1).First, k) _
                            .ToArray

                        Return Aggregate x As KdNodeHeapItem(Of DataSet)
                               In knn
                               Order By x.distance
                               Take CInt(k / 2)
                               Let d = x.distance
                               Into Average(d)
                    End Function) _
            .ToArray
        Dim meps As Double = averageDist.Average

        Call println($"get average point distance from {sampleSize} sample data:")
        Call println(averageDist)

        Call println("use mean distance as eps threshold for dbscan:")
        Call println(meps)

        Dim dbscan As dbscanResult = clustering.dbscan(
            data:=pixels,
            eps:=meps * 1.125,
            env:=env
        )
        Dim classinfo As Dictionary(Of String, String) = dbscan.cluster _
            .ToDictionary(Function(d) d.ID,
                          Function(d)
                              Return d.Cluster
                          End Function)

        Return (From d As DataSet
                In pixels
                Select classinfo(d.ID)).ToArray
    End Function

    Private Class point2DReader : Inherits KdNodeAccessor(Of DataSet)

        ReadOnly dims As String() = {"x", "y"}

        Public Overrides Sub setByDimensin(x As DataSet, dimName As String, value As Double)
            x(dimName) = value
        End Sub

        Public Overrides Function GetDimensions() As String()
            Return dims
        End Function

        Public Overrides Function metric(a As DataSet, b As DataSet) As Double
            Dim v1 As Double() = a(dims)
            Dim v2 As Double() = b(dims)

            Return v1.EuclideanDistance(v2)
        End Function

        Public Overrides Function getByDimension(x As DataSet, dimName As String) As Double
            Return x(dimName)
        End Function

        Public Overrides Function nodeIs(a As DataSet, b As DataSet) As Boolean
            Return a Is b
        End Function

        Public Overrides Function activate() As DataSet
            Return New DataSet With {
                .ID = App.NextTempName,
                .Properties = New Dictionary(Of String, Double) From {
                    {"x", 0.0},
                    {"y", 0.0}
                }
            }
        End Function
    End Class

    ''' <summary>
    ''' ### DBSCAN density reachability and connectivity clustering
    ''' 
    ''' Generates a density based clustering of arbitrary shape as 
    ''' introduced in Ester et al. (1996).
    ''' 
    ''' Clusters require a minimum no of points (MinPts) within a maximum 
    ''' distance (eps) around one of its members (the seed). Any point 
    ''' within eps around any point which satisfies the seed condition 
    ''' is a cluster member (recursively). Some points may not belong to 
    ''' any clusters (noise).
    ''' </summary>
    ''' <param name="data">data matrix, data.frame, dissimilarity matrix 
    ''' or dist-object. Specify method="dist" if the data should be 
    ''' interpreted as dissimilarity matrix or object. Otherwise Euclidean 
    ''' distances will be used.</param>
    ''' <param name="eps">Reachability distance, see Ester et al. (1996).</param>
    ''' <param name="minPts">Reachability minimum no. Of points, see Ester et al. (1996).</param>
    ''' <param name="scale">scale the data if TRUE.</param>
    ''' <param name="method">
    ''' "dist" treats data as distance matrix (relatively fast but memory 
    ''' expensive), "raw" treats data as raw data and avoids calculating a 
    ''' distance matrix (saves memory but may be slow), "hybrid" expects 
    ''' also raw data, but calculates partial distance matrices (very fast 
    ''' with moderate memory requirements).
    ''' </param>
    ''' <param name="seeds">FALSE to not include the isseed-vector in the dbscan-object.</param>
    ''' <param name="countmode">
    ''' NULL or vector of point numbers at which to report progress.
    ''' </param>
    ''' <returns>
    ''' the result data is not keeps the same order as the data input!
    ''' </returns>
    <ExportAPI("dbscan")>
    Public Function dbscan(<RRawVectorArgument> data As Object,
                           eps As Double,
                           Optional minPts As Integer = 5,
                           Optional scale As Boolean = False,
                           Optional method As dbScanMethods = dbScanMethods.raw,
                           Optional seeds As Boolean = True,
                           Optional countmode As Object = Nothing,
                           Optional filterNoise As Boolean = False,
                           Optional reorder_class As Boolean = False,
                           Optional densityCut As Double = -1,
                           Optional env As Environment = Nothing) As dbscanResult

        Dim x As DataSet()

        If data Is Nothing Then
            Return Nothing
        ElseIf TypeOf data Is DataSet() Then
            x = DirectCast(data, DataSet())
        ElseIf TypeOf data Is Rdataframe Then
            With DirectCast(data, Rdataframe)
                Dim rownames As String() = .getRowNames

                x = .nrows _
                    .Sequence _
                    .Select(Function(i)
                                Dim id As String = rownames.ElementAtOrDefault(i, i + 1)
                                Dim row As Dictionary(Of String, Object) = .getRowList(i, drop:=True)
                                Dim r As New DataSet With {
                                    .ID = id,
                                    .Properties = row.AsNumeric
                                }

                                Return r
                            End Function) _
                    .ToArray
            End With
        Else
            Throw New NotImplementedException
        End If

        Dim dist As Func(Of DataSet, DataSet, Double)

        Select Case method
            Case dbScanMethods.dist
                x = x _
                    .Euclidean _
                    .PopulateRowObjects(Of DataSet) _
                    .ToArray
                dist = Function(a, b) a(b.ID)
            Case dbScanMethods.raw
                Dim all As String() = x.PropertyNames

                dist = Function(a, b)
                           Return a(all).EuclideanDistance(b(all))
                       End Function
            Case dbScanMethods.hybrid
                Throw New NotImplementedException
            Case Else
                Throw New NotImplementedException
        End Select

        Dim isseed As Integer() = Nothing
        Dim result = New DbscanAlgorithm(Of DataSet)(
            metricFunc:=dist,
            println:=env.WriteLineHandler
        ).ComputeClusterDBSCAN(
            allPoints:=x,
            epsilon:=eps,
            minPts:=minPts,
            is_seed:=isseed,
            filterNoise:=filterNoise,
            densityCut:=densityCut
        )
        Dim clusterData As EntityClusterModel() = result _
            .Select(Function(c)
                        Return c _
                            .Select(Function(r)
                                        Return New EntityClusterModel With {
                                            .Cluster = c.name,
                                            .ID = r.ID,
                                            .Properties = r.Properties
                                        }
                                    End Function)
                    End Function) _
            .IteratesALL _
            .ToArray

        If reorder_class Then
            clusterData = clusterData _
                .OrderBy(Function(d) Integer.Parse(d.Cluster)) _
                .ToArray
        End If

        Return New dbscanResult With {
            .cluster = clusterData,
            .eps = eps,
            .MinPts = minPts,
            .isseed = isseed _
                .Select(Function(i) x(i).ID) _
                .ToArray
        }
    End Function
End Module

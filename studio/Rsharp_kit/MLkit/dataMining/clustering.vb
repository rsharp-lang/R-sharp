#Region "Microsoft.VisualBasic::5da78c319d4484d1288216812c64b1c4, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//dataMining/clustering.vb"

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

'   Total Lines: 834
'    Code Lines: 576
' Comment Lines: 165
'   Blank Lines: 93
'     File Size: 35.15 KB


' Module clustering
' 
'     Function: btreeClusterFUN, clusterGroups, clusterResultDataFrame, clusterSummary, cmeansSummary
'               dbscan, dbscan_objects, densityA, ensureNotIsDistance, fuzzyCMeans
'               hclust, hdbscan_exec, hleaf, hnode, Kmeans
'               showHclust, ToHClust
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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.BinaryTree
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.DBSCAN
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Distance
Imports Microsoft.VisualBasic.DataMining.HDBSCAN.Runner
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.DataMining.Lloyds
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder
Imports Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder.GMM
Imports Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder.GMM.EMGaussianMixtureModel
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports BisectingKMeans = Microsoft.VisualBasic.DataMining.KMeans.Bisecting.BisectingKMeans
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

        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(Bisecting.Cluster()), AddressOf clustersDf1)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(EntityClusterModel()), AddressOf clusterResultDataFrame)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(FuzzyCMeansEntity()), AddressOf cmeansSummary)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(
            type:=GetType(dbscanResult),
            handler:=Function(result, args, env)
                         Return DirectCast(result, dbscanResult).cluster.clusterResultDataFrame(args, env)
                     End Function)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(BTreeCluster), AddressOf treeDf)

        Call REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of Cluster)(AddressOf showHclust)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function showHclust(cluster As Cluster) As String
        Return cluster.ToConsoleLine
    End Function

    Private Function clustersDf1(clusters As Bisecting.Cluster(), args As list, env As Environment) As Rdataframe
        Dim df As New Rdataframe With {.columns = New Dictionary(Of String, Array)}
        Dim width As Integer = clusters(0).centroid.Length
        Dim v As List(Of Double)() = New List(Of Double)(width - 1) {}
        Dim rownames As New List(Of String)
        Dim class_id As New List(Of Integer)

        For i As Integer = 0 To width - 1
            v(i) = New List(Of Double)
        Next

        For Each c As Bisecting.Cluster In clusters
            For Each p As ClusterEntity In c
                Call rownames.Add(p.uid)
                Call class_id.Add(c.Cluster)

                For i As Integer = 0 To width - 1
                    Call v(i).Add(p(i))
                Next
            Next
        Next

        df.add("clusters", class_id)
        df.rownames = rownames.ToArray

        For i As Integer = 0 To width - 1
            Call df.add($"V{i + 1}", v(i))
        Next

        Return df
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

    ''' <summary>
    ''' <see cref="clusterResultDataFrame"/>
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <param name="args">
    ''' colnames could be optional
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function treeDf(tree As BTreeCluster, args As list, env As Environment) As Rdataframe
        Return tree _
            .GetClusterResult(vnames:=args.getValue(Of String())("colnames", env)) _
            .ToArray _
            .clusterResultDataFrame(args, env)
    End Function

    ''' <summary>
    ''' generates a cluster result dataframe with data:
    ''' 
    ''' rownames, [Cluster, ...property_names...]
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <Extension>
    Public Function clusterResultDataFrame(data As EntityClusterModel(), args As list, env As Environment) As Rdataframe
        Dim table As File = data.ToCsvDoc
        Dim matrix As New Rdataframe With {
            .columns = New Dictionary(Of String, Array)
        }
        Dim header As String
        Dim colVals As Array
        Dim row_names = args.getValue(Of Object)("row.names", env, Nothing)

        For Each column As String() In table.Columns.Skip(1)
            header = column(Scan0)
            colVals = column.Skip(1).ToArray
            matrix.columns.Add(header, colVals)
        Next

        If row_names Is Nothing Then
            matrix.rownames = table.Columns _
                .First _
                .Skip(1) _
                .ToArray
        Else
            matrix.rownames = CLRVector.asCharacter(row_names)
        End If

        Return matrix
    End Function

    ''' <summary>
    ''' Construct a Gaussian Mixture Model with specific n components
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("gmm")>
    Public Function gmmf(<RRawVectorArgument>
                         x As Object,
                         Optional components As Integer = 3,
                         Optional threshold As Double = 0.0000001,
                         Optional strict As Boolean = True,
                         Optional verbose As Boolean = False,
                         Optional env As Environment = Nothing) As Object

        If x Is Nothing Then
            Return Nothing
        End If

        If TypeOf x Is Rdataframe Then
            Dim rowdatas As ClusterEntity() = DirectCast(x, Rdataframe).forEachRow() _
                .Select(Function(v)
                            Return New ClusterEntity With {
                                .uid = v.name,
                                .entityVector = CLRVector.asNumeric(v.value)
                            }
                        End Function) _
                .ToArray

            ' nd
            Return GMM.Solver.Predicts(rowdatas, components, threshold, strict:=strict)
        End If

        If TypeOf x Is vector Then
            x = DirectCast(x, vector).data
        End If

        Dim seq As pipeline = pipeline.TryCreatePipeline(Of ClusterEntity)(x, env)

        If seq.isError Then
            If x.GetType.IsArray Then
                ' 1d
                x = TryCastGenericArray(x, env)
                x = GMM.Solver.Predicts(CLRVector.asNumeric(x), components, threshold,
                      verbose:=verbose)

                Return x
            Else
                Return seq.getError
            End If
        End If

        ' nd
        Return GMM.Solver.Predicts(seq.populates(Of ClusterEntity)(env), components, threshold, strict:=strict)
    End Function

    ''' <summary>
    ''' Get cluster assign result
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("gmm.predict")>
    <RApiReturn(TypeCodes.integer)>
    Public Function gmm_predict(x As Object, Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Nothing
        End If

        If TypeOf x Is Mixture Then
            Return DirectCast(x, Mixture).data _
                .Select(Function(di) di.max) _
                .ToArray
        ElseIf TypeOf x Is GaussianMixtureModel Then
            Return DirectCast(x, GaussianMixtureModel).Probs _
                .Select(Function(di) which.Max(di) + 1) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(GaussianMixtureModel), x.GetType, env)
        End If
    End Function

    <ExportAPI("gmm.components")>
    Public Function gmm_components(x As Object) As Object
        If x Is Nothing Then
            Return Nothing
        End If

        If TypeOf x Is Mixture Then
            Dim comps = DirectCast(x, Mixture).components
            Dim df As New Rdataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = comps _
                    .Select(Function(c, i) $"C{i + 1}") _
                    .ToArray
            }

            Call df.add("mean", comps.Select(Function(c) If(c.Mean.IsNaNImaginary, Double.NaN, c.Mean)))
            Call df.add("stdev", comps.Select(Function(c) If(c.Stdev.IsNaNImaginary, Double.NaN, c.Stdev)))
            Call df.add("weight", comps.Select(Function(c) If(c.Weight.IsNaNImaginary, Double.NaN, c.Weight)))

            Return df
        Else
            Throw New NotImplementedException
        End If
    End Function

    <ExportAPI("gmm.predict_proba")>
    Public Function gmm_predict_proba(x As Object, Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Nothing
        End If

        If TypeOf x Is Mixture Then
            Dim mx As Mixture = DirectCast(x, Mixture)
            Dim df As New Rdataframe With {.columns = New Dictionary(Of String, Array)}
            Dim ds = mx.data.ToArray

            df.rownames = ds.Select(Function(di) di.dataId).ToArray
            df.add("max", ds.Select(Function(di) di.max))

            Dim offset As Integer

            For i As Integer = 0 To mx.components.Length - 1
                offset = i
                df.add($"C{i + 1}", ds.Select(Function(di) di.probs(offset)))
            Next

            Return df
        ElseIf TypeOf x Is GaussianMixtureModel Then
            Dim mx As GaussianMixtureModel = DirectCast(x, GaussianMixtureModel)
            Dim df As New Rdataframe With {.columns = New Dictionary(Of String, Array)}
            Dim ds = mx.DataSet
            Dim probs = mx.Probs
            Dim index As Integer = 0

            df.rownames = ds.Select(Function(di) di.uid).ToArray
            df.add("max", probs.Select(Function(di) which.Max(di) + 1))

            For i As Integer = 0 To mx.Components.Length - 1
                index = i
                df.add($"C{i + 1}", probs.Select(Function(di) di(index)))
            Next

            Return df
        Else
            Return Message.InCompatibleType(GetType(GaussianMixtureModel), x.GetType, env)
        End If
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

    Private Function getDataModel(x As Object, env As Environment) As [Variant](Of Message, EntityClusterModel())
        Dim model As EntityClusterModel()

        If x.GetType.IsArray Then
#Disable Warning
            Select Case REnv.MeasureArrayElementType(x)
                Case GetType(DataSet)
                    model = DirectCast(REnv.asVector(Of DataSet)(x), DataSet()).ToKMeansModels
                Case GetType(EntityClusterModel)
                    model = REnv.asVector(Of EntityClusterModel)(x)
                Case Else
                    Return REnv.Internal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
            End Select
#Enable Warning
        ElseIf TypeOf x Is Rdataframe Then
            Dim colNames As String() = DirectCast(x, Rdataframe).columns _
                .Keys _
                .ToArray

            model = DirectCast(x, Rdataframe) _
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
            Return REnv.Internal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
        End If

        Return model
    End Function

    Dim m_traceback As TraceBackAlgorithm

    ''' <summary>
    ''' get the clustering traceback
    ''' </summary>
    ''' <param name="x">
    ''' the json data for construct the traceback matrix object
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("getTraceback")>
    <RApiReturn(GetType(TracebackMatrix))>
    Public Function getTraceback(Optional x As list = Nothing, Optional env As Environment = Nothing) As Object
        If Not x Is Nothing Then
            Dim data As NamedCollection(Of String)() = x _
                .AsGeneric(Of String())(env) _
                .Select(Function(t)
                            Return New NamedCollection(Of String)(t.Key, t.Value)
                        End Function) _
                .ToArray

            Return New TracebackMatrix With {.data = data}
        End If

        If m_traceback Is Nothing Then
            Return Nothing
        Else
            Dim points_traceback = m_traceback.GetTraceBack.ToArray
            Dim list As New TracebackMatrix With {
                .data = points_traceback
            }

            Return list
        End If
    End Function

    ''' <summary>
    ''' 
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="T1"></param>
    ''' <param name="T2"></param>
    ''' <param name="seed">
    ''' use the canopy method as kmeans seed or just used for clustering?
    ''' 
    ''' set this parameter to value true means used the result as seed, then a seed object of 
    ''' type <see cref="CanopySeeds"/> will be generated from this function. otherwise parameter 
    ''' value false means the result is a collection of the <see cref="Bisecting.Cluster"/> 
    ''' result, you can convert the cluster result to a dataframe via ``as.data.frame`` method.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' value of <paramref name="T1"/> should greater than <paramref name="T2"/>, example as:
    ''' 
    ''' ```
    ''' T1 = 8 and T2 = 4
    ''' ```
    ''' </remarks>
    <ExportAPI("canopy")>
    Public Function Canopy(<RRawVectorArgument> x As Object,
                           Optional T1 As Double = Double.NaN,
                           Optional T2 As Double = Double.NaN,
                           Optional seed As Boolean = True,
                           Optional env As Environment = Nothing) As Object

        If x Is Nothing Then
            Return Nothing
        End If

        Dim model = getDataModel(x, env)

        If model Like GetType(Message) Then
            Return model.TryCast(Of Message)
        End If

        Dim maps As New DataSetConvertor(model.TryCast(Of EntityClusterModel()))
        Dim data As IEnumerable(Of ClusterEntity) = maps.GetVectors(model.TryCast(Of EntityClusterModel()))
        Dim builder As CanopyBuilder

        If T1.IsNaNImaginary OrElse T2.IsNaNImaginary Then
            builder = New CanopyBuilder(data)
        Else
            builder = New CanopyBuilder(data, T1, T2)
        End If

        If seed Then
            Return builder.KMeansSeeds
        Else
            Return builder.Solve
        End If
    End Function

    ''' <summary>
    ''' K-Means Clustering
    ''' </summary>
    ''' <param name="x">
    ''' numeric matrix of data, or an object that can be coerced 
    ''' to such a matrix (such as a numeric vector or a data 
    ''' frame with all numeric columns).
    ''' </param>
    ''' <param name="centers">
    ''' either the number of clusters, say k, or a set of initial 
    ''' (distinct) cluster centres. If a number, a random set of 
    ''' (distinct) rows in x is chosen as the initial centres.
    ''' 
    ''' this parameter value could be an integer value or a seed value object 
    ''' in clr type <see cref="CanopySeeds"/> which is produced via the 
    ''' ``canopy`` function.
    ''' </param>
    ''' <param name="n_threads">the parallel options, for configs the number of 
    ''' cpu cores for run the parallel task code.</param>
    ''' <param name="debug"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("kmeans")>
    <RApiReturn(GetType(EntityClusterModel))>
    Public Function Kmeans(<RRawVectorArgument>
                           x As Object,
                           Optional centers As Object = 3,
                           Optional bisecting As Boolean = False,
                           Optional n_threads As Integer = 16,
                           Optional debug As Boolean = False,
                           Optional traceback As Boolean = False,
                           Optional env As Environment = Nothing) As Object

        If x Is Nothing Then
            Return Nothing
        End If

        Dim model = getDataModel(x, env)

        If model Like GetType(Message) Then
            Return model.TryCast(Of Message)
        End If

        Dim nk As Integer

        If TypeOf centers Is CanopySeeds Then
            nk = DirectCast(centers, CanopySeeds).k
        Else
            nk = CLRVector.asInteger(centers).DefaultFirst(3)
        End If

        If bisecting Then
            Dim maps As New DataSetConvertor(model.TryCast(Of EntityClusterModel()))
            Dim bikmeans As New BisectingKMeans(
                dataList:=maps.GetVectors(model.TryCast(Of EntityClusterModel())),
                k:=nk,
                traceback:=traceback,
                n_threads:=n_threads
            )
            Dim result = bikmeans.runBisectingKMeans().ToArray
            Dim kmeans_result As New List(Of EntityClusterModel)
            Dim i As i32 = 1

            m_traceback = bikmeans

            For Each cluster In result
                Call kmeans_result.AddRange(maps.GetObjects(cluster.DataPoints, setClass:=++i))
            Next

            Return kmeans_result.ToArray
        ElseIf TypeOf centers Is CanopySeeds Then

        Else
            Return model.TryCast(Of EntityClusterModel()) _
                .Kmeans(nk, debug, n_threads:=n_threads) _
                .ToArray
        End If
    End Function

    <ExportAPI("lloyds")>
    Public Function Lloyds(<RRawVectorArgument>
                           x As Object,
                           Optional k% = 3,
                           Optional env As Environment = Nothing) As Object

        If x Is Nothing Then
            Return Nothing
        End If

        Dim model = getDataModel(x, env)

        If model Like GetType(Message) Then
            Return model.TryCast(Of Message)
        End If

        Dim maps As New DataSetConvertor(model.TryCast(Of EntityClusterModel()))
        Dim alg As New LloydsMethodClustering(maps.GetPoints(model.TryCast(Of EntityClusterModel())), k:=k)
        Dim result = maps.GetObjects(alg.Clustering).ToArray

        Return result
    End Function

    ''' <summary>
    ''' Silhouette Coefficient
    ''' </summary>
    ''' <param name="x">the cluster result</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Silhouette score is used to evaluate the quality of clusters created using clustering 
    ''' algorithms such as K-Means in terms of how well samples are clustered with other samples 
    ''' that are similar to each other. The Silhouette score is calculated for each sample of 
    ''' different clusters. To calculate the Silhouette score for each observation/data point, 
    ''' the following distances need to be found out for each observations belonging to all the 
    ''' clusters:
    ''' 
    ''' Mean distance between the observation And all other data points In the same cluster. This
    ''' distance can also be called a mean intra-cluster distance. The mean distance Is denoted by a
    ''' Mean distance between the observation And all other data points Of the Next nearest cluster.
    ''' This distance can also be called a mean nearest-cluster distance. The mean distance Is 
    ''' denoted by b
    ''' 
    ''' Silhouette score, S, for Each sample Is calculated Using the following formula:
    ''' 
    ''' \(S = \frac{(b - a)}{max(a, b)}\)
    ''' 
    ''' The value Of the Silhouette score varies from -1 To 1. If the score Is 1, the cluster Is
    ''' dense And well-separated than other clusters. A value near 0 represents overlapping clusters
    ''' With samples very close To the decision boundary Of the neighboring clusters. A negative 
    ''' score [-1, 0] indicates that the samples might have got assigned To the wrong clusters.
    ''' </remarks>
    <ExportAPI("silhouette_score")>
    Public Function silhouette_score(<RRawVectorArgument> x As Object,
                                     Optional traceback As TracebackMatrix = Nothing,
                                     Optional env As Environment = Nothing) As Object

        Dim pull = dataSetCommon(x, env)

        If pull Like GetType(Message) Then
            Return pull.TryCast(Of Message)
        End If

        If traceback Is Nothing Then
            Return pull.TryCast(Of IEnumerable(Of ClusterEntity)).Silhouette
        Else
            Dim itr As New TraceBackIterator(traceback.data)
            Dim data = pull.TryCast(Of IEnumerable(Of ClusterEntity)).ToArray
            Dim curveLine = EvaluationScore.SilhouetteCoeff(data, itr).ToArray

            Dim df As New Rdataframe With {
                .columns = New Dictionary(Of String, Array)
            }

            Call df.add("num_class", curveLine.Select(Function(t) t.X))
            Call df.add("silhouette", curveLine.Select(Function(t) t.Y))

            Return df
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="traceback"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("calinski_harabasz")>
    Public Function calinski_harabasz(<RRawVectorArgument> x As Object,
                                      Optional traceback As TracebackMatrix = Nothing,
                                      Optional env As Environment = Nothing) As Object
        Dim pull = dataSetCommon(x, env)

        If pull Like GetType(Message) Then
            Return pull.TryCast(Of Message)
        End If

        If traceback Is Nothing Then
            Return pull.TryCast(Of IEnumerable(Of ClusterEntity)).calinskiharabasz
        Else
            Dim itr As New TraceBackIterator(traceback.data)
            Dim data = pull.TryCast(Of IEnumerable(Of ClusterEntity)).ToArray
            Dim curveLine = EvaluationScore.CalinskiHarabaszs(data, itr).ToArray

            Dim df As New Rdataframe With {
                .columns = New Dictionary(Of String, Array)
            }

            Call df.add("num_class", curveLine.Select(Function(t) t.X))
            Call df.add("calinski_harabasz", curveLine.Select(Function(t) t.Y))

            Return df
        End If
    End Function

    Private Function dataSetCommon(x As Object, env As Environment) As [Variant](Of Message, IEnumerable(Of ClusterEntity))
        Dim points As pipeline = pipeline.TryCreatePipeline(Of EntityClusterModel)(x, env)
        Dim data As IEnumerable(Of ClusterEntity)

        If TypeOf x Is Rdataframe Then
            data = DirectCast(x, Rdataframe) _
                .forEachRow _
                .Select(Function(r)
                            Return New ClusterEntity(r.name, CLRVector.asNumeric(r.value))
                        End Function)
        ElseIf points.isError Then
            points = pipeline.TryCreatePipeline(Of ClusterEntity)(x, env)

            If points.isError Then
                Return points.getError
            Else
                data = points.populates(Of ClusterEntity)(env)
            End If
        Else
            Dim pull = points.populates(Of EntityClusterModel)(env).ToArray
            Dim mapper As New DataSetConvertor(pull)

            data = mapper.GetVectors(pull)
        End If

        Return data
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
    ''' do clustering via binary tree
    ''' </summary>
    ''' <param name="d">the input dataset</param>
    ''' <param name="equals">the score threshold that asserts that two vector is
    ''' equals, then they will assigned to a same tree cluster node.</param>
    ''' <param name="gt">the score threshold that asserts that two vector is not
    ''' the same but similar to other, then we could put the new vector into the
    ''' right node of current cluster tree node.</param>
    ''' <param name="as_hclust">
    ''' and also converts the tree data as the hclust data model?
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' the cluster result could be converts from clr object to R# dataframe object
    ''' via the ``as.data.frame`` function.
    ''' </returns>
    <ExportAPI("btree")>
    <RApiReturn(GetType(BTreeCluster), GetType(Cluster))>
    Public Function btreeClusterFUN(<RRawVectorArgument> d As Object,
                                    Optional equals As Double = 0.9,
                                    Optional gt As Double = 0.7,
                                    Optional as_hclust As Boolean = False,
                                    Optional method As CompareMethods = CompareMethods.SpectrumDotProduct,
                                    Optional env As Environment = Nothing) As Object
        Dim cluster As BTreeCluster

        If d Is Nothing Then
            Return Internal.debug.stop(New NullReferenceException("the given distance matrix object can not be nothing!"), env)
        End If

        If TypeOf d Is DistanceMatrix Then
            cluster = DirectCast(d, DistanceMatrix) _
                .ensureNotIsDistance _
                .BTreeCluster(equals, gt)
        ElseIf TypeOf d Is Rdataframe Then
            cluster = DirectCast(d, Rdataframe) _
                .forEachRow _
                .Select(Function(ri)
                            Return New ClusterEntity(ri.name, CLRVector.asNumeric(ri.value))
                        End Function) _
                .BTreeClusterVector(equals, gt, method)
        Else
            Dim data As pipeline = pipeline.TryCreatePipeline(Of DataSet)(d, env)

            If data.isError Then
                Return data.getError
            Else
                cluster = data _
                    .populates(Of DataSet)(env) _
                    .BTreeCluster(equals, gt, method)
            End If
        End If

        If as_hclust Then
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
    ''' get or set the cluster class labels
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="[class]"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("clusters")>
    Public Function clusters(<RRawVectorArgument> x As Object,
                             <RRawVectorArgument>
                             <RByRefValueAssign>
                             Optional [class] As Object = Nothing,
                             Optional env As Environment = Nothing) As Object

        Dim entities As pipeline = pipeline.TryCreatePipeline(Of EntityClusterModel)(x, env)

        If entities.isError Then
            Return entities.getError
        End If

        If [class] Is Nothing Then
            ' get cluster labels
            Return entities.populates(Of EntityClusterModel)(env) _
                .Select(Function(i) i.Cluster) _
                .ToArray
        End If

        ' set cluster labels
        If TypeOf [class] Is list Then
            Dim labels As Dictionary(Of String, String) = DirectCast([class], list) _
                .AsGeneric(Of String)(env)
            Dim list As New List(Of String)

            For Each item As EntityClusterModel In entities.populates(Of EntityClusterModel)(env)
                item.Cluster = labels.TryGetValue(item.ID, [default]:="no_class")
                list.Add(item.Cluster)
            Next

            Return list.ToArray
        Else
            Dim labels As String() = CLRVector.asCharacter([class])
            Dim pull As EntityClusterModel() = entities.populates(Of EntityClusterModel)(env).ToArray

            For i As Integer = 0 To pull.Length - 1
                pull(i).Cluster = labels(i)
            Next

            Return labels
        End If
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

        Dim labelList As String() = CLRVector.asCharacter(labels)

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

    <ExportAPI("hdbscan")>
    Public Function hdbscan_exec(<RRawVectorArgument>
                                 data As Object,
                                 Optional min_points As Integer = 6,
                                 Optional min_clusters As Integer = 6,
                                 Optional env As Environment = Nothing) As Object

        Dim dataset As Double()()
        Dim labels As String()

        If TypeOf data Is Rdataframe Then
            Dim rows = DirectCast(data, Rdataframe) _
                .forEachRow _
                .Select(Function(v)
                            Return New NamedCollection(Of Double)(v.name, CLRVector.asNumeric(v.value))
                        End Function) _
                .ToArray

            labels = rows.Select(Function(v) v.name).ToArray
            dataset = rows.Select(Function(v) v.value).ToArray
        Else
            Return Internal.debug.stop("", env)
        End If

        Dim opts As New HdbscanParameters(Of Double()) With {
            .DataSet = dataset,
            .CacheDistance = True,
            .MinPoints = min_points,
            .MinClusterSize = min_clusters,
            .DistanceFunction = New CosineSimilarity
        }
        Dim result As HdbscanResult = HdbscanRunner.Run(opts)
        Dim clusters As New list With {.slots = New Dictionary(Of String, Object)}

        For i As Integer = 0 To labels.Length - 1
            Call clusters.add(labels(i), result.Labels(i))
        Next

        Return clusters
    End Function

    ''' <summary>
    ''' ### K-NN Classifier in R Programming
    ''' 
    ''' K-Nearest Neighbor or K-NN is a Supervised Non-linear classification 
    ''' algorithm. K-NN is a Non-parametric algorithm i.e it doesn’t make any 
    ''' assumption about underlying data or its distribution. It is one of 
    ''' the simplest and widely used algorithm which depends on it’s k value
    ''' (Neighbors) and finds it’s applications in many industries like 
    ''' finance industry, healthcare industry etc.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="k"></param>
    ''' <param name="jaccard"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' In the KNN algorithm, K specifies the number of neighbors and its 
    ''' algorithm is as follows:
    '''
    ''' + Choose the number K Of neighbor.
    ''' + Take the K Nearest Neighbor Of unknown data point according To distance.
    ''' + Among the K-neighbors, Count the number Of data points In Each category.
    ''' + Assign the New data point To a category, where you counted the most neighbors.
    ''' 
    ''' For the Nearest Neighbor classifier, the distance between two points 
    ''' Is expressed in the form of Euclidean Distance.
    ''' </remarks>
    <ExportAPI("knn")>
    Public Function knn(<RRawVectorArgument>
                        x As Object,
                        Optional k As Integer = 16,
                        Optional jaccard As Double = 0.6,
                        Optional env As Environment = Nothing) As Object

        Dim data As ClusterEntity()
        Dim colnames As String()

        If x Is Nothing Then
            Return Nothing
        End If

        If TypeOf x Is Rdataframe Then
            Dim df As Rdataframe = x

            colnames = df.colnames
            data = df.forEachRow _
                .Select(Function(d)
                            Return New ClusterEntity(d.name, CLRVector.asNumeric(d.value))
                        End Function) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(Rdataframe), x.GetType, env)
        End If

        Call VBDebugger.EchoLine("build kd-tree for the input dataset...")

        Dim graph As New KNNGraph(data)

        Call VBDebugger.EchoLine("build kd-tree finished!")
        Call VBDebugger.EchoLine("do knn search and build binary avl-tree for run clustering...")

        Dim clusters As BTreeCluster = graph.GetGraph(k, jaccard)
        Dim args As New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"colnames", colnames}
            }
        }

        Call VBDebugger.EchoLine("export cluster dataframe...")

        Dim result As Rdataframe = clusters.treeDf(args, env)

        Return result
    End Function

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

        Dim dataLabels As String() = x.Keys.ToArray
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
                .ToArray,
            .dataLabels = dataLabels
        }
    End Function
End Module

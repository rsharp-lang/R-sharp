#Region "Microsoft.VisualBasic::7d5b938fa597d384dbe4e71595eb9d9c, Library\R.math\dataScience\dataMining\clustering.vb"

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

    ' Module clustering
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: clusterResultDataFrame, clusterSummary, Kmeans
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# data clustering tools
''' </summary>
<Package("stats.clustering")>
Module clustering

    Sub New()
        Call REnv.Internal.generic.add("summary", GetType(EntityClusterModel()), AddressOf clusterSummary)

        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(EntityClusterModel()), AddressOf clusterResultDataFrame)
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
    ''' <param name="MinPts">Reachability minimum no. Of points, see Ester et al. (1996).</param>
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
    ''' <returns></returns>
    <ExportAPI("dbscan")>
    Public Function dbscan(<RRawVectorArgument> data As Object, eps As Double, Optional MinPts As Integer = 5, Optional scale As Boolean = False, Optional method As dbScanMethods = dbScanMethods.hybrid, Optional seeds As Boolean = True, Optional countmode As Object = Nothing)

    End Function

    Public Enum dbScanMethods
        hybrid
        raw
        dist
    End Enum
End Module


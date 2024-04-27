#Region "Microsoft.VisualBasic::7566c24f0d1214f754dffc620ca7fee0, E:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//Manifold.vb"

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

    '   Total Lines: 258
    '    Code Lines: 170
    ' Comment Lines: 59
    '   Blank Lines: 29
    '     File Size: 10.73 KB


    ' Module Manifold
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: asGraph, exportUmapTable, umapProjection
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

''' <summary>
''' UMAP: Uniform Manifold Approximation and Projection for Dimension Reduction
''' </summary>
<Package("umap")>
Module Manifold

    Sub New()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(Umap), AddressOf exportUmapTable)
        Call Internal.generic.add("plot", GetType(Umap), AddressOf datasetKit.EmbeddingRender)
    End Sub

    <RGenericOverloads("as.data.frame")>
    Private Function exportUmapTable(umap As Umap, args As list, env As Environment) As Rdataframe
        Dim labels As String() = CLRVector.asCharacter(args.getByName("labels"))
        Dim colNames As String() = CLRVector.asCharacter(args.getByName("dimension"))
        Dim table As New Rdataframe With {
            .columns = New Dictionary(Of String, Array)
        }
        Dim projection As Double()() = umap.GetEmbedding

        If labels.IsNullOrEmpty Then
            labels = projection _
                .Select(Function(r, i) $"item_{i + 1}") _
                .ToArray
        End If
        If colNames.IsNullOrEmpty Then
            ' ensure not null
            colNames = projection(Scan0) _
                .Select(Function(r, i) $"dimension_{i + 1}") _
                .ToArray
        End If

        Dim nsize As Integer = projection(Scan0).Length
        Dim oldSize As Integer = colNames.TryCount

        If oldSize < nsize Then
            ReDim Preserve colNames(nsize - 1)

            For i As Integer = oldSize To nsize - 1
                colNames(i) = $"dim_{i}"
            Next
        End If

        For i As Integer = 0 To colNames.Length - 1
#Disable Warning
            table.columns(colNames(i)) = projection _
                .Select(Function(r) r(i)) _
                .ToArray
#Enable Warning
        Next

        table.rownames = labels

        Return table
    End Function

    ''' <summary>
    ''' UMAP: Uniform Manifold Approximation and Projection for Dimension Reduction
    ''' </summary>
    ''' <param name="data">data must be normalized! matrix value could be a dataframe object, or clr type <see cref="INumericMatrix"/>.</param>
    ''' <param name="dimension">
    ''' default 2, The dimension of the space to embed into.
    ''' </param>
    ''' <param name="numberOfNeighbors">
    ''' default 15, The size of local neighborhood (in terms of number of neighboring sample points) 
    ''' used for manifold approximation.
    ''' </param>
    ''' <param name="customMapCutoff">
    ''' cutoff value in range ``[0,1]``
    ''' </param>
    ''' <param name="customNumberOfEpochs">
    ''' default None, The number of training epochs to be used in optimizing the low dimensional embedding. 
    ''' Larger values result in more accurate embeddings.
    ''' </param>
    ''' <param name="KDsearch">
    ''' knn search via KD-tree?
    ''' </param>
    ''' <param name="localConnectivity">
    ''' default 1, The local connectivity required -- i.e. the number of nearest neighbors that should 
    ''' be assumed to be connected at a local level.
    ''' </param>
    ''' <param name="setOpMixRatio">
    ''' default 1.0, The value of this parameter should be between 0.0 and 1.0; a value of 1.0 will use 
    ''' a pure fuzzy union, while 0.0 will use a pure fuzzy intersection.
    ''' </param>
    ''' <param name="minDist">
    ''' default 0.1, The effective minimum distance between embedded points.
    ''' </param>
    ''' <param name="spread">
    ''' default 1.0, The effective scale of embedded points. In combination with ``min_dist`` this determines 
    ''' how clustered/clumped the embedded points are.
    ''' </param>
    ''' <param name="learningRate">
    ''' default 1.0, The initial learning rate for the embedding optimization.
    ''' </param>
    ''' <param name="repulsionStrength">
    ''' default 1.0, Weighting applied to negative samples in low dimensional embedding optimization.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("umap")>
    <RApiReturn("labels", "umap")>
    Public Function umapProjection(<RRawVectorArgument> data As Object,
                                   Optional dimension% = 2,
                                   Optional numberOfNeighbors As Integer = 15,
                                   Optional localConnectivity As Double = 1,
                                   Optional KnnIter As Integer = 64,
                                   Optional bandwidth As Double = 1,
                                   Optional customNumberOfEpochs As Integer? = Nothing,
                                   Optional customMapCutoff As Double? = Nothing,
                                   Optional debug As Boolean = False,
                                   Optional KDsearch As Boolean = False,
                                   Optional spectral_cos As Boolean = True,
                                   Optional setOpMixRatio As Double = 1,
                                   Optional minDist As Double = 0.1F,
                                   Optional spread As Double = 1,
                                   Optional repulsionStrength As Double = 1,
                                   Optional learningRate As Double = 1.0F,
                                   Optional env As Environment = Nothing) As Object
        Dim labels As String()
        Dim matrix As Double()()
        Dim report As RunSlavePipeline.SetProgressEventHandler
        Dim dist As DistanceCalculation

        If spectral_cos Then
            dist = AddressOf DistanceFunctions.SpectralSimilarity
        Else
            dist = AddressOf DistanceFunctions.CosineForNormalizedVectors
        End If

        If TypeOf data Is Rdataframe Then
            labels = DirectCast(data, Rdataframe).getRowNames
            matrix = DirectCast(data, Rdataframe) _
                .forEachRow _
                .Select(Function(r)
                            ' convert each row as vector
                            Return CLRVector.asNumeric(r.value)
                        End Function) _
                .ToArray
        ElseIf data.GetType.ImplementInterface(Of INumericMatrix) Then
            matrix = DirectCast(data, INumericMatrix).ArrayPack
            labels = matrix _
                .Select(Function(r, i) i.ToString) _
                .ToArray
        Else
            Dim raw As pipeline = pipeline.TryCreatePipeline(Of DataSet)(data, env)

            If raw.isError Then
                Return raw.getError
            End If

            Dim rawMatrix As DataSet() = raw.populates(Of DataSet)(env).ToArray
            Dim cols As String() = rawMatrix.PropertyNames

            labels = rawMatrix.Keys(distinct:=False)
            matrix = rawMatrix.Select(Function(r) r(cols)).ToArray
        End If

        If debug OrElse env.globalEnvironment.debugMode Then
            report = AddressOf RunSlavePipeline.SendProgress
        Else
            report = Sub()
                         ' do nothing
                     End Sub
        End If

        Dim umap As New Umap(
            distance:=dist,
            dimensions:=dimension,
            numberOfNeighbors:=numberOfNeighbors,
            localConnectivity:=localConnectivity,
            KnnIter:=KnnIter,
            bandwidth:=bandwidth,
            customNumberOfEpochs:=customNumberOfEpochs,
            customMapCutoff:=customMapCutoff,
            progressReporter:=report,
            spread:=spread,
            kdTreeKNNEngine:=KDsearch,
            setOpMixRatio:=setOpMixRatio,
            minDist:=minDist,
            learningRate:=learningRate,
            repulsionStrength:=repulsionStrength
        )
        Dim nEpochs As Integer

        Call Console.WriteLine("Initialize fit..")

        nEpochs = umap.InitializeFit(matrix)

        Console.WriteLine("- Done")
        Console.WriteLine()
        Console.WriteLine("Calculating..")

        Call umap.Step(nEpochs)

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"labels", labels},
                {"umap", umap}
            }
        }
    End Function

    ''' <summary>
    ''' Extract the umap graph
    ''' </summary>
    ''' <param name="umap"></param>
    ''' <param name="labels"></param>
    ''' <param name="groups"></param>
    ''' <param name="threshold"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function asGraph(umap As Umap,
                            <RRawVectorArgument>
                            labels As Object,
                            <RRawVectorArgument>
                            Optional groups As Object = Nothing,
                            Optional threshold As Double = 0,
                            Optional env As Environment = Nothing) As Object

        Dim labelList As String() = CLRVector.asCharacter(labels)
        Dim uniqueLabels As String() = labelList.makeNames(unique:=True)
        Dim g As NetworkGraph = umap.CreateGraph(uniqueLabels, labelList, threshold:=threshold)

        If Not groups Is Nothing Then
            labelList = CLRVector.asCharacter(groups)

            Call base.print("cluster groups that you defined for the nodes:", , env)
            Call base.print(labelList.Distinct.OrderBy(Function(str) str).ToArray, , env)

            For i As Integer = 0 To uniqueLabels.Length - 1
                g.GetElementByID(uniqueLabels(i)).data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = labelList(i)
            Next
        End If

        Return g
    End Function
End Module

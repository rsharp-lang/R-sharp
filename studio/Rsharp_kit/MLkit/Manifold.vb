#Region "Microsoft.VisualBasic::dd767d6a3840a7f41b36afef706536f4, studio\Rsharp_kit\MLkit\Manifold.vb"

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

    ' Module Manifold
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: asGraph, exportUmapTable, plot, umapProjection
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' UMAP: Uniform Manifold Approximation and Projection for Dimension Reduction
''' </summary>
<Package("umap")>
Module Manifold

    Sub New()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(Umap), AddressOf exportUmapTable)
        Call Internal.generic.add("plot", GetType(Umap), AddressOf plot)
    End Sub

    Private Function plot(input As Umap, args As list, env As Environment) As GraphicsData
        Dim size$ = InteropArgumentHelper.getSize(args!size)
        Dim pointSize# = args.getValue("point_size", env, 15.0)
        Dim showLabels As Boolean = args.getValue("show_labels", env, False)
        Dim labels As String() = args.getValue(Of String())("labels", env)
        Dim labelStyle$ = args.getValue("label_style", env, CSSFont.Win10Normal)
        Dim labelColor$ = args.getValue("label_color", env, "black")
        Dim clusters As list = args.getValue(Of list)("clusters", env)
        Dim bubbleAlpha As Integer = args.getValue("bubble_alpha", env, 0.0) * 255
        Dim clusterData As Dictionary(Of String, String) = Nothing

        If Not clusters Is Nothing Then
            clusterData = clusters.slots _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return Scripting.ToString([single](a.Value))
                              End Function)
        End If

        If input.dimension = 2 Then
            Return input.DrawUmap2D(size:=size, labels:=labels, clusters:=clusterData)
        Else
            Dim camera As Camera = args.getValue(Of Camera)("camera", env)

            If camera Is Nothing Then
                env.AddMessage("the 3D camera is nothing, default camera value will be apply!", MSG_TYPES.WRN)
                camera = New Camera With {
                    .screen = size.SizeParser,
                    .angleX = 120,
                    .angleY = 120,
                    .angleZ = 30,
                    .fov = 1500,
                    .viewDistance = 500
                }
            Else
                size = $"{camera.screen.Width},{camera.screen.Height}"
            End If

            Return input.DrawUmap3D(
                camera:=camera,
                size:=size,
                showLabels:=showLabels,
                pointSize:=pointSize,
                labels:=labels,
                labelCSS:=labelStyle,
                clusters:=clusterData,
                labelColor:=labelColor,
                bubbleAlpha:=bubbleAlpha
            )
        End If
    End Function

    Private Function exportUmapTable(umap As Umap, args As list, env As Environment) As Rdataframe
        Dim labels As String() = REnv.asVector(Of String)(args.getByName("labels"))
        Dim colNames As String() = REnv.asVector(Of String)(args.getByName("dimension"))
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
            colNames = projection(Scan0) _
                .Select(Function(r, i) $"dimension_{i + 1}") _
                .ToArray
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
    ''' <param name="data"></param>
    ''' <param name="dimension"></param>
    ''' <param name="customMapCutoff">
    ''' cutoff value in range ``[0,1]``
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("umap")>
    <RApiReturn(GetType(list))>
    Public Function umapProjection(<RRawVectorArgument> data As Object,
                                   Optional dimension% = 2,
                                   Optional numberOfNeighbors As Integer = 15,
                                   Optional localConnectivity As Double = 1,
                                   Optional KnnIter As Integer = 64,
                                   Optional bandwidth As Double = 1,
                                   Optional customNumberOfEpochs As Integer? = Nothing,
                                   Optional customMapCutoff As Double? = Nothing,
                                   Optional env As Environment = Nothing) As Object
        Dim labels As String()
        Dim matrix As Double()()

        If TypeOf data Is Rdataframe Then
            labels = DirectCast(data, Rdataframe).getRowNames
            matrix = DirectCast(data, Rdataframe) _
                .forEachRow _
                .Select(Function(r)
                            Return r.Select(Function(n) CDbl(n)).ToArray
                        End Function) _
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

        Dim umap As New Umap(
            distance:=AddressOf DistanceFunctions.CosineForNormalizedVectors,
            dimensions:=dimension,
            numberOfNeighbors:=numberOfNeighbors,
            localConnectivity:=localConnectivity,
            KnnIter:=KnnIter,
            bandwidth:=bandwidth,
            customNumberOfEpochs:=customNumberOfEpochs,
            customMapCutoff:=customMapCutoff
        )
        Dim nEpochs As Integer

        Call Console.WriteLine("Initialize fit..")

        nEpochs = umap.InitializeFit(matrix)

        Console.WriteLine("- Done")
        Console.WriteLine()
        Console.WriteLine("Calculating..")

        For i As Integer = 0 To nEpochs - 1
            Call umap.Step()

            If (100 * i / nEpochs) Mod 5 = 0 Then
                Console.WriteLine($"- Completed {i + 1} of {nEpochs} [{CInt(100 * i / nEpochs)}%]")
            End If
        Next

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"labels", labels},
                {"umap", umap}
            }
        }
    End Function

    <ExportAPI("as.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function asGraph(umap As Umap, <RRawVectorArgument> labels As Object,
                            <RRawVectorArgument>
                            Optional groups As Object = Nothing,
                            Optional threshold As Double = 0,
                            Optional env As Environment = Nothing) As Object

        Dim labelList As String() = REnv.asVector(Of String)(labels)
        Dim uniqueLabels As String() = labelList.makeNames(unique:=True)
        Dim g As NetworkGraph = umap.CreateGraph(uniqueLabels, labelList, threshold:=threshold)

        If Not groups Is Nothing Then
            labelList = REnv.asVector(Of String)(groups)

            Call base.print("cluster groups that you defined for the nodes:", env)
            Call base.print(labelList.Distinct.OrderBy(Function(str) str).ToArray, env)

            For i As Integer = 0 To uniqueLabels.Length - 1
                g.GetElementByID(uniqueLabels(i)).data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = labelList(i)
            Next
        End If

        Return g
    End Function
End Module

#Region "Microsoft.VisualBasic::d31c5265220070ff37138a07f221f023, R-sharp\studio\Rsharp_kit\MLkit\dataset\datasetKit.vb"

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

    '   Total Lines: 193
    '    Code Lines: 144
    ' Comment Lines: 28
    '   Blank Lines: 21
    '     File Size: 7.86 KB


    ' Module datasetKit
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: demoMatrix, dimensionRange, EmbeddingRender, getNormalizeMatrix, readMNISTLabelledVector
    '               readModelDataset, Tabular
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports DataTable = Microsoft.VisualBasic.Data.csv.IO.DataSet
Imports FeatureFrame = Microsoft.VisualBasic.Math.DataFrame.DataFrame
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' the machine learning dataset toolkit
''' </summary>
<Package("dataset", Category:=APICategories.UtilityTools)>
Module datasetKit

    Sub New()
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(FeatureFrame), AddressOf toDataframe)
    End Sub

    Private Function toDataframe(features As FeatureFrame, args As list, env As Environment) As Rdataframe
        Return New Rdataframe With {
            .columns = features.features _
                .ToDictionary(Function(v) v.Key,
                              Function(v)
                                  Return v.Value.vector
                              End Function),
            .rownames = features.rownames
        }
    End Function

    <ExportAPI("toFeatureSet")>
    <RApiReturn(GetType(FeatureFrame))>
    Public Function toFeatureSet(x As Rdataframe, Optional env As Environment = Nothing) As Object
        Dim featureSet As New Dictionary(Of String, FeatureVector)
        Dim general As Array

        For Each name As String In x.columns.Keys
            general = x(columnName:=name)
            general = TryCastGenericArray(general, env)

            If Not FeatureVector.CheckSupports(general.GetType.GetElementType) Then
                Return Internal.debug.stop($"not supports '{name}'!", env)
            End If

            featureSet(name) = FeatureVector.FromGeneral(general)
        Next

        Return New FeatureFrame With {
            .rownames = x.getRowNames,
            .features = featureSet
        }
    End Function

    Friend Function EmbeddingRender(input As IDataEmbedding, args As list, env As Environment) As GraphicsData
        Dim size$ = InteropArgumentHelper.getSize(args!size, env)
        Dim pointSize# = args.getValue("point_size", env, 15.0)
        Dim showLabels As Boolean = args.getValue("show_labels", env, False)
        Dim showBubble As Boolean = args.getValue("show_bubble", env, False)
        Dim labels As String() = args.getValue(Of String())("labels", env)
        Dim labelStyle$ = args.getValue("label_style", env, CSSFont.Win10Normal)
        Dim labelColor$ = args.getValue("label_color", env, "black")
        ' [label => clusterid]
        Dim clusters As list = args.getValue(Of list)("clusters", env)
        Dim bubbleAlpha As Integer = args.getValue("bubble_alpha", env, 0.0) * 255
        Dim legendLabelCSS$ = args.getValue("legendlabel", env, CSSFont.PlotLabelNormal)
        Dim colors As String = args.getValue("colorSet", env, "Clusters")
        Dim padding As String = args.getValue("padding", env, "padding:150px 150px 300px 300px;")
        Dim clusterData As Dictionary(Of String, String) = Nothing

        If Not clusters Is Nothing Then
            clusterData = clusters.slots _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return any.ToString([single](a.Value))
                              End Function)
        End If

        If input.dimension = 2 Then
            Return input.DrawEmbedding2D(
                size:=size,
                labels:=labels,
                clusters:=clusterData,
                pointSize:=pointSize,
                showConvexHull:=showBubble,
                legendLabelCSS:=legendLabelCSS,
                colorSet:=colors,
                padding:=padding
            )
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

            Return input.DrawEmbedding3D(
                camera:=camera,
                size:=size,
                showLabels:=showLabels,
                pointSize:=pointSize,
                labels:=labels,
                labelCSS:=labelStyle,
                clusters:=clusterData,
                labelColor:=labelColor,
                bubbleAlpha:=bubbleAlpha,
                colorSet:=colors,
                padding:=padding
            )
        End If
    End Function

    ''' <summary>
    ''' get the normalization matrix from a given machine learning training dataset.
    ''' </summary>
    ''' <param name="dataset"></param>
    ''' <returns></returns>
    <ExportAPI("normalize_matrix")>
    Public Function getNormalizeMatrix(dataset As DataSet) As NormalizeMatrix
        Return dataset.NormalizeMatrix
    End Function

    ''' <summary>
    ''' convert machine learning dataset to dataframe table.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="markOuput"></param>
    ''' <returns></returns>
    <ExportAPI("as.tabular")>
    Public Function Tabular(x As DataSet, Optional markOuput As Boolean = True) As DataTable()
        Return x.ToTable(markOuput).ToArray
    End Function

    ''' <summary>
    ''' read the dataset for training the machine learning model
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("read.ML_model")>
    Public Function readModelDataset(file As String) As DataSet
        Return file.LoadXml(Of DataSet)
    End Function

    <ExportAPI("read.mnist.labelledvector")>
    Public Function readMNISTLabelledVector(messagepack As String, Optional takes As Integer = -1) As dataframe
        Using file As Stream = messagepack.Open(IO.FileMode.Open, doClear:=False, [readOnly]:=True)
            Return LabelledVector.CreateDataFrame(MsgPackSerializer.Deserialize(Of LabelledVector())(file), takes)
        End Using
    End Function

    ''' <summary>
    ''' create demo matrix for run test
    ''' </summary>
    ''' <param name="size">number of rows</param>
    ''' <param name="dimensions">number of columns</param>
    ''' <param name="pzero">percentage of zero in an entity vector</param>
    ''' <param name="nclass">number of class tags</param>
    ''' <returns></returns>
    <ExportAPI("gaussian")>
    Public Function demoMatrix(size As Integer, dimensions As Integer,
                               Optional pzero As Double = 0.8,
                               Optional nclass% = 5) As dataframe

        Dim tagRanges = nclass _
            .Sequence _
            .Select(Function(tag)
                        Return New NamedCollection(Of Func(Of Double))($"class_{tag + 1}", dimensionRange(dimensions, pzero))
                    End Function) _
            .ToArray
        Dim dataset As New List(Of NamedValue(Of Double()))

        For i As Integer = 1 To size
            Dim tag = tagRanges(randf.NextInteger(nclass))
            Dim vec = tag.Select(Function(p) p()).ToArray

            dataset.Add(New NamedValue(Of Double()) With {.Name = tag.name, .Value = vec, .Description = i})
        Next

        Dim matrix As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        For i As Integer = 0 To dimensions - 1
#Disable Warning
            matrix.columns($"X{i + 1}") = dataset _
                .Select(Function(d) d.Value(i)) _
                .ToArray
#Enable Warning
        Next

        Return matrix
    End Function

    Private Iterator Function dimensionRange(dimensions As Integer, pzero As Double) As IEnumerable(Of Func(Of Double))
        For i As Integer = 0 To dimensions - 1
            Dim min As Double = randf.NextInteger(10000000)
            Dim max As Double = min * 100

            Yield Function()
                      If randf.NextDouble <= pzero Then
                          Return 0
                      Else
                          Return randf.NextDouble(min, max)
                      End If
                  End Function
        Next
    End Function

End Module

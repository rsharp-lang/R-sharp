#Region "Microsoft.VisualBasic::2ba536a1ee5d77cb8ba3a74cb16f1ce7, studio\Rsharp_kit\MLkit\dataset\datasetKit.vb"

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

' Module datasetKit
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: getNormalizeMatrix, readMNISTLabelledVector, readModelDataset, Tabular
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting
Imports DataTable = Microsoft.VisualBasic.Data.csv.IO.DataSet

''' <summary>
''' the machine learning dataset toolkit
''' </summary>
<Package("dataset", Category:=APICategories.UtilityTools)>
Module datasetKit

    Sub New()

    End Sub

    Friend Function EmbeddingRender(input As IDataEmbedding, args As list, env As Environment) As GraphicsData
        Dim size$ = InteropArgumentHelper.getSize(args!size)
        Dim pointSize# = args.getValue("point_size", env, 15.0)
        Dim showLabels As Boolean = args.getValue("show_labels", env, False)
        Dim showBubble As Boolean = args.getValue("show_bubble", env, False)
        Dim labels As String() = args.getValue(Of String())("labels", env)
        Dim labelStyle$ = args.getValue("label_style", env, CSSFont.Win10Normal)
        Dim labelColor$ = args.getValue("label_color", env, "black")
        ' [label => clusterid]
        Dim clusters As list = args.getValue(Of list)("clusters", env)
        Dim bubbleAlpha As Integer = args.getValue("bubble_alpha", env, 0.0) * 255
        Dim legendLabelCSS$ = args.getValue("legendlabel_style", env, CSSFont.PlotLabelNormal)
        Dim colors As String = args.getValue("colorSet", env, "Set1:c8")
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
                colorSet:=colors
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
                colorSet:=colors
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
End Module

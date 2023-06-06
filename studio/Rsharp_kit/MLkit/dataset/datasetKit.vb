#Region "Microsoft.VisualBasic::6b03cd43a706fc87b281c6e12fdf80f0, F:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//dataset/datasetKit.vb"

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

'   Total Lines: 411
'    Code Lines: 320
' Comment Lines: 37
'   Blank Lines: 54
'     File Size: 16.64 KB


' Class UnionMatrix
' 
'     Function: CreateMatrix
' 
'     Sub: Add
' 
' Module datasetKit
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: addRow, binEncoder, boolEncoder, dataDescription, demoMatrix
'               dimensionRange, EmbeddingRender, Encoding, factorEncoder, getNormalizeMatrix
'               mapEncoder, mapLambda, readMNISTLabelledVector, readModelDataset, Tabular
'               toDataframe, toFeatureSet, toMatrix
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.FeatureFrame
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.Debugger
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports DataTable = Microsoft.VisualBasic.Data.csv.IO.DataSet
Imports FeatureFrame = Microsoft.VisualBasic.Math.DataFrame.DataFrame
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

Public Class UnionMatrix

    ReadOnly records As New List(Of NamedValue(Of list))

    Public Sub Add(recordName As String, data As list)
        records.Add(New NamedValue(Of list)(recordName, data))
    End Sub

    Public Function CreateMatrix() As Rdataframe
        Dim allFeatures As String() = records _
            .Select(Function(v) v.Value.getNames) _
            .IteratesALL _
            .ToArray _
            .DoCall(AddressOf CLRVector.asCharacter) _
            .Distinct _
            .ToArray
        Dim rownames As String() = records.Select(Function(a) a.Name).uniqueNames
        Dim matrix As New Dictionary(Of String, Array)

        For Each name As String In allFeatures
            Dim v As Object() = records _
                .Select(Function(a)
                            Return If(a.Value.hasName(name), REnv.single(a.Value.getByName(name)), 0.0)
                        End Function) _
                .ToArray

            Call matrix.Add(name, CLRVector.asNumeric(v))
        Next

        Return New Rdataframe With {
            .rownames = rownames,
            .columns = matrix
        }
    End Function

End Class

''' <summary>
''' the machine learning dataset toolkit
''' </summary>
<Package("dataset", Category:=APICategories.UtilityTools)>
<RTypeExport("data_matrix", GetType(UnionMatrix))>
Module datasetKit

    Sub New()
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(FeatureFrame), AddressOf toDataframe)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(UnionMatrix), AddressOf toMatrix)
        Call REnv.Internal.generic.add("fit", GetType(SequenceGraphTransform), AddressOf fitSgt)
    End Sub

    Private Function toMatrix(data As UnionMatrix, args As list, env As Environment) As Rdataframe
        Return data.CreateMatrix
    End Function

    Private Function fitSgt(sgt As SequenceGraphTransform, args As list, env As Environment) As Object
        Dim sequence As Object = args.getBySynonyms("sequence", "seq", "seqs", "sequences")
        Dim result = env.EvaluateFramework(Of String, Dictionary(Of String, Double))(sequence, AddressOf sgt.fit)

        Return result
    End Function

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

    <ExportAPI("SGT")>
    Public Function SGT(Optional kappa As Double = 1, Optional lengthsensitive As Boolean = False) As SequenceGraphTransform
        Return New SequenceGraphTransform(kappa:=kappa, lengthsensitive:=lengthsensitive)
    End Function

    <ExportAPI("add_sample")>
    Public Function addRow(matrix As UnionMatrix, sampleId As String, data As list) As UnionMatrix
        Call matrix.Add(sampleId, data)
        Return matrix
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

            featureSet(name) = FeatureVector.FromGeneral(name, general)
        Next

        Return New FeatureFrame With {
            .rownames = x.getRowNames,
            .features = featureSet
        }
    End Function

    <ExportAPI("description")>
    Public Function dataDescription(x As Object, Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is Rdataframe Then
            x = datasetKit.toFeatureSet(x, env)
        End If

        If TypeOf x Is Message Then
            Return x
        ElseIf Not TypeOf x Is FeatureFrame Then
            Return Message.InCompatibleType(GetType(FeatureFrame), x.GetType, env)
        End If

        Return New Rdataframe With {
            .rownames = FeatureDescription _
                .GetDescriptions _
                .ToArray,
            .columns = DirectCast(x, FeatureFrame).features _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return FeatureDescription.DescribFeature(a.Value)
                              End Function)
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
    Public Function readMNISTLabelledVector(messagepack As String, Optional takes As Integer = -1) As Rdataframe
        Using file As Stream = messagepack.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
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
                               Optional nclass% = 5) As Rdataframe

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

        Dim matrix As New Rdataframe With {
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

    ''' <summary>
    ''' do feature encoding
    ''' </summary>
    ''' <param name="features"></param>
    ''' <param name="encoder">
    ''' a set of the encoder function that apply to the 
    ''' corresponding feature data.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("encoding")>
    <RApiReturn(GetType(FeatureFrame))>
    Public Function Encoding(features As FeatureFrame,
                             <RListObjectArgument>
                             encoder As list,
                             Optional env As Environment = Nothing) As Object

        Dim encoderMaps As New Encoder

        For Each fieldName As String In encoder.getNames
            Dim code As Object = encoder.getByName(fieldName)
            Dim err As Message = Nothing

            If TypeOf code Is RMethodInfo Then
                err = mapEncoder(DirectCast(code, RMethodInfo).GetNetCoreCLRDeclaration.Name, fieldName, encoderMaps, env)
            ElseIf TypeOf code Is DeclareLambdaFunction Then
                err = encoderMaps.mapLambda(DirectCast(code, DeclareLambdaFunction), env)
            Else
                Return Internal.debug.stop(New NotImplementedException($"{fieldName} -> {code.GetType.FullName}"), env)
            End If

            If Not err Is Nothing Then
                Return err
            End If
        Next

        Return encoderMaps.Encoding(features)
    End Function

    <Extension>
    Private Function mapLambda(encoderMaps As Encoder, lambda As DeclareLambdaFunction, env As Environment) As Object
        Dim fieldName = lambda.parameterNames.First
        Dim code = lambda.closure.Evaluate(env)

        If Program.isException(code) Then
            Return code
        End If

        If TypeOf code Is SymbolReference Then
            Return mapEncoder(DirectCast(code, SymbolReference).symbol, fieldName, encoderMaps, env)
        ElseIf TypeOf code Is NamespaceFunctionSymbolReference Then
            code = DirectCast(code, NamespaceFunctionSymbolReference).symbol
            Return mapEncoder(DirectCast(code, SymbolReference).symbol, fieldName, encoderMaps, env)
        ElseIf TypeOf code Is RMethodInfo Then
            Return mapEncoder(DirectCast(code, RMethodInfo).GetNetCoreCLRDeclaration.Name, fieldName, encoderMaps, env)
        ElseIf TypeOf code Is FeatureEncoder Then
            encoderMaps.AddEncodingRule(fieldName, DirectCast(code, FeatureEncoder))
        Else
            Return Internal.debug.stop(New NotImplementedException($"{fieldName} -> {code.GetType.FullName}"), env)
        End If

        Return Nothing
    End Function

    Private Function mapEncoder(code As String, fieldName As String, encoderMaps As Encoder, env As Environment) As Message
        Select Case code
            Case NameOf(binEncoder), "to_bins"
                encoderMaps.AddEncodingRule(fieldName, binEncoder)
            Case NameOf(factorEncoder), "to_factors"
                encoderMaps.AddEncodingRule(fieldName, factorEncoder)
            Case NameOf(boolEncoder), "to_ints"
                encoderMaps.AddEncodingRule(fieldName, boolEncoder)
            Case Else
                Return Internal.debug.stop(New NotImplementedException($"{fieldName} -> {code}"), env)
        End Select

        Return Nothing
    End Function

    <ExportAPI("to_bins")>
    Public Function binEncoder(Optional nbins As Integer = 3, Optional format As String = "G4") As FeatureEncoder
        Return New NumericBinsEncoder(nbins, format)
    End Function

    <ExportAPI("to_factors")>
    Public Function factorEncoder() As EnumEncoder
        Return New EnumEncoder
    End Function

    <ExportAPI("to_ints")>
    Public Function boolEncoder() As FlagEncoder
        Return New FlagEncoder
    End Function
End Module

#Region "Microsoft.VisualBasic::a9bff2c6c1fbb09fb7035b756237c991, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//MachineLearning/CNN.vb"

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

'   Total Lines: 71
'    Code Lines: 40
' Comment Lines: 22
'   Blank Lines: 9
'     File Size: 2.44 KB


' Module CNN
' 
'     Function: detectObject, loadModel, saveModel
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports layer = Microsoft.VisualBasic.MachineLearning.CNN.Layer
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' feed-forward phase of deep Convolutional Neural Networks
''' </summary>
<Package("CNN")>
<RTypeExport("cnn", GetType(LayerBuilder))>
Module CNNTools

    <ExportAPI("cnn")>
    Public Function cnn_new() As LayerBuilder
        Return New LayerBuilder
    End Function

    <ROperator("+")>
    Public Function addLayer(cnn As LayerBuilder, layer As layer) As LayerBuilder
        Return cnn.add(layer)
    End Function

    <ExportAPI("input_layer")>
    Public Function input_layer(<RRawVectorArgument> size As Object) As layer
        Dim sz As Integer() = CLRVector.asInteger(size)
        Dim sz_val As New Dimension(sz(0), sz(1))

        Return layer.buildInputLayer(sz_val)
    End Function

    <ExportAPI("conv_layer")>
    Public Function conv_layer(outMapNum As Integer, <RRawVectorArgument> kernelSize As Object) As layer
        Dim kn_sz As Integer() = CLRVector.asInteger(kernelSize)
        Dim sz_val As New Dimension(kn_sz(0), kn_sz(1))

        Return layer.buildConvLayer(outMapNum, sz_val)
    End Function

    <ExportAPI("samp_layer")>
    Public Function samp_layer(<RRawVectorArgument> scaleSize As Object) As layer
        Dim sz As Integer() = CLRVector.asInteger(scaleSize)
        Dim scale As New Dimension(sz(0), sz(1))

        Return layer.buildSampLayer(scale)
    End Function

    <ExportAPI("output_layer")>
    Public Function output_layer(class_num As Integer) As layer
        Return layer.buildOutputLayer(class_num)
    End Function

    <ExportAPI("training")>
    <RApiReturn(GetType(CNN))>
    Public Function training(cnn As Object, dataset As Object,
                             <RRawVectorArgument>
                             Optional labels As Object = Nothing,
                             Optional max_loops As Integer = 100,
                             Optional batch_size As Integer? = Nothing,
                             Optional env As Environment = Nothing) As Object
        Dim cnn_val As CNN
        Dim batchSize As Integer
        Dim ds As SampleData()

        If TypeOf dataset Is dataframe Then
            Dim df As dataframe = DirectCast(dataset, dataframe)

            If TypeOf labels Is String Then
                Dim label As Double() = CLRVector.asNumeric(df(CStr(labels)))

                df = df.projectByColumn({CStr(labels)}, env, reverse:=True)
                ds = df.forEachRow _
                    .Select(Function(r, i)
                                Return New SampleData(CLRVector.asNumeric(r.value), label(i)) With {
                                    .id = r.name
                                }
                            End Function) _
                    .ToArray
            Else
                labels = REnv.TryCastGenericArray(REnv.asVector(Of Object)(labels), env)

                If DataFramework.IsNumericCollection(labels.GetType) Then
                    Dim label As Double() = CLRVector.asNumeric(labels)

                    ds = df.forEachRow() _
                        .Select(Function(r, i)
                                    Return New SampleData(CLRVector.asNumeric(r.value), label(i)) With {
                                        .id = r.name
                                    }
                                End Function) _
                        .ToArray
                Else
                    Return Message.InCompatibleType(GetType(String), labels.GetType, env)
                End If
            End If
        Else
            Return Message.InCompatibleType(GetType(dataframe), dataset.GetType, env)
        End If

        If batch_size Is Nothing Then
            batchSize = ds.Length / 250
        Else
            batchSize = CInt(batch_size)
        End If

        If TypeOf cnn Is CNN Then
            cnn_val = cnn
        ElseIf TypeOf cnn Is LayerBuilder Then
            cnn_val = New CNN(layerBuilder:=cnn, batchSize:=batchSize)
        Else
            Return Message.InCompatibleType(GetType(CNN), cnn.GetType, env)
        End If

        cnn_val = cnn_val.train(ds, max_loops)

        Return cnn_val
    End Function

    <ExportAPI("predict")>
    Public Function predict(cnn As CNN, dataset As Object,
                            <RRawVectorArgument>
                            Optional class_labels As Object = "class_%d",
                            Optional env As Environment = Nothing) As Object

        Dim ds As SampleData()

        If TypeOf dataset Is dataframe Then
            ds = DirectCast(dataset, dataframe) _
                .forEachRow _
                .Select(Function(r)
                            Return New SampleData(CLRVector.asNumeric(r.value)) With {
                                .id = r.name
                            }
                        End Function) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(dataframe), dataset.GetType, env)
        End If

        Dim result As New dataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = ds.Select(Function(d) d.id).ToArray
        }
        Dim outputs As New List(Of Double())
        Dim class_types As String()

        Call layer.prepareForNewBatch()

        For Each sample As SampleData In ds
            Call outputs.Add(cnn.predict(sample))
        Next

        If TypeOf class_labels Is String Then
            If outputs(0).Length <> 1 Then
                ' is template
                class_types = Enumerable.Range(1, outputs(0).Length) _
                    .Select(Function(i) CStr(class_labels).Replace("%d", i)) _
                    .ToArray
            Else
                ' is single
                class_types = {CStr(class_labels)}
            End If
        ElseIf class_labels Is Nothing Then
            class_types = Enumerable.Range(1, outputs(0).Length) _
                .Select(Function(i) $"class_{i}") _
                .ToArray
        Else
            class_types = CLRVector.asCharacter(class_labels)
        End If

        For i As Integer = 0 To outputs(0).Length - 1
            Call result.add(class_types(i), outputs.Select(Function(r) r(i)))
        Next

        Return result
    End Function

    ''' <summary>
    ''' load a CNN model from file
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    <ExportAPI("CeNiN")>
    Public Function loadModel(file As String) As CeNiN
        Return New CeNiN(file)
    End Function

    ''' <summary>
    ''' classify a object from a given image data
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="target"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("detectObject")>
    Public Function detectObject(model As CeNiN, target As Bitmap, Optional env As Environment = Nothing) As dataframe
        Dim result As NamedValue(Of Double)() = model.DetectObject(target, dev:=env.globalEnvironment.stdout)
        Dim classes As String() = result.Select(Function(v) v.Name).ToArray
        Dim probs As Double() = result.Select(Function(v) v.Value).ToArray

        Return New dataframe With {
            .columns = New Dictionary(Of String, Array) From {
                {"class", classes},
                {"probs", probs}
            }
        }
    End Function

    ''' <summary>
    ''' save the CNN model into a binary data file
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("saveModel")>
    Public Function saveModel(model As CeNiN, file As Object, Optional env As Environment = Nothing) As Object
        Dim buffer = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Dim result As Boolean = model.Save(buffer)

        If TypeOf file Is String Then
            Call buffer.TryCast(Of Stream).Dispose()
        End If

        Return result
    End Function
End Module

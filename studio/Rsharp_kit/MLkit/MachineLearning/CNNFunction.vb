﻿#Region "Microsoft.VisualBasic::ba0c53b8c962fbb31c8de1131709201b, studio\Rsharp_kit\MLkit\MachineLearning\CNNFunction.vb"

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

    '   Total Lines: 120
    '    Code Lines: 97 (80.83%)
    ' Comment Lines: 5 (4.17%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (15.00%)
    '     File Size: 4.92 KB


    ' Class CNNFunction
    ' 
    '     Function: (+2 Overloads) DoPrediction, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports renv = SMRUCC.Rsharp.Runtime

Public Class CNNFunction : Inherits RDefaultFunction

    Friend cnn As ConvolutionalNN

    Public Overrides Function ToString() As String
        Return $"CNN<{cnn.ToString}>(...);"
    End Function

    Public Shared Function DoPrediction(cnn As ConvolutionalNN, dataset As Object,
                                        <RRawVectorArgument>
                                        Optional class_labels As Object = "class_%d",
                                        Optional is_generative As Boolean = False,
                                        Optional scalar_vector As Boolean = False,
                                        Optional env As Environment = Nothing) As Object
        Dim ds As SampleData()

        If TypeOf dataset Is vector Then
            dataset = DirectCast(dataset, vector).data
            dataset = renv.TryCastGenericArray(dataset, env)
        End If

        If Program.isException(dataset) Then
            Return dataset
        End If

        If TypeOf dataset Is dataframe Then
            ds = DirectCast(dataset, dataframe) _
                .forEachRow _
                .Select(Function(r)
                            Return New SampleData(CLRVector.asNumeric(r.value)) With {
                                .id = r.name
                            }
                        End Function) _
                .ToArray
        ElseIf TypeOf dataset Is SampleData() OrElse TypeOf dataset Is SampleData Then
            ds = renv.asVector(Of SampleData)(dataset)
        Else
            Return Message.InCompatibleType(GetType(dataframe), dataset.GetType, env)
        End If

        Dim result As New dataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = ds.Select(Function(d) d.id).ToArray
        }
        Dim outputs As New List(Of Double())
        Dim class_types As String()
        Dim data As New DataBlock(cnn.input.dims, cnn.input.out_depth, c:=0)
        Dim dd As Integer = ds.Length / 50
        Dim i As i32 = 0

        ds = SampleData.TransformDataset(ds, is_generative:=is_generative, is_training:=False).ToArray

        For Each sample As SampleData In ds
            Call data.addImageData(sample.features, 1.0)
            Call outputs.Add(cnn.predict(data))

            ' 20241125 just contains one sample data andalso user required
            ' of returns a vector data
            ' then returns the vector result directly from here
            If scalar_vector AndAlso ds.Length = 1 Then
                Return outputs(Scan0)
            End If

            If dd > 0 AndAlso ++i Mod dd = 0 Then
                Call VBDebugger.EchoLine($"[{i}/{ds.Length}] {(i / ds.Length * 100).ToString("F2")}% ... {sample.id}")
            End If
        Next

        If TypeOf class_labels Is String Then
            If outputs(0).Length <> 1 Then
                ' is template
                class_types = Enumerable.Range(1, outputs(0).Length) _
                    .Select(Function(ind) CStr(class_labels).Replace("%d", ind)) _
                    .ToArray
            Else
                ' is single
                class_types = {CStr(class_labels)}
            End If
        ElseIf class_labels Is Nothing Then
            class_types = Enumerable.Range(1, outputs(0).Length) _
                .Select(Function(ind) $"class_{ind}") _
                .ToArray
        Else
            class_types = CLRVector.asCharacter(class_labels)
        End If

        For idx As Integer = 0 To outputs(0).Length - 1
#Disable Warning
            Call result.add(class_types(idx), outputs.Select(Function(r) r(idx)))
#Enable Warning
        Next

        Return result
    End Function

    <RDefaultFunction>
    Public Function DoPrediction(<RRawVectorArgument> dataset As Object,
                                 <RRawVectorArgument>
                                 Optional class_labels As Object = "class_%d",
                                 Optional is_generative As Boolean = False,
                                 Optional scalar_vector As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

        Return DoPrediction(cnn, dataset, class_labels, is_generative, scalar_vector, env)
    End Function

End Class

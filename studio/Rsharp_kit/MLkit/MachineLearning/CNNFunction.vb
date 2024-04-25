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
                                 Optional env As Environment = Nothing) As Object

        Return DoPrediction(cnn, dataset, class_labels, is_generative, env)
    End Function

End Class

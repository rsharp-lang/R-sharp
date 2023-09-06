Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

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

        If TypeOf dataset Is dataframe Then
            ds = DirectCast(dataset, dataframe) _
                .forEachRow _
                .Select(Function(r)
                            Return New SampleData(CLRVector.asNumeric(r.value)) With {
                                .id = r.name
                            }
                        End Function) _
                .ToArray
        ElseIf TypeOf dataset Is SampleData() Then
            ds = DirectCast(dataset, SampleData())
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

        ds = SampleData.TransformDataset(ds, is_generative:=is_generative, is_training:=False).ToArray

        For Each sample As SampleData In ds
            Call data.addImageData(sample.features, 1.0)
            Call outputs.Add(cnn.predict(data))
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
#Disable Warning
            Call result.add(class_types(i), outputs.Select(Function(r) r(i)))
#Enable Warning
        Next

        Return result
    End Function

    <RDefaultFunction>
    Public Function DoPrediction(dataset As Object,
                                 <RRawVectorArgument>
                                 Optional class_labels As Object = "class_%d",
                                 Optional is_generative As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

        Return DoPrediction(cnn, dataset, class_labels, is_generative, env)
    End Function

End Class

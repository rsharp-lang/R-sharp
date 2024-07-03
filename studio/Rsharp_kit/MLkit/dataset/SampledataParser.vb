Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization

Module SampledataParser

    Public Function Convert(x As Object, env As Environment) As [Variant](Of Message, SampleData())
        If x Is Nothing Then
            Return Internal.debug.stop("the required sample data should not be nothing", env)
        End If

        Throw New NotImplementedException
    End Function

    Public Function ConvertVAE(x As Object, env As Environment) As [Variant](Of Message, SampleData())
        If x Is Nothing Then
            Return Internal.debug.stop("the required sample data x should not be nothing!", env)
        End If

        If TypeOf x Is dataframe Then
            Dim rows = DirectCast(x, dataframe).forEachRow.ToArray

            Return rows _
                .Select(Function(a)
                            Dim v As Double() = CLRVector.asNumeric(a.value)

                            Return New SampleData With {
                                .id = a.name,
                                .features = v,
                                .labels = v
                            }
                        End Function) _
                .ToArray
        ElseIf x.GetType.ImplementInterface(Of INumericMatrix) Then
            Dim rows = DirectCast(x, INumericMatrix).ArrayPack
            Dim labels As String()

            If x.GetType.ImplementInterface(Of ILabeledMatrix) Then
                labels = DirectCast(x, ILabeledMatrix).GetLabels.ToArray
            Else
                labels = rows.Select(Function(r, i) $"r_{i + 1}").ToArray
            End If

            Return rows _
                .Select(Function(r, i)
                            Return New SampleData With {
                                .id = labels(i),
                                .labels = r,
                                .features = r
                            }
                        End Function) _
                .ToArray
        ElseIf TypeOf x Is list Then
            Return DirectCast(x, list).slots _
                .Select(Function(r)
                            Dim v As Double() = CLRVector.asNumeric(r.Value)

                            Return New SampleData With {
                                .id = r.Key,
                                .features = v,
                                .labels = v
                            }
                        End Function) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(dataframe), x.GetType, env)
        End If
    End Function
End Module

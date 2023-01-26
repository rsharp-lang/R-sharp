Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.DataFrame
Imports REnv = SMRUCC.Rsharp.Runtime

Public Module FeatureDescription

    Public Function GetDescriptions() As IEnumerable(Of String)
        Return {
            "min", "max", "width", "mean", "sd", "rsd"
        }
    End Function

    <Extension>
    Public Function DescribFeature(feature As FeatureVector) As Array
        If feature.type Is GetType(Boolean) Then
            Return DescribLogical(feature.vector)
        ElseIf DataFramework.IsNumericType(feature.type) Then
            Return DescribNumeric(REnv.asVector(Of Double)(feature.vector))
        ElseIf feature.type Is GetType(String) Then
            Return DescribCharacter(feature.vector)
        Else
            Throw New NotImplementedException
        End If
    End Function

    Private Function DescribNumeric(v As Double()) As Double()
        Return {
            v.Min, v.Max, New DoubleRange(v).Length, v.Average, v.SD, v.RSD
        }
    End Function

    Private Function DescribCharacter(v As String()) As String()
        Dim order = v.OrderBy(Function(s) s).ToArray
        Dim counts = order _
            .GroupBy(Function(s) s) _
            .OrderByDescending(Function(g) g.Count) _
            .ToArray

        Return {
            order.First, order.Last, counts.Length.ToString, counts.First.Key, "NA", "NA"
        }
    End Function

    Private Function DescribLogical(v As Boolean()) As Boolean()
        Dim logicalGroup = v.GroupBy(Function(b) b).OrderByDescending(Function(g) g.Count).ToArray

        Return {
            False, True, True, logicalGroup.First.Key, False, False
        }
    End Function
End Module

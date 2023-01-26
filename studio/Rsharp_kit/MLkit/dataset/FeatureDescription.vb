Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Math.DataFrame
Imports REnv = SMRUCC.Rsharp.Runtime

Public Module FeatureDescription

    Public Function GetDescriptions() As IEnumerable(Of String)
        Return {
            "type",
            "min", "max", "width", "mean", "sd"
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

    End Function

    Private Function DescribCharacter(v As String()) As String()

    End Function

    Private Function DescribLogical(v As Boolean()) As Double()

    End Function
End Module

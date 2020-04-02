Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Public Class activation

    Public Property hidden As String
    Public Property output As String

    Public Function CreateActivations() As LayerActives
        Dim defaultActive As [Default](Of String) = ActiveFunction.Sigmoid

        Return New LayerActives With {
            .hiddens = ActiveFunction.Parse(hidden Or defaultActive),
            .output = ActiveFunction.Parse(output Or defaultActive),
            .Input = ActiveFunction.Parse(defaultActive)
        }
    End Function

    Public Overloads Shared Widening Operator CType([default] As String) As activation
        Dim tokens As String() = [default].StringSplit(";\s*")
        Dim actives As New activation
        Dim configs As NamedValue(Of String)() = tokens _
            .Select(Function(str)
                        Return str.GetTagValue(":", trim:=True)
                    End Function) _
            .ToArray

        actives.hidden = configs.Where(Function(a) a.Name.TextEquals(NameOf(activation.hidden))).FirstOrDefault.Value
        actives.output = configs.Where(Function(a) a.Name.TextEquals(NameOf(activation.output))).FirstOrDefault.Value

        Return actives
    End Operator
End Class

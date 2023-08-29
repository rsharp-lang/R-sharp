Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Public Class CNNLayerArguments

    Public Property type As String
    Public Property args As list

    Public Function CreateLayer(cnn As LayerBuilder) As LayerBuilder
        Select Case type
            Case NameOf(CNNTools.input_layer) : cnn.buildInputLayer(args!dims, args!depth)
            Case Else
                Throw New NotImplementedException(type)
        End Select

        Return cnn
    End Function

End Class

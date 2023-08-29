Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Public Class CNNLayerArguments

    Public Property type As String
    Public Property args As list

    Public Function CreateLayer(cnn As LayerBuilder) As LayerBuilder
        Select Case type
            Case NameOf(CNNTools.input_layer) : cnn.buildInputLayer(args!dims, args!depth)
            Case NameOf(CNNTools.conv_layer) : cnn.buildConvLayer(args!sx, args!filters, args!stride, args!padding)
            Case NameOf(CNNTools.softmax_layer) : cnn.buildSoftmaxLayer()
            Case NameOf(CNNTools.pool_layer) : cnn.buildPoolLayer(args!sx, args!stride, args!padding)
            Case NameOf(CNNTools.dropout_layer) : cnn.buildDropoutLayer()

            Case Else
                Throw New NotImplementedException(type)
        End Select

        Return cnn
    End Function

End Class

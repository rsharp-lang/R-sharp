Imports Microsoft.VisualBasic.MachineLearning.CNN
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Public Class CNNLayerArguments

    Public Property type As String
    Public Property args As list

    Public Overrides Function ToString() As String
        Return type
    End Function

    Public Function CreateLayer(cnn As LayerBuilder) As LayerBuilder
        Select Case type
            Case NameOf(CNNTools.input_layer) : cnn.buildInputLayer(args!dims, args!depth)
            Case NameOf(CNNTools.conv_layer) : cnn.buildConvLayer(args!sx, args!filters, args!stride, args!padding)
            Case NameOf(CNNTools.softmax_layer) : cnn.buildSoftmaxLayer()
            Case NameOf(CNNTools.pool_layer) : cnn.buildPoolLayer(args!sx, args!stride, args!padding)
            Case NameOf(CNNTools.dropout_layer) : cnn.buildDropoutLayer(args!drop_prob)
            Case NameOf(CNNTools.relu_layer) : cnn.buildReLULayer()
            Case NameOf(CNNTools.leaky_relu_layer) : cnn.buildLeakyReLULayer()
            Case NameOf(CNNTools.sigmoid_layer) : cnn.buildSigmoidLayer()
            Case NameOf(CNNTools.lrn_layer) : cnn.buildLocalResponseNormalizationLayer(args!n)
            Case NameOf(CNNTools.tanh_layer) : cnn.buildTanhLayer()
            Case NameOf(CNNTools.full_connected_layer) : cnn.buildFullyConnectedLayer(args!size)
            Case NameOf(CNNTools.regression_layer) : cnn.buildRegressionLayer()
            Case NameOf(CNNTools.conv_transpose_layer) : cnn.buildConv2DTransposeLayer(args!filter, args!filters, args!stride)

            Case Else
                Throw New NotImplementedException(type)
        End Select

        Return cnn
    End Function

End Class

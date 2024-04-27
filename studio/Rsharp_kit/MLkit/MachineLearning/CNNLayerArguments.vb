#Region "Microsoft.VisualBasic::62ea2046e4f8207e081043c7f06dc6d0, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//MachineLearning/CNNLayerArguments.vb"

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

    '   Total Lines: 37
    '    Code Lines: 30
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.80 KB


    ' Class CNNLayerArguments
    ' 
    '     Properties: args, type
    ' 
    '     Function: CreateLayer, ToString
    ' 
    ' /********************************************************************************/

#End Region

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
            Case NameOf(CNNTools.conv_transpose_layer) : cnn.buildConv2DTransposeLayer(args!dims, args!filter, args!filters, args!stride)
            Case NameOf(CNNTools.gaussian_layer) : cnn.buildGaussian()

            Case Else
                Throw New NotImplementedException(type)
        End Select

        Return cnn
    End Function

End Class

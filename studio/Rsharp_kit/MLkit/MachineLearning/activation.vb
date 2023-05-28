#Region "Microsoft.VisualBasic::1542ca2855fb0ae21bd9bc7eb71936d8, F:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//MachineLearning/activation.vb"

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

    '   Total Lines: 35
    '    Code Lines: 28
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.45 KB


    ' Class activation
    ' 
    '     Properties: hidden, output
    ' 
    '     Function: CreateActivations
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations

Public Class activation

    Public Property hidden As String
    Public Property output As String

    Public Function CreateActivations() As LayerActives
        Dim defaultActive As [Default](Of String) = ActiveFunction.Sigmoid

        Return New LayerActives With {
            .hiddens = ActiveFunction.Parse(hidden Or defaultActive),
            .output = ActiveFunction.Parse(output Or defaultActive)
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

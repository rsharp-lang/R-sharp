#Region "Microsoft.VisualBasic::1f07f0d97ae4e81cc27b18832e277376, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/signalKit//LinearFunction.vb"

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

    '   Total Lines: 16
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 524 B


    ' Class LinearFunction
    ' 
    '     Function: GetSignal
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Public Class LinearFunction : Inherits RDefaultFunction

    Friend linear As Resampler

    <RDefaultFunction>
    Public Function GetSignal(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim x_vec As Double() = CLRVector.asNumeric(x)
        Dim y_vec As Double() = linear(x_vec)

        Return y_vec
    End Function
End Class

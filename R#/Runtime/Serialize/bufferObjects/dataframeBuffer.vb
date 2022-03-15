#Region "Microsoft.VisualBasic::67097b2bf6394753c3faf188598c27b3, R-sharp\R#\Runtime\Serialize\bufferObjects\dataframeBuffer.vb"

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

    '   Total Lines: 26
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 718.00 B


    '     Class dataframeBuffer
    ' 
    '         Properties: code
    ' 
    '         Function: getFrame, getValue
    ' 
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Serialize

    Public Class dataframeBuffer : Inherits BufferObject

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.dataframe
            End Get
        End Property

        Public Overrides Sub Serialize(buffer As Stream)
            Throw New NotImplementedException()
        End Sub

        Public Function getFrame() As dataframe
            Throw New NotImplementedException
        End Function

        Public Overrides Function getValue() As Object
            Return getFrame()
        End Function
    End Class
End Namespace

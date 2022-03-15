#Region "Microsoft.VisualBasic::20a9d2b1932668b0c56b2a2c46a09d65, R-sharp\R#\Runtime\Serialize\bufferObjects\rawBuffer.vb"

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


     Code Statistics:

        Total Lines:   35
        Code Lines:    23
        Comment Lines: 3
        Blank Lines:   9
        File Size:     928.00 B


    '     Class rawBuffer
    ' 
    '         Properties: buffer, code
    ' 
    '         Function: getEmptyBuffer, getValue
    ' 
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace Runtime.Serialize

    ''' <summary>
    ''' a vector of raw bytes
    ''' </summary>
    Public Class rawBuffer : Inherits BufferObject

        Public Property buffer As MemoryStream

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.raw
            End Get
        End Property

        Public Shared Function getEmptyBuffer() As rawBuffer
            Return New rawBuffer With {.buffer = New MemoryStream()}
        End Function

        Public Overrides Sub Serialize(buffer As Stream)
            Dim tmp As Byte() = Me.buffer.ToArray

            Call buffer.Write(tmp, Scan0, tmp.Length)
            Call buffer.Flush()

            Erase tmp
        End Sub

        Public Overrides Function getValue() As Object
            Return buffer
        End Function
    End Class
End Namespace

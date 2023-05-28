#Region "Microsoft.VisualBasic::8b25023b8189740b9dc6a2e8df12a413, F:/GCModeller/src/R-sharp/R#//Runtime/Serialize/bufferObjects/rawBuffer.vb"

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

    '   Total Lines: 45
    '    Code Lines: 31
    ' Comment Lines: 3
    '   Blank Lines: 11
    '     File Size: 1.20 KB


    '     Class rawBuffer
    ' 
    '         Properties: buffer, code
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getEmptyBuffer, getValue
    ' 
    '         Sub: loadBuffer, Serialize
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

        Sub New()
        End Sub

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

        Protected Overrides Sub loadBuffer(stream As Stream)
            buffer = New MemoryStream
            stream.CopyTo(buffer)
            buffer.Flush()
            buffer.Seek(Scan0, SeekOrigin.Begin)
        End Sub
    End Class
End Namespace

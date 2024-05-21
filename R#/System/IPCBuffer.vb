#Region "Microsoft.VisualBasic::d95f3c8cca9d18fbf66b80756303ebdf, R#\System\IPCBuffer.vb"

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

    '   Total Lines: 68
    '    Code Lines: 54 (79.41%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 14 (20.59%)
    '     File Size: 2.39 KB


    '     Class IPCBuffer
    ' 
    '         Properties: buffer, chunksize, requestId
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ParseBuffer, ToString
    ' 
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports DataBuffer = SMRUCC.Rsharp.Runtime.Serialize.Buffer

Namespace Development

    Public Class IPCBuffer : Inherits RawStream

        Public Property requestId As String
        Public Property buffer As DataBuffer
        Public Property chunksize As Integer

        Sub New(request_id As String, buffer As DataBuffer)
            Me.requestId = request_id
            Me.buffer = buffer
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{requestId}] {buffer.code.Description} in {chunksize} bytes"
        End Function

        Public Shared Function ParseBuffer(buffer As Stream) As IPCBuffer
            Dim int_size As Byte() = New Byte(3) {}
            Dim sizeof As Integer
            Dim bytes As Byte()

            buffer.Read(int_size, Scan0, int_size.Length)
            sizeof = BitConverter.ToInt32(int_size, Scan0)
            bytes = New Byte(sizeof - 1) {}
            buffer.Read(bytes, Scan0, sizeof)

            Dim id As String = Encoding.UTF8.GetString(bytes)
            Dim data As DataBuffer
            Dim size As Integer

            buffer.Read(int_size, Scan0, int_size.Length)
            sizeof = BitConverter.ToInt32(int_size, Scan0)

            Using ms As MemoryStream = BufferObject.SubStream(buffer, 4 + bytes.Length + 4, sizeof)
                data = DataBuffer.ParseBuffer(ms)
                size = ms.Length
            End Using

            Return New IPCBuffer With {
                .buffer = data,
                .requestId = id,
                .chunksize = size
            }
        End Function

        Public Overrides Sub Serialize(memory As Stream)
            Dim text As Byte() = Encoding.UTF8.GetBytes(requestId)
            Dim int_size As Byte() = BitConverter.GetBytes(text.Length)
            Dim bytes As Byte() = buffer.Serialize

            Call memory.Write(int_size, Scan0, 4)
            Call memory.Write(text, Scan0, text.Length)
            Call memory.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
            Call memory.Write(bytes, Scan0, bytes.Length)
            Call memory.Flush()
        End Sub
    End Class
End Namespace

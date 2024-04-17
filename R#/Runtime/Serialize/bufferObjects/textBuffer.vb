#Region "Microsoft.VisualBasic::2fb9fed14c2331cff8fb88c9925b67dc, D:/GCModeller/src/R-sharp/R#//Runtime/Serialize/bufferObjects/textBuffer.vb"

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

    '   Total Lines: 62
    '    Code Lines: 45
    ' Comment Lines: 3
    '   Blank Lines: 14
    '     File Size: 1.64 KB


    '     Class textBuffer
    ' 
    '         Properties: code, text
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: getValue
    ' 
    '         Sub: loadBuffer, Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text

Namespace Runtime.Serialize

    ''' <summary>
    ''' utf8 text data
    ''' </summary>
    Public Class textBuffer : Inherits BufferObject

        Public Property text As String
        ''' <summary>
        ''' the content text mime type
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>used for the http service.</remarks>
        Public Property mime As String

        Public Overrides ReadOnly Property code As BufferObjects
            Get
                Return BufferObjects.text
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(buffer As Stream)
            Call loadBuffer(buffer)
        End Sub

        Sub New(raw As Byte())
            Using ms As New MemoryStream(raw)
                Call loadBuffer(ms)
            End Using
        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Dim data As Byte() = Encodings.UTF8.CodePage.GetBytes(text)
            Dim mime As Byte() = Encoding.ASCII.GetBytes(If(Me.mime, "plain/text"))

            Call buffer.Write(BitConverter.GetBytes(mime.Length), Scan0, RawStream.INT32)
            Call buffer.Write(mime, Scan0, mime.Length)
            Call buffer.Write(data, Scan0, data.Length)
            Call buffer.Flush()

            Erase data
        End Sub

        Public Overrides Function getValue() As Object
            Return text
        End Function

        Protected Overrides Sub loadBuffer(stream As Stream)
            Dim raw As Byte()

            If TypeOf stream Is MemoryStream Then
                raw = DirectCast(stream, MemoryStream).ToArray
            Else
                Using buffer As New MemoryStream
                    stream.CopyTo(buffer)
                    raw = buffer.ToArray
                End Using
            End If

            Dim buf As Byte() = New Byte(RawStream.INT32 - 1) {}
            Dim len As Integer
            Dim offset As Integer

            Array.ConstrainedCopy(raw, 0, buf, 0, buf.Length)
            len = BitConverter.ToInt32(buf, 0)
            buf = New Byte(len - 1) {}
            offset = RawStream.INT32 + len
            Array.ConstrainedCopy(raw, RawStream.INT32, buf, 0, buf.Length)

            mime = Encoding.ASCII.GetString(buf)
            text = Encodings.UTF8 _
                .CodePage _
                .GetString(raw, offset, raw.Length - offset)
        End Sub
    End Class
End Namespace

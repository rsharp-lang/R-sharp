#Region "Microsoft.VisualBasic::80533ad8b1f70d5f5d141229c4610345, R-sharp\R#\Runtime\Serialize\Buffer.vb"

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

    '   Total Lines: 76
    '    Code Lines: 62
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 2.96 KB


    '     Class Buffer
    ' 
    '         Properties: code, data
    ' 
    '         Function: MeasureBufferMagic, ParseBuffer, ToString
    ' 
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Serialize

    Public Class Buffer : Inherits RawStream

        Public Property data As BufferObject

        Public ReadOnly Property code As BufferObjects
            Get
                Select Case data.GetType
                    Case GetType(rawBuffer) : Return BufferObjects.raw
                    Case GetType(textBuffer) : Return BufferObjects.text
                    Case GetType(bitmapBuffer) : Return BufferObjects.bitmap
                    Case GetType(messageBuffer) : Return BufferObjects.message
                    Case GetType(vectorBuffer) : Return BufferObjects.vector
                    Case Else
                        Throw New NotImplementedException(data.GetType.FullName)
                End Select
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return code.Description
        End Function

        Public Shared Function ParseBuffer(raw As Stream) As Buffer
            Dim code As BufferObjects = MeasureBufferMagic(raw)
            Dim bufferObject As BufferObject
            Dim data As MemoryStream = BufferObject.SubStream(raw, 4, raw.Length - 4)

            Select Case code
                Case BufferObjects.raw
                    bufferObject = New rawBuffer With {.buffer = data}
                Case BufferObjects.text
                    bufferObject = New textBuffer With {.text = Encoding.UTF8.GetString(data.ToArray)}
                Case BufferObjects.bitmap
                    bufferObject = New bitmapBuffer(data)
                Case BufferObjects.vector
                    bufferObject = vectorBuffer.CreateBuffer(data)
                Case BufferObjects.message
                    bufferObject = messageBuffer.CreateBuffer(data)
                Case Else
                    Throw New NotImplementedException(code.Description)
            End Select

            Return New Buffer With {
                .data = bufferObject
            }
        End Function

        Public Shared Function MeasureBufferMagic(raw As Stream) As BufferObjects
            Dim magic As Byte() = New Byte(3) {}

            Call raw.Seek(Scan0, SeekOrigin.Begin)
            Call raw.Read(magic, Scan0, magic.Length)

            Return CType(BitConverter.ToInt32(magic, Scan0), BufferObjects)
        End Function

        Public Overrides Sub Serialize(buffer As Stream)
            Dim chunkBuffer As Byte() = data.Serialize

            Call buffer.Write(BitConverter.GetBytes(CInt(code)), Scan0, 4)
            Call buffer.Write(chunkBuffer, Scan0, chunkBuffer.Length)
            Call buffer.Flush()

            Erase chunkBuffer
        End Sub
    End Class
End Namespace

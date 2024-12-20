﻿#Region "Microsoft.VisualBasic::ca0eef3758e871dde21f9661493ba581, R#\Runtime\Serialize\Buffer.vb"

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

    '   Total Lines: 88
    '    Code Lines: 67 (76.14%)
    ' Comment Lines: 7 (7.95%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 14 (15.91%)
    '     File Size: 3.71 KB


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

Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Serialize

    ''' <summary>
    ''' data buffer model for run ``R#`` IPC
    ''' </summary>
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
                    Case GetType(rscriptBuffer) : Return BufferObjects.rscript
                    Case GetType(listBuffer) : Return BufferObjects.list
                    Case GetType(dataframeBuffer) : Return BufferObjects.dataframe
                    Case GetType(NullObject) : Return BufferObjects.vector
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
                    ' 20220916
                    ' 
                    ' try to make patch to the NULL literal result
                    '
                    If data.Length = 0 Then
                        bufferObject = New NullObject
                    Else
                        bufferObject = New rawBuffer With {
                            .buffer = data
                        }
                    End If
                Case BufferObjects.text : bufferObject = New textBuffer(data)
                Case BufferObjects.bitmap : bufferObject = New bitmapBuffer(data)
                Case BufferObjects.vector : bufferObject = New vectorBuffer(data)
                Case BufferObjects.message : bufferObject = New messageBuffer(data)
                Case BufferObjects.rscript : bufferObject = New rscriptBuffer(data)
                Case BufferObjects.dataframe : bufferObject = New dataframeBuffer(data)
                Case BufferObjects.list : bufferObject = New listBuffer(data)
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

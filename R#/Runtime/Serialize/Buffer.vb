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
                    bufferObject = New bitmapBuffer With {.bitmap = Image.FromStream(data)}
                Case BufferObjects.vector
                    bufferObject = vectorBuffer.CreateBuffer(data)
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Serialize() As Byte()
            Return BitConverter _
                .GetBytes(CInt(code)) _
                .JoinIterates(data.Serialize) _
                .ToArray
        End Function
    End Class
End Namespace
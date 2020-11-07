Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Serialize

    Public Class bitmapBuffer : Inherits RawStream

        Public Property bitmap As Image

        Sub New(bytes As Byte())
            Using ms As New MemoryStream(bytes)
                bitmap = Image.FromStream(ms)
            End Using
        End Sub

        Public Overrides Function Serialize() As Byte()
            Using buffer As New MemoryStream
                Call bitmap.Save(buffer, Imaging.ImageFormat.Png)
                Call buffer.Flush()

                Return buffer.ToArray
            End Using
        End Function
    End Class
End Namespace
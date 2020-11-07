Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Serialize

    Public MustInherit Class BufferObject : Inherits RawStream


        Public Shared Function SubStream(stream As Stream, from As Long, length As Integer) As MemoryStream
            Dim buffer As Byte() = New Byte(length - 1) {}

            stream.Seek(from, SeekOrigin.Begin)
            stream.Read(buffer, Scan0, length)

            Return New MemoryStream(buffer)
        End Function
    End Class
End Namespace
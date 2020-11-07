Imports System.IO

Namespace Runtime.Serialize

    Public Class rawBuffer : Inherits BufferObject

        Public Property buffer As MemoryStream

        Public Overrides Function Serialize() As Byte()
            Return buffer.ToArray
        End Function
    End Class
End Namespace
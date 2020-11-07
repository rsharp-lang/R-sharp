Imports System.IO

Namespace Runtime.Serialize

    Public Class rawBuffer : Inherits BufferObject

        Public Property buffer As MemoryStream

        Public Shared Function getEmptyBuffer() As rawBuffer
            Return New rawBuffer With {.buffer = New MemoryStream()}
        End Function

        Public Overrides Function Serialize() As Byte()
            Return buffer.ToArray
        End Function
    End Class
End Namespace
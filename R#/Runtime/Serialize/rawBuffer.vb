Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Serialize

    Public Class rawBuffer : Inherits RawStream

        Public Property buffer As MemoryStream

        Public Overrides Function Serialize() As Byte()
            Return buffer.ToArray
        End Function
    End Class
End Namespace
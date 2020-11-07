Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text

Namespace Runtime.Serialize

    Public Class textBuffer : Inherits RawStream

        Public Property text As String

        Sub New()
        End Sub

        Sub New(raw As Byte())
            text = Encodings.UTF8.CodePage.GetString(raw)
        End Sub

        Public Overrides Function Serialize() As Byte()
            Return Encodings.UTF8.CodePage.GetBytes(text)
        End Function
    End Class
End Namespace
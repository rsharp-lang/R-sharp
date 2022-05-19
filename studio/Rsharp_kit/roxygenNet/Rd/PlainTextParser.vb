Imports Microsoft.VisualBasic.Emit.Marshal

Public Class PlainTextParser : Inherits RDocParser

    Protected endContent As Boolean = False

    Friend Sub New(text As Pointer(Of Char))
        Me.text = text
    End Sub

    Protected Overrides Sub walkChar(c As Char)
        If c = "}"c Then
            endContent = True
        Else
            buffer += c
        End If
    End Sub

    Public Function GetCurrentText() As String
        Do While Not endContent
            Call walkChar(++text)
        Loop

        Return New String(buffer)
    End Function
End Class
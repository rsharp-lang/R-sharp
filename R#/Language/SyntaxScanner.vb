Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    ''' <summary>
    ''' The token scanner
    ''' </summary>
    Public Class Scanner

        Dim code As CharPtr
        Dim buffer As New List(Of Char)
        Dim escape As New Escapes

        Private Class Escapes

            Friend comment, [string] As Boolean

            Public Overrides Function ToString() As String
                If comment Then
                    Return "comment"
                ElseIf [string] Then
                    Return "string"
                Else
                    Return "code"
                End If
            End Function
        End Class

        Sub New(source As String)
            Me.code = source
        End Sub
    End Class
End Namespace
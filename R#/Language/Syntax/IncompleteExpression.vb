Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language.Syntax

    ''' <summary>
    ''' Helper for R shell terminal multiple line editing
    ''' </summary>
    Public NotInheritable Class IncompleteExpression

        Dim lines As New List(Of String)

        Private Sub New()
        End Sub

        Public Function Append(line As String) As IncompleteExpression
            Call lines.Add(line)
            Return Me
        End Function

        Public Function GetRScript() As Rscript
            Return Rscript.AutoHandleScript(lines.JoinBy(vbCrLf))
        End Function

        ''' <summary>
        ''' test the given line tokens is in-complete expression or not?
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' in-complete expression:
        ''' 
        ''' 1. ends with operator token
        ''' 2. bracket stack not closed
        ''' </remarks>
        Public Shared Function CheckTokenSequence(tokens As Token()) As Boolean
            If tokens.IsNullOrEmpty Then
                ' special case:
                ' empty expression is completed
                Return False
            End If

            ' ends with open:  xxx(
            ' ends with operator: xxx *
            ' ends with comma: xxx(aaa,
            If tokens.Last.name = TokenType.open OrElse
                tokens.Last.name = TokenType.operator OrElse
                tokens.Last.name = TokenType.comma Then

                Return True
            End If

            ' check of the stack closed?


            Return False
        End Function
    End Class
End Namespace
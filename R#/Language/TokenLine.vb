Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Public Class TokenLine

        Public ReadOnly Property tokens As Token()

        ''' <summary>
        ''' the size of the <see cref="tokens"/> array
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property length As Integer
            Get
                Return tokens.Length
            End Get
        End Property

        Default Public ReadOnly Property getToken(i As Integer) As Token
            Get
                If i < 0 Then
                    i = tokens.Length + i
                End If

                Return tokens(i)
            End Get
        End Property

        Sub New(tokens As IEnumerable(Of Token))
            Me.tokens = tokens
        End Sub

        Friend Function StripDelimiterTokens() As TokenLine
            _tokens = tokens _
                .Where(Function(t)
                           Return Not t.name = TokenType.delimiter
                       End Function) _
                .ToArray

            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return tokens.Select(Function(t) t.text).JoinBy(" ")
        End Function
    End Class
End Namespace
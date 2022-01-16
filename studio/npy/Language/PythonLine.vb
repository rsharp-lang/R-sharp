Imports System.Data
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer


Public Class PythonLine

    Public ReadOnly Property tokens As Token()
    Public ReadOnly Property levels As Integer

    Default Public ReadOnly Property Token(i As Integer) As Token
        Get
            If i < 0 Then
                i = tokens.Length + i
            End If

            Return tokens(i)
        End Get
    End Property

    ''' <summary>
    ''' the size of the <see cref="tokens"/> array
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property length As Integer
        Get
            Return tokens.Length
        End Get
    End Property

    Sub New(tokens As IEnumerable(Of Token))
        Me.tokens = tokens.ToArray
        Me.levels = Me.tokens _
            .TakeWhile(Function(t)
                           Return t.name = TokenType.delimiter
                       End Function) _
            .Count
        Me.tokens = Me.tokens _
            .Where(Function(t)
                       Return Not t.name = TokenType.delimiter
                   End Function) _
            .ToArray
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{levels}]" & vbTab & ": " & tokens.Select(Function(t) t.text).JoinBy(" ")
    End Function

End Class

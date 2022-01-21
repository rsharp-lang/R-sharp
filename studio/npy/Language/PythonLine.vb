Imports System.Data
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Class PythonLine : Inherits TokenLine

    Public ReadOnly Property levels As Integer

    Sub New(tokens As Token())
        Call MyBase.New(tokens)

        Me.levels = Me.tokens _
            .TakeWhile(Function(t)
                           Return t.name = TokenType.delimiter
                       End Function) _
            .Count
        Me.StripDelimiterTokens()
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{levels}]" & vbTab & ": " & tokens.Select(Function(t) t.text).JoinBy(" ")
    End Function

End Class

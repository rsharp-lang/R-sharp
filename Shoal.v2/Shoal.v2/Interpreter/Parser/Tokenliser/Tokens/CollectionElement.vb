Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Interpreter.Parser.Tokens

    Public Class CollectionElement : Inherits Token

        Public Property Array As Tokens.InternalExpression
        ''' <summary>
        ''' 单个位置元素或者条件表达式或者一个位置集合
        ''' </summary>
        ''' <returns></returns>
        Public Property Index As Tokens.InternalExpression

        Sub New(Expression As String)
            Call MyBase.New(0, Expression)

            If Expression.First = "["c AndAlso Expression.Last = "]"c Then
                Expression = Mid(Expression, 2, Len(Expression) - 2)
            End If

            Dim Index As String = Regex.Match(Expression, "\[.+\]").Value
            Dim Array As String = If(Not String.IsNullOrEmpty(Index), Expression.Replace(Index, ""), Expression)

            Me.Array = New InternalExpression(Array)
            Index = Mid(Index, 2, Len(Index) - 2)

            Dim Tokens = New Parser.TextTokenliser.MSLTokens().Parsing(Index).Tokens

            If Tokens.Length = 1 Then  ' $var,  ~First,  ~Last,  &const,  
                Me.Index = New Tokens.InternalExpression(Index)
            ElseIf String.Equals(Tokens.First.GetTokenValue, "where", StringComparison.OrdinalIgnoreCase)
                Me.Index = New InternalExpression(String.Join(" ", (From t In Tokens.Skip(1) Select t.GetTokenValue.CliToken).ToArray))
            ElseIf (From t In Tokens Where t.GetTokenValue.Last = "," Select 1).ToArray.Length = Tokens.Length - 1
                Me.Index = New InternalExpression("{" & String.Join(" ", Tokens.ToArray(Of String)([CType]:=Function(t) t.GetTokenValue)) & "}")
            Else
                Throw New SyntaxErrorException(Expression.CliToken & " is not a value element indexing expression!")
            End If
        End Sub
    End Class
End Namespace
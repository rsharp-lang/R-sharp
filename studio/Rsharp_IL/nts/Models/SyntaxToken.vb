Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Class SyntaxToken

    ''' <summary>
    ''' <see cref="Token"/> or <see cref="Expression"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property value As Object
    Public Property index As Integer

    Sub New(i As Integer, t As Token)
        index = i
        value = t
    End Sub

    Sub New(i As Integer, exp As Expression)
        index = i
        value = exp
    End Sub

    Public Overrides Function ToString() As String
        Return $"<{index}> {value.ToString}"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <DebuggerStepThrough>
    Public Function [TryCast](Of T As Class)() As T
        Return TryCast(value, T)
    End Function

    Public Function IsToken(token As TokenType, text As String()) As Boolean
        If Not TypeOf value Is Token Then
            Return False
        End If
        If DirectCast(value, Token).name <> token Then
            Return False
        End If

        Return text.Any(Function(si) si = DirectCast(value, Token).text)
    End Function

    Public Function IsToken(token As TokenType, Optional text As String = Nothing) As Boolean
        If Not TypeOf value Is Token Then
            Return False
        End If
        If DirectCast(value, Token).name <> token Then
            Return False
        End If

        If text Is Nothing Then
            Return True
        Else
            Return text = DirectCast(value, Token).text
        End If
    End Function

    Public Function GetSymbol() As String
        If TypeOf value Is Token Then
            Return DirectCast(value, Token).text
        Else
            Return ValueAssignExpression.GetSymbol(DirectCast(value, Expression))
        End If
    End Function

    Public Shared Operator Like(t As SyntaxToken, type As Type) As Boolean
        If t Is Nothing OrElse t.value Is Nothing Then
            Return False
        Else
            Return t.value.GetType Is type OrElse t.value.GetType.IsInheritsFrom(type, strict:=False)
        End If
    End Operator

    Public Shared Iterator Function Cast(list As IEnumerable(Of SyntaxToken)) As IEnumerable(Of [Variant](Of Expression, String))
        For Each item In list
            If item Like GetType(Token) Then
                Yield New [Variant](Of Expression, String)(item.TryCast(Of Token).text)
            Else
                Yield New [Variant](Of Expression, String)(item.TryCast(Of Expression))
            End If
        Next
    End Function

End Class

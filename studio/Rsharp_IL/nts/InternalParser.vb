Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Public Module InternalParser

    <Extension>
    Public Function ParseTsScript(script As Rscript, Optional debug As Boolean = False) As Program
        Return New SyntaxTree(script, debug).ParseTsScript()
    End Function

    <Extension>
    Public Function ParseTypeScriptLine(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
        Return Expression.CreateExpression(tokens, opts)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="tokens">
    ''' first is open
    ''' last is close
    ''' </param>
    ''' <param name="opts"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' token list is comes from the stack range get, just split into multiple parts via operator
    ''' </remarks>
    <Extension>
    Public Function GetExpression(tokens As SyntaxToken(), fromComma As Boolean, opts As SyntaxBuilderOptions) As Expression
        Dim source As IEnumerable(Of SyntaxToken)

        If fromComma Then
            source = tokens
        Else
            source = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2)
        End If

        Dim blocks = source _
            .Split(
                delimiter:=Function(t)
                               Return t Like GetType(Token) AndAlso t.TryCast(Of Token).name = TokenType.operator
                           End Function,
                deliPosition:=DelimiterLocation.Individual
            ) _
            .ToArray
        Dim parse As SyntaxToken() = New SyntaxToken(blocks.Length - 1) {}
        Dim syntax As SyntaxResult

        For i As Integer = 0 To blocks.Length - 1
            Dim part = blocks(i)

            If part.Length = 1 Then
                If Not part(Scan0) Like GetType(Token) Then
                    ' is expression
                    parse(i) = part(Scan0)
                ElseIf part(Scan0).TryCast(Of Token).name = TokenType.operator Then
                    parse(i) = part(Scan0)
                Else
                    ' is token
                    syntax = part.Select(Function(t) t.TryCast(Of Token)).ParseTypeScriptLine(opts)
                    parse(i) = New SyntaxToken(-1, syntax.expression)
                End If
            Else
                syntax = part.Select(Function(t) t.TryCast(Of Token)).ParseTypeScriptLine(opts)
            End If
        Next

        Select Case tokens(Scan0).TryCast(Of Token).text
            Case "("
            Case "{"
            Case "["
                ' is a vector

            Case Else
                If fromComma Then

                Else
                    Throw New NotImplementedException
                End If
        End Select
    End Function
End Module

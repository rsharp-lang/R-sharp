Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

''' <summary>
''' A temp expression collection for the function invoke parameters
''' </summary>
Public Class ExpressionCollecton : Inherits Expression

    Public Overrides ReadOnly Property type As TypeCodes
    Public Overrides ReadOnly Property expressionName As Rsharp.Development.Package.File.ExpressionTypes

    Public Property expressions As Expression()

    Public Shared Function GetExpressions(exp As Expression) As Expression()
        If TypeOf exp Is ExpressionCollecton Then
            Return DirectCast(exp, ExpressionCollecton).expressions
        Else
            Return {exp}
        End If
    End Function

    Public Overrides Function Evaluate(envir As Rsharp.Runtime.Environment) As Object
        Throw New NotImplementedException()
    End Function
End Class

Public Module InternalParser

    <Extension>
    Public Function ParseTsScript(script As Rscript, Optional debug As Boolean = False) As Program
        Return New SyntaxTree(script, debug).ParseTsScript()
    End Function

    <Extension>
    Public Function ParseTypeScriptLine(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
        Return Expression.CreateExpression(tokens, opts)
    End Function

    <Extension>
    Private Function ParseCommaList(tokens As SyntaxToken(), opts As SyntaxBuilderOptions) As ExpressionCollecton
        Dim blocks = tokens.Split(Function(t) t Like GetType(Token) AndAlso t.TryCast(Of Token).name = TokenType.comma, DelimiterLocation.NotIncludes).ToArray
        Dim parse As Expression() = New Expression(blocks.Length - 1) {}

        For i As Integer = 0 To blocks.Length - 1
            parse(i) = blocks(i).ParseValueExpression(opts)
        Next

        Return New ExpressionCollecton With {
            .expressions = parse
        }
    End Function

    ''' <summary>
    ''' multiple lines expression
    ''' </summary>
    ''' <param name="tokens"></param>
    ''' <param name="opts"></param>
    ''' <returns></returns>
    <Extension>
    Private Function ParseClosure(tokens As SyntaxToken(), opts As SyntaxBuilderOptions) As ClosureExpression
        Dim lines = tokens _
            .Split(Function(t) t Like GetType(Token) AndAlso t.TryCast(Of Token).name = TokenType.terminator) _
            .ToArray
        Dim exps As Expression() = New Expression(lines.Length - 1) {}

        For i As Integer = 0 To lines.Length - 1
            tokens = lines(i)
            exps(i) = tokens.ParseValueExpression(opts)
        Next

        Return New ClosureExpression(exps)
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
        If fromComma Then
            Return tokens.ParseValueExpression(opts)
        Else
            Dim source As IEnumerable(Of SyntaxToken) = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .ToArray

            If tokens(Scan0) Like GetType(Token) AndAlso tokens(Scan0).TryCast(Of Token).text = "{" Then
                ' closure
                Return DirectCast(source, SyntaxToken()).ParseClosure(opts)
            Else
                Return DirectCast(source, SyntaxToken()).ParseCommaList(opts)
            End If
        End If
    End Function

    <Extension>
    Private Function ParseValueExpression(tokens As SyntaxToken(), opts As SyntaxBuilderOptions) As Expression
        Dim blocks = tokens _
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
                parse(i) = New SyntaxToken(-1, syntax.expression)
            End If
        Next

        If parse.Length > 1 Then
            syntax = SyntaxToken.Cast(parse).CreateBinaryExpression(opts)
        Else
            syntax = New SyntaxResult(parse(Scan0).TryCast(Of Expression))
        End If

        Return syntax.expression
    End Function
End Module

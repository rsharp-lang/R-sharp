﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
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

    <Extension>
    Private Function ParseCommaList(tokens As SyntaxToken(), opts As SyntaxBuilderOptions) As SyntaxResult
        Dim blocks = tokens _
            .Split(Function(t) t Like GetType(Token) AndAlso t.TryCast(Of Token).name = TokenType.comma, DelimiterLocation.NotIncludes) _
            .ToArray
        Dim parse As Expression() = New Expression(blocks.Length - 1) {}
        Dim syntax As SyntaxResult

        For i As Integer = 0 To blocks.Length - 1
            syntax = blocks(i).ParseValueExpression(opts)

            If syntax.isException Then
                Return syntax
            Else
                parse(i) = syntax.expression
            End If
        Next

        Return New ExpressionCollection With {
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
    Private Function ParseClosure(tokens As SyntaxToken(), opts As SyntaxBuilderOptions) As SyntaxResult
        Dim lines = tokens _
            .Split(Function(t) t Like GetType(Token) AndAlso t.TryCast(Of Token).name = TokenType.terminator) _
            .ToArray
        Dim exps As Expression() = New Expression(lines.Length - 1) {}
        Dim syntax As SyntaxResult

        For i As Integer = 0 To lines.Length - 1
            tokens = lines(i)
            syntax = tokens.ParseValueExpression(opts)

            If syntax.isException Then
                Return syntax
            Else
                exps(i) = syntax.expression
            End If
        Next

        Return New ClosureExpression(exps)
    End Function

    <Extension>
    Private Iterator Function GetParameters(paramTokens As SyntaxToken(), opts As SyntaxBuilderOptions) As IEnumerable(Of DeclareNewSymbol)
        Dim list = paramTokens _
            .Split(Function(t) t Like GetType(Token) AndAlso t.TryCast(Of Token).name = TokenType.comma, DelimiterLocation.NotIncludes) _
            .ToArray
        Dim exp As Expression

        For Each block As SyntaxToken() In list
            Dim val = block.ParseValueExpression(opts)

            If val.isException Then
                Throw New Exception(val.error.ToString)
            Else
                exp = val.expression
            End If

            If TypeOf exp Is ValueAssignExpression Then
                ' optional parameter
                Dim assign As ValueAssignExpression = DirectCast(exp, ValueAssignExpression)

                ' Yield New DeclareNewSymbol ( )
            ElseIf TypeOf exp Is SymbolReference Then
                Dim symbol As SymbolReference = DirectCast(exp, SymbolReference)

                Yield New DeclareNewSymbol(symbol.symbol, Nothing)
            Else
                Throw New NotImplementedException
            End If
        Next
    End Function

    <Extension>
    Private Function ParseFunctionValue(tokens As SyntaxToken(), opts As SyntaxBuilderOptions) As SyntaxResult
        Dim body As SyntaxToken = tokens(tokens.Length - 2)
        Dim paramTokens As SyntaxToken() = tokens.Skip(1).Take(tokens.Length - 1 - 3).ToArray
        Dim stack As StackFrame = opts.GetStackTrace(tokens(Scan0).TryCast(Of Token))

        paramTokens = paramTokens _
            .Skip(1) _
            .Take(paramTokens.Length - 2) _
            .ToArray

        Return New DeclareNewFunction(
            funcName:=$"<${App.GetNextUniqueName("anonymous_")}>",
            parameters:=paramTokens.GetParameters(opts).ToArray,
            body:=body.TryCast(Of ClosureExpression),
            stackframe:=stack
        )
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
    Public Function GetExpression(tokens As SyntaxToken(), fromComma As Boolean, opts As SyntaxBuilderOptions) As SyntaxResult
        If tokens.IsNullOrEmpty Then
            Return Nothing
        End If
        If fromComma Then
            If tokens.First Like GetType(Token) Then
                Dim tk As Token = tokens.First.TryCast(Of Token)

                If tk.isKeyword("function") Then
                    If tokens.Last Like GetType(Token) AndAlso tokens.Last.TryCast(Of Token) = (TokenType.close, "}") Then
                        Return tokens.ParseFunctionValue(opts)
                    End If
                ElseIf tk.name = TokenType.open Then
                    If Not tokens.Any(Function(t) t Like GetType(Token) AndAlso t.TryCast(Of Token).name = TokenType.close) Then
                        Return New SyntaxResult(New SyntaxError())
                    End If
                End If
            End If

            Return tokens.ParseValueExpression(opts)
        Else
            Dim source As SyntaxToken() = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .ToArray

            If source.Length = 1 AndAlso Not source(Scan0) Like GetType(Token) Then
                Return source(Scan0).TryCast(Of Expression)
            ElseIf source.Length = 0 Then
                Return New ExpressionCollection With {
                    .expressions = {}
                }
            End If

            If tokens(Scan0) Like GetType(Token) AndAlso tokens(Scan0).TryCast(Of Token).text = "{" Then
                If (Not tokens.Any(Function(t) t.isTerminator)) AndAlso tokens.Any(Function(t) t.isComma) AndAlso tokens.Any(Function(t) t.isSequenceSymbol) Then
                    ' is json literal
                    Return source.ParseJSONliteral(opts)
                ElseIf tokens.Any(Function(t) t.isSequenceSymbol) Then
                    ' is json literal
                    ' {x:b}
                    Return source.ParseJSONliteral(opts)
                End If

                ' closure
                Return source.ParseClosure(opts)
            ElseIf source(Scan0).IsToken(TokenType.keyword) Then
                Return source.ParseKeywordExpression(source(Scan0).TryCast(Of Token).text, opts)
            Else
                Return source.ParseCommaList(opts)
            End If
        End If
    End Function

    <Extension>
    Private Function ParseJSONliteral(tokens As SyntaxToken(), opts As SyntaxBuilderOptions) As SyntaxResult
        Dim blocks = tokens.Split(Function(t) t.isComma, DelimiterLocation.NotIncludes)
        Dim json As New List(Of NamedValue(Of Expression))

        For Each part In blocks
            Dim name = {part(0)}.ParseValueExpression(opts)
            Dim value = part.Skip(2).ToArray.ParseValueExpression(opts)

            json.Add(New NamedValue(Of Expression)(InvokeParameter.GetSymbolName(name.expression), value.expression))
        Next

        Return New SyntaxResult(New JSONLiteral(json))
    End Function

    <Extension>
    Private Function ParseKeywordExpression(tokens As SyntaxToken(), keyword As String, opts As SyntaxBuilderOptions) As SyntaxResult
        Static declare_keywords As Index(Of String) = {"var", "let", "const"}

        If keyword Like declare_keywords Then
            Dim symbol = tokens(1)
            Dim value As SyntaxResult
            Dim type As String

            If tokens(2) Like GetType(Token) AndAlso tokens(2).TryCast(Of Token) = (TokenType.operator, "=") Then
                '   0 1 2 ...
                ' var x = xxx
                value = tokens.Skip(3).ToArray.ParseValueExpression(opts)
                type = "any"
            Else
                '     0 123      4 ...
                ' const x:string = xxx
                type = tokens(3).GetSymbol
                value = tokens.Skip(5).ToArray.ParseValueExpression(opts)
            End If

            If value.isException Then
                Return value
            Else
                Return New DeclareNewSymbol(symbol.GetSymbol, opts.GetStackTrace(tokens(Scan0).TryCast(Of Token)), value.expression)
            End If
        ElseIf keyword = "if" Then
            Dim stack = opts.GetStackTrace(tokens(0).TryCast(Of Token))

            If tokens.Last.IsToken(TokenType.close, "}") Then
                ' if(...) {...}
                Dim body = tokens.Skip(tokens.Length - 3).ToArray
                Dim test = tokens.Skip(1).Take(tokens.Length - 1 - body.Length).ToArray
                Dim test_exp = test.GetExpression(fromComma:=False, opts)
                Dim body_exp = body.GetExpression(fromComma:=False, opts)
                Dim [if] As New IfBranch(
                    ifTest:=ExpressionCollection.GetExpressions(test_exp.expression).First,
                    trueClosure:=DirectCast(body_exp.expression, ClosureExpression),
                    stackframe:=stack
                )

                Return New SyntaxResult([if])
            Else
                ' if (...) ...
            End If
        ElseIf keyword = "function" Then
            Return tokens.ParseFunctionValue(opts)
        End If
    End Function

    <Extension>
    Private Function ParseValueExpression(tokens As SyntaxToken(), opts As SyntaxBuilderOptions) As SyntaxResult
        If tokens.IsNullOrEmpty Then
            Return New SyntaxResult(Literal.NULL)
        ElseIf tokens.Length = 3 Then
            Dim test1 = tokens(0) Like GetType(IfBranch)
            Dim test2 = tokens(1).IsToken(TokenType.keyword, "else")
            Dim test3 = tokens(2) Like GetType(Expression)
            Dim iftest As IfBranch = tokens(0).TryCast(Of IfBranch)

            If test1 AndAlso test2 AndAlso test3 Then
                Return New SyntaxResult(New IIfExpression(
                    iftest:=iftest.ifTest,
                    trueResult:=iftest.trueClosure.body,
                    falseResult:=tokens(2).TryCast(Of Expression),
                    stackFrame:=iftest.stackFrame
                ))
            End If
        End If
        If tokens(Scan0) Like GetType(Token) AndAlso tokens(Scan0).TryCast(Of Token).name = TokenType.keyword Then
            Return tokens.ParseKeywordExpression(tokens(0).TryCast(Of Token).text, opts)
        End If

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

        Return syntax
    End Function
End Module
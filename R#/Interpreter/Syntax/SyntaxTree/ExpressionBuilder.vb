#Region "Microsoft.VisualBasic::e143e740957590c109c1d482c5a3eaba, R#\Interpreter\Syntax\SyntaxTree\ExpressionBuilder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module ExpressionBuilder
    ' 
    '         Function: getTupleSymbols, getValueAssign, keywordExpressionHandler, ParseExpression, parseInvoke
    '                   parseSymbolIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Annotation
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser

    Module ExpressionBuilder

        <Extension>
        Friend Function keywordExpressionHandler(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim keyword As String = code(Scan0)(Scan0).text

            Select Case keyword
                Case "let", "const"
                    If code > 4 AndAlso code(2).isKeyword("as") AndAlso code(3).isKeyword("function") Then
                        ' let <name> as function(...) {}
                        ' 申明一个函数
                        ' let <name> = function(...) {}
                        ' 将一个匿名函数赋值给左边的目标变量
                        Return SyntaxImplements.DeclareNewFunction(code, opts)
                    Else
                        Return SyntaxImplements.DeclareNewSymbol(code, keyword = "const", opts)
                    End If
                Case "if" : Return SyntaxImplements.IfClosure(code.Skip(1).IteratesALL, opts)
                Case "else" : Return code.Skip(1).IteratesALL.ToArray.DoCall(Function(tokens) SyntaxImplements.ElseClosure(tokens, opts))
                Case "elseif" : Return code.Skip(1).IteratesALL.DoCall(Function(tokens) SyntaxImplements.ElseIfClosure(tokens, opts))
                Case "return" : Return SyntaxImplements.ReturnValue(code.Skip(1).IteratesALL, opts)
                Case "for" : Return SyntaxImplements.ForLoop(code.Skip(1).IteratesALL, opts)
                Case "from", "aggregate"
                    If code < 3 Then
                        code(Scan0)(Scan0).name = TokenType.identifier
                        Return code.ParseExpression(opts)
                    Else
                        Return SyntaxImplements.LinqExpression(code, opts)
                    End If
                Case "imports" : Return SyntaxImplements.[Imports](code, opts)
                Case "function"
                    Return SyntaxImplements.DeclareAnonymousFunction(code, opts)
                Case "suppress"
                    Dim evaluate As SyntaxResult = code _
                        .Skip(1) _
                        .IteratesALL _
                        .DoCall(Function(tokens)
                                    Return opts.ParseExpression(tokens, opts)
                                End Function)

                    If evaluate.isException Then
                        Return evaluate
                    Else
                        Return New Suppress(evaluate.expression)
                    End If
                Case "modeof", "typeof", "valueof"
                    Return SyntaxImplements.ModeOf(keyword, code.Skip(1).IteratesALL.ToArray, opts)
                Case "require"
                    Return code(1) _
                        .Skip(1) _
                        .Take(code(1).Length - 2) _
                        .ToArray _
                        .DoCall(Function(tokens)
                                    Return SyntaxImplements.Require(tokens, opts)
                                End Function)
                Case "next"
                    ' continute for
                    Return New ContinuteFor
                Case "new"
                    ' create new object
                    Return SyntaxImplements.CreateNewObject(code(Scan0)(Scan0), code.Skip(1).IteratesALL.ToArray, opts)
                Case "break"
                    Return New BreakLoop
                Case "using"
                    Return SyntaxImplements.UsingClosure(code.Skip(1), opts)
                Case "while"
                    Return SyntaxImplements.WhileLoopSyntax.CreateLoopExpression(code, opts)
                Case "try"
                    Dim allSeq = code.IteratesALL.ToArray

                    If allSeq.isAcceptor Then
                        Return TryCatchSyntax.CreateTryError(allSeq, opts)
                    ElseIf allSeq.isFunctionInvoke Then
                        Return TryCatchSyntax.CreateTryError(allSeq, opts)
                    Else
                        allSeq(Scan0).name = TokenType.identifier
                        Return Nothing
                    End If

                Case "switch"

                    Dim allSeq = code.IteratesALL.ToArray

                    If allSeq.isAcceptor Then
                        Return SwitchClosureSyntax.GetSwitchs(allSeq, opts)
                    Else
                        allSeq(Scan0).name = TokenType.identifier
                        Return Nothing
                    End If

                Case Else
                    ' may be it is using keyword as identifier name
                    Return Nothing
            End Select
        End Function

        <Extension>
        Friend Function ParseExpression(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            If code = 0 Then
                Return SyntaxResult.CreateError(New SyntaxErrorException("expressin tokens can not be empty!"), opts)
            ElseIf code(Scan0).isKeyword Then
                Dim expression As SyntaxResult = code.keywordExpressionHandler(opts)

                ' if expression is nothing
                ' then it means the keyword is probably 
                ' using keyword as identifier name
                If Not expression Is Nothing Then
                    Return expression
                Else
                    code(Scan0)(Scan0).name = TokenType.identifier
                End If
            ElseIf code(Scan0)(Scan0) = (TokenType.keyword, "if") AndAlso code.Any(Function(t) t.Length = 1 AndAlso t(Scan0) = (TokenType.keyword, "else")) Then
                ' simple iif expression
                ' if (xxx) a else b
                Return code.IIfExpression(opts)
            End If

            If code = 1 Then
                Dim item As Token() = code(Scan0)

                If item.isLiteral Then
                    Return SyntaxImplements.LiteralSyntax(item(Scan0), opts)
                ElseIf item.isIdentifier Then
                    Return New SymbolReference(item(Scan0))
                ElseIf item.Length = 1 AndAlso item(Scan0).name = TokenType.annotation Then
                    Select Case Strings.LCase(item(Scan0).text)
                        Case "@stop" : Return New SyntaxResult(New BreakPoint)
                        Case "@script" : Return New SyntaxResult(New ScriptSymbol)
                        Case "@home" : Return New SyntaxResult(New HomeSymbol)
                        Case "@host" : Return New SyntaxResult(New HostSymbol)
                        Case "@dir" : Return New SyntaxResult(New ScriptFolder)

                        Case Else
                            Return SyntaxResult.CreateError(New NotImplementedException(item(Scan0).text), opts.SetCurrentRange(item))
                    End Select
                Else
                    Dim ifelse = item.ifElseTriple

                    If ifelse.ifelse Is Nothing Then
                        Return item.CreateTree(opts)
                    Else
                        Return SyntaxImplements.IIfExpression(ifelse.test, ifelse.ifelse, opts)
                    End If
                End If
            ElseIf code.isLambdaFunction Then
                ' is a lambda function
                Return SyntaxImplements.DeclareLambdaFunction(code, opts)
            End If

            If code(Scan0).isIdentifier Then
                If code(1).isOperator Then
                    Dim opText$ = code(1)(Scan0).text

                    If opText = "=" OrElse opText = "<-" Then
                        Return SyntaxImplements.ValueAssign(code, opts)
                    ElseIf opText = "~" Then
                        Return SyntaxImplements.FormulaExpressionSyntax.RunParse(code, opts)
                    End If
                ElseIf code = 2 Then
                    Dim result = parseInvoke(code, opts)

                    If Not result Is Nothing Then
                        Return result
                    End If
                End If
            ElseIf code(1).isOperator("=", "<-") Then
                Return getValueAssign(code, opts)
            ElseIf code = 2 Then
                If code(Scan0).Length = 1 AndAlso code(Scan0)(Scan0) = (TokenType.operator, "$") Then
                    Return SyntaxImplements.FunctionInvoke(code.IteratesALL.ToArray, opts)
                ElseIf code(Scan0).Length = 1 AndAlso code(Scan0)(Scan0) = (TokenType.operator, "!") Then
                    ' not xxxx
                    Dim valExpression As SyntaxResult = opts.ParseExpression(code(1), opts)

                    If valExpression.isException Then
                        Return valExpression
                    Else
                        Return New UnaryNot(valExpression.expression)
                    End If
                ElseIf code(Scan0).isIdentifier OrElse code(Scan0).isKeyword Then
                    Dim result = parseInvoke(code, opts)

                    If Not result Is Nothing Then
                        Return result
                    Else
                        result = parseSymbolIndex(code, opts)

                        If Not result Is Nothing Then
                            Return result
                        End If
                    End If
                ElseIf code(Scan0).Length = 1 AndAlso code(Scan0)(Scan0) = (TokenType.operator, "-") Then
                    Dim number As SyntaxResult = ParseExpression(New List(Of Token()) From {code(1)}, opts)

                    If number.isException Then
                        Return number
                    Else
                        Return New UnaryNumeric("-", number.expression)
                    End If
                End If
            ElseIf code = 3 Then
                If code.isSequenceSyntax Then
                    Dim seq = code(Scan0).SplitByTopLevelDelimiter(TokenType.sequence)
                    Dim from = seq(Scan0)
                    Dim [to] = seq(2)
                    Dim steps As Token() = Nothing

                    Call opts.SetCurrentRange(code.IteratesALL.ToArray)

                    If code > 1 Then
                        If code(1).isKeyword("step") Then
                            steps = code(2)
                        ElseIf code(1).isOperator Then
                            ' do nothing
                            GoTo Binary
                        Else
                            Return SyntaxResult.CreateError(New SyntaxErrorException, opts)
                        End If
                    End If

                    Return SyntaxImplements.SequenceLiteral(from, [to], steps, opts)
                End If
            ElseIf code(Scan0).Length Mod 4 = 0 Then
                Dim firstBlock As Token() = code(Scan0)

                ' 20210421
                ' syntax of user annotation
                ' 
                ' [@symbol primitive literal]
                '
                ' there is no comma symbol in the primitive literal, if there is any
                ' comma symbol in the expression, then it means is a vector literal syntax
                '
                If (Not firstBlock.Any(Function(a) a = (TokenType.comma, ","))) AndAlso
                    firstBlock.First = (TokenType.open, "[") AndAlso
                    firstBlock.Last = (TokenType.close, "]") Then

                    Dim annotations = firstBlock.ParseAnnotations.ToArray
                    Dim result = code.Skip(1).AsList.ParseExpression(opts)
                    Dim attrGroups As NamedValue(Of String())() = annotations _
                        .GroupBy(Function(a) a.Name) _
                        .Select(Function(a)
                                    Return New NamedValue(Of String())(a.Key, a.Select(Function(i) i.Value).ToArray)
                                End Function) _
                        .ToArray

                    If Not result.isException AndAlso TypeOf result.expression Is SymbolExpression Then
                        Call DirectCast(result.expression, SymbolExpression).AddCustomAttributes(attrGroups)
                    End If

                    Return result
                End If
            End If

            If code = 5 AndAlso code(Scan0).isFunctionInvoke Then

                If (code(1).Length = 1 AndAlso code(1)(Scan0) = (TokenType.operator, "%")) AndAlso
                   (code(2).Length = 1 AndAlso (code(2)(Scan0) = (TokenType.keyword, "do") OrElse code(2)(Scan0) = (TokenType.identifier, "do"))) AndAlso
                   (code(3).Length = 1 AndAlso code(3)(Scan0) = (TokenType.operator, "%")) Then

                    If code(4).First = (TokenType.open, "{") AndAlso code(4).Last = (TokenType.close, "}") Then
                        Return FunctionAcceptorSyntax.FunctionAcceptorInvoke(code(Scan0), code(4), opts)
                    End If
                End If
            End If

Binary:
            If code(Scan0).Length = 1 AndAlso code(Scan0)(Scan0) = (TokenType.operator, "~") Then
                Return FormulaExpressionSyntax.GetExpressionLiteral(code, opts)
            Else
                Return code.ParseBinaryExpression(opts)
            End If
        End Function

        ''' <summary>
        ''' a(...)
        ''' </summary>
        ''' <param name="code">code block with 2 elements</param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        Private Function parseInvoke(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim part2 As Token() = code(1)

            If part2(Scan0) = (TokenType.open, "(") AndAlso part2.Last = (TokenType.close, ")") Then
                Dim invoke = part2.SplitByTopLevelDelimiter(TokenType.close)

                If invoke = 2 Then
                    Return SyntaxImplements.FunctionInvoke(code(Scan0)(Scan0), part2, opts)
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' a[...]
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        Friend Function parseSymbolIndex(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim first As SyntaxResult = opts.ParseExpression(code(Scan0), opts)

            If first.isException Then
                Return first
            End If

            If TypeOf first.expression Is SymbolReference Then
                Dim tokens As Token() = code(1) _
                    .Skip(1) _
                    .Take(code(1).Length - 2) _
                    .ToArray

                Return first.expression.SymbolIndexer(
                    tokens:=tokens,
                    opts:=opts.SetCurrentRange(code.IteratesALL.ToArray)
                )
            Else
                Return Nothing
            End If
        End Function

        Private Function getValueAssign(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            ' tuple value assign
            ' or member reference assign
            Dim target As Token() = code(Scan0)
            Dim value As Token() = code.Skip(2).IteratesALL.ToArray
            Dim symbol As Expression()

            If target.isSimpleSymbolIndexer Then
                Dim syntaxTemp As SyntaxResult = SyntaxImplements.SymbolIndexer(target, opts)

                If syntaxTemp.isException Then
                    Return syntaxTemp
                Else
                    symbol = {syntaxTemp.expression}
                End If
            ElseIf target.isFunctionInvoke Then
                ' func(x) <- vals
                ' byref calls
                Dim vals As SyntaxResult = code _
                    .Skip(2) _
                    .IteratesALL _
                    .DoCall(Function(tokens)
                                Return opts.ParseExpression(tokens, opts)
                            End Function)

                If vals.isException Then
                    Return vals
                Else
                    Dim calls As SyntaxResult = SyntaxImplements.FunctionInvoke(target, opts)
                    Dim sourceMap As StackFrame = opts.GetStackTrace(target(Scan0), target(Scan0).text)

                    If calls.isException Then
                        Return calls
                    Else
                        Return New ByRefFunctionCall(calls.expression, vals.expression, sourceMap)
                    End If
                End If
            ElseIf target.Length >= 3 Then
                ' the exception is always the last one
                With target.Skip(1) _
                           .Take(code(Scan0).Length - 2) _
                           .DoCall(Function(tokens)
                                       Return getTupleSymbols(tokens, opts)
                                   End Function) _
                           .ToArray

                    If .Last.isException Then
                        Return .Last
                    Else
                        symbol = .Select(Function(e) e.expression) _
                                 .ToArray
                    End If
                End With
            Else
                With opts.ParseExpression(target, opts)
                    If .isException Then
                        Return .ByRef
                    Else
                        symbol = { .expression}
                    End If
                End With
            End If

            Dim valExpression As SyntaxResult = opts.ParseExpression(value, opts)

            If valExpression.isException Then
                Return valExpression
            Else
                Return New ValueAssignExpression(symbol, valExpression.expression)
            End If
        End Function

        Private Iterator Function getTupleSymbols(target As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As IEnumerable(Of SyntaxResult)
            For Each token As SyntaxResult In target.SplitByTopLevelDelimiter(TokenType.comma) _
                                                    .Where(Function(t) Not t.isComma) _
                                                    .Select(Function(tokens)
                                                                Return opts.ParseExpression(tokens, opts)
                                                            End Function)
                Yield token

                If token.isException Then
                    Exit For
                End If
            Next
        End Function
    End Module
End Namespace

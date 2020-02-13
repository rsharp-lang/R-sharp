#Region "Microsoft.VisualBasic::f5bbb071c3addd000fae69af0061b287, R#\Interpreter\Syntax\SyntaxTree\ExpressionBuilder.vb"

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
    '         Function: getTupleSymbols, getValueAssign, keywordExpressionHandler, ParseExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser

    Module ExpressionBuilder

        <Extension>
        Friend Function keywordExpressionHandler(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim keyword As String = code(Scan0)(Scan0).text

            Select Case keyword
                Case "let"
                    If code > 4 AndAlso code(3).isKeyword("function") Then
                        Return SyntaxImplements.DeclareNewFunction(code, opts)
                    Else
                        Return SyntaxImplements.DeclareNewVariable(code, opts)
                    End If
                Case "if" : Return SyntaxImplements.IfBranch(code.Skip(1).IteratesALL, opts)
                Case "else" : Return code.Skip(1).IteratesALL.ToArray.DoCall(Function(tokens) SyntaxImplements.ElseBranch(tokens, opts))
                Case "elseif" : Return code.Skip(1).IteratesALL.DoCall(Function(tokens) SyntaxImplements.ElseIfBranch(tokens, opts))
                Case "return" : Return SyntaxImplements.ReturnValue(code.Skip(1).IteratesALL, opts)
                Case "for" : Return SyntaxImplements.ForLoop(code.Skip(1).IteratesALL, opts)
                Case "from" : Return SyntaxImplements.LinqExpression(code, opts)
                Case "imports" : Return SyntaxImplements.[Imports](code, opts)
                Case "function"
                    Dim [let] = New Token() {New Token(TokenType.keyword, "let")}
                    Dim name = New Token() {New Token(TokenType.identifier, $"<${App.GetNextUniqueName("anonymous_")}>") With {.span = code(Scan0)(Scan0).span}}
                    Dim [as] = New Token() {New Token(TokenType.keyword, "as")}

                    code = ({[let], name, [as]}) + code

                    Return SyntaxImplements.DeclareNewFunction(code, opts)
                Case "suppress"
                    Dim evaluate As SyntaxResult = code _
                        .Skip(1) _
                        .IteratesALL _
                        .DoCall(Function(tokens)
                                    Return Expression.CreateExpression(tokens, opts)
                                End Function)

                    If evaluate.isException Then
                        Return evaluate
                    Else
                        Return New Suppress(evaluate.expression)
                    End If
                Case "modeof", "typeof", "valueof"
                    Return SyntaxImplements.ModeOf(keyword, code(1), opts)
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
                Case "using"
                    Return SyntaxImplements.UsingClosure(code.Skip(1), opts)
                Case Else
                    ' may be it is using keyword as identifier name
                    Return Nothing
            End Select
        End Function

        <Extension>
        Friend Function ParseExpression(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            If code(Scan0).isKeyword Then
                Dim expression As SyntaxResult = code.keywordExpressionHandler(opts)

                ' if expression is nothing
                ' then it means the keyword is probably 
                ' using keyword as identifier name
                If Not expression Is Nothing Then
                    Return expression
                Else
                    code(Scan0)(Scan0).name = TokenType.identifier
                End If
            End If

            If code = 1 Then
                Dim item As Token() = code(Scan0)

                If item.isLiteral Then
                    Return SyntaxImplements.LiteralSyntax(item(Scan0), opts)
                ElseIf item.isIdentifier Then
                    Return New SymbolReference(item(Scan0))
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
                    End If
                End If
            ElseIf code(1).isOperator("=", "<-") Then
                Return getValueAssign(code, opts)
            ElseIf code = 2 Then
                If code(Scan0).Length = 1 AndAlso code(Scan0)(Scan0) = (TokenType.operator, "$") Then
                    Return SyntaxImplements.FunctionInvoke(code.IteratesALL.ToArray, opts)
                ElseIf code(Scan0).Length = 1 AndAlso code(Scan0)(Scan0) = (TokenType.operator, "!") Then
                    ' not xxxx
                    Dim valExpression As SyntaxResult = Expression.CreateExpression(code(1), opts)

                    If valExpression.isException Then
                        Return valExpression
                    Else
                        Return New UnaryNot(valExpression.expression)
                    End If
                End If
            ElseIf code = 3 Then
                If code.isSequenceSyntax Then
                    Dim seq = code(Scan0).SplitByTopLevelDelimiter(TokenType.sequence)
                    Dim from = seq(Scan0)
                    Dim [to] = seq(2)
                    Dim steps As Token() = Nothing

                    If code > 1 Then
                        If code(1).isKeyword("step") Then
                            steps = code(2)
                        ElseIf code(1).isOperator Then
                            ' do nothing
                            GoTo Binary
                        Else
                            Return New SyntaxResult(New SyntaxErrorException, opts.debug)
                        End If
                    End If

                    Return SyntaxImplements.SequenceLiteral(from, [to], steps, opts)
                End If
            End If
Binary:
            Return code.ParseBinaryExpression(opts)
        End Function

        Private Function getValueAssign(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            ' tuple value assign
            ' or member reference assign
            Dim target As Token() = code(Scan0)
            Dim value As Token() = code(2)
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
                                Return Expression.CreateExpression(tokens, opts)
                            End Function)

                If vals.isException Then
                    Return vals
                Else
                    Dim calls As SyntaxResult = SyntaxImplements.FunctionInvoke(target, opts)

                    If calls.isException Then
                        Return calls
                    Else
                        Return New ByRefFunctionCall(calls.expression, vals.expression)
                    End If
                End If
            Else
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
            End If

            Dim valExpression As SyntaxResult = Expression.CreateExpression(value, opts)

            If valExpression.isException Then
                Return valExpression
            Else
                Return New ValueAssign(symbol, valExpression.expression)
            End If
        End Function

        Private Iterator Function getTupleSymbols(target As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As IEnumerable(Of SyntaxResult)
            For Each token As SyntaxResult In target.SplitByTopLevelDelimiter(TokenType.comma) _
                                                    .Where(Function(t) Not t.isComma) _
                                                    .Select(Function(tokens)
                                                                Return Expression.CreateExpression(tokens, opts)
                                                            End Function)
                Yield token

                If token.isException Then
                    Exit For
                End If
            Next
        End Function
    End Module
End Namespace

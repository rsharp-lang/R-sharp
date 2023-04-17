#Region "Microsoft.VisualBasic::a7f5b4aa6be26c4bf2f79cca5f444db7, D:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxTree/ExpressionTree.vb"

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


    ' Code Statistics:

    '   Total Lines: 262
    '    Code Lines: 208
    ' Comment Lines: 17
    '   Blank Lines: 37
    '     File Size: 12.19 KB


    '     Module ExpressionTree
    ' 
    '         Function: CreateTree, isDotNetMemberReference, ObjectInvoke, ParseDotNetmember, ParseExpressionTree
    '                   simpleSequence
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language.Syntax.SyntaxParser

    Module ExpressionTree

        <Extension>
        Public Function CreateTree(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim err As Exception = Nothing
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.comma, err:=err)

            Call opts.SetCurrentRange(tokens)

            If Not err Is Nothing Then
                Return SyntaxResult.CreateError(err, opts)
            End If

            If blocks = 1 Then
                Dim expression As SyntaxResult = blocks(Scan0).simpleSequence(opts)

                If Not expression Is Nothing Then
                    Return expression
                ElseIf blocks = 1 AndAlso blocks(Scan0)(Scan0) = (TokenType.operator, "~") Then
                    Return FormulaExpressionSyntax.GetExpressionLiteral(blocks, opts)
                Else
                    ' 是一个复杂的表达式
                    Return blocks(Scan0).ParseExpressionTree(opts)
                End If
            ElseIf opts.isBuildVector Then
                Dim expressions As New List(Of Expression)
                Dim temp As SyntaxResult

                For i As Integer = 0 To blocks.Count - 1 Step 2
                    temp = blocks(i).ParseExpressionTree(opts)

                    If temp.isException Then
                        Return temp
                    Else
                        expressions.Add(temp.expression)
                    End If
                Next

                Return New VectorLiteral(expressions.ToArray, TypeCodes.generic)
            Else
                Return SyntaxResult.CreateError(New NotImplementedException, opts)
            End If
        End Function

        <Extension>
        Private Function simpleSequence(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.sequence, includeKeyword:=True)

            If blocks = 3 Then
                Return SyntaxImplements.SequenceLiteral(blocks(Scan0), blocks(2), Nothing, opts)
            ElseIf blocks = 5 Then
                Return SyntaxImplements.SequenceLiteral(blocks(Scan0), blocks(2), blocks.ElementAtOrDefault(4), opts)
            Else
                Return Nothing
            End If
        End Function

        <Extension>
        Public Function isDotNetMemberReference(code As List(Of Token())) As Boolean
            If code <> 3 Then
                Return False
            ElseIf code(0).First <> (TokenType.open, "[") OrElse code(0).Last <> (TokenType.close, "]") Then
                Return False
            ElseIf code(1).Length <> 1 OrElse code(1)(Scan0) <> (TokenType.operator, "::") Then
                Return False
            ElseIf code(2).Length <> 1 OrElse Not (code(2)(0).name = TokenType.identifier OrElse code(2)(0).name = TokenType.keyword) Then
                Return False
            Else
                Return True
            End If
        End Function

        <Extension>
        Public Function ParseDotNetmember(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim obj = code(0).Skip(1).Take(code(0).Length - 2).DoCall(Function(i) opts.ParseExpression(i, opts))
            Dim t = code(2)

            If t.Length = 1 AndAlso t(Scan0).name = TokenType.keyword Then
                t(Scan0).name = TokenType.identifier
            End If

            Dim target As SyntaxResult = opts.ParseExpression(t, opts)

            If obj.isException Then
                Return obj
            ElseIf target.isException Then
                Return target
            Else
                Return New SyntaxResult(New DotNetObject(obj.expression, target.expression))
            End If
        End Function

        <Extension>
        Private Function ParseExpressionTree(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim blocks As List(Of Token())

            If tokens.Length = 1 Then
                If tokens(Scan0).name = TokenType.stringInterpolation Then
                    Return SyntaxImplements.StringInterpolation(tokens(Scan0), opts)
                ElseIf tokens(Scan0).name = TokenType.cliShellInvoke Then
                    Return SyntaxImplements.CommandLine(tokens(Scan0), opts)
                ElseIf tokens(Scan0) = (TokenType.operator, "$") Then
                    Return New SymbolReference("$")
                ElseIf tokens(Scan0).name = TokenType.regexp Then
                    Return New Regexp(tokens(Scan0).text)
                ElseIf tokens(Scan0).name = TokenType.stringLiteral Then
                    Return New Literal(tokens(Scan0).text)
                ElseIf tokens(Scan0).name = TokenType.numberLiteral Then
                    Return New Literal(Val(tokens(Scan0).text))
                ElseIf tokens(Scan0).name = TokenType.integerLiteral Then
                    Return New Literal(CLng(tokens(Scan0).text))
                Else
                    blocks = New List(Of Token()) From {tokens}
                End If
            ElseIf tokens.Length = 2 AndAlso tokens(Scan0).name = TokenType.iif Then
                Return SyntaxImplements.CommandLineArgument(tokens, opts)
            Else
                blocks = tokens.SplitByTopLevelDelimiter(TokenType.operator)
                opts = opts.SetCurrentRange(tokens)
            End If

            If blocks = 3 AndAlso blocks.isDotNetMemberReference Then
                Return blocks.ParseDotNetmember(opts)
            End If

            If blocks > 1 Then
                Return ParseBinaryExpression(blocks, opts)
            End If

            ' 在下面解析简单的表达式
            If tokens.isFunctionInvoke Then
                If tokens(Scan0) = (TokenType.keyword, {"require", "return", "if", "for", "function"}) Then
                    Dim keyword As Token() = {tokens(Scan0)}
                    Dim content As Token() = tokens.Skip(1).ToArray
                    Dim join = New List(Of Token()) From {keyword, content}

                    Return join.keywordExpressionHandler(opts)
                Else
                    Return SyntaxImplements.FunctionInvoke(tokens, opts)
                End If
            ElseIf tokens.isSimpleSymbolIndexer Then
                Return SyntaxImplements.SymbolIndexer(tokens, opts)
            ElseIf tokens.isAcceptor Then
                Return FunctionAcceptorSyntax.FunctionAcceptorInvoke(tokens, opts)
            ElseIf tokens(Scan0).name = TokenType.open Then
                Dim openSymbol = tokens(Scan0).text

                If openSymbol = "[" Then
                    Return SyntaxImplements.VectorLiteral(tokens, opts)
                ElseIf openSymbol = "(" Then
                    ' (xxxx)
                    ' (xxxx)[xx]
                    ' (function(){...})(...)
                    Dim splitTokens = tokens.SplitByTopLevelDelimiter(TokenType.close)

                    If splitTokens = 2 Then
                        '  0     1
                        ' (xxxx |) is a single expression
                        ' 是一个表达式
                        Return splitTokens(Scan0) _
                            .Skip(1) _
                            .SplitByTopLevelDelimiter(TokenType.operator) _
                            .ParseBinaryExpression(opts)
                    ElseIf splitTokens = 4 Then
                        If splitTokens.Last.Length = 1 AndAlso splitTokens.Last()(Scan0) = (TokenType.close, "]") Then
                            ' 0      1    2     3
                            ' (xxxx |) | [xxx | ]
                            ' symbol indexer
                            Dim symbolExpression As Token() = splitTokens(Scan0).Skip(1).ToArray
                            Dim indexerExpression As Token() = splitTokens(2) _
                                .JoinIterates(splitTokens.Last) _
                                .ToArray

                            Return SyntaxImplements.SymbolIndexer(symbolExpression, indexerExpression, opts)
                        Else
                            ' 可能是一个匿名函数的调用
                            ' (function(...){})(...)
                            If splitTokens(1).Length = 1 AndAlso splitTokens(1)(Scan0).text = ")" AndAlso
                               splitTokens(2)(Scan0).text = "(" AndAlso
                               splitTokens.Last.Length = 1 AndAlso splitTokens.Last(Scan0).text = ")" Then

                                Return splitTokens.ObjectInvoke(opts)
                            Else
                                Return SyntaxResult.CreateError(New SyntaxErrorException, opts)
                            End If
                        End If
                    Else
                        Return SyntaxResult.CreateError(New NotImplementedException, opts)
                    End If
                ElseIf openSymbol = "{" Then
                    ' 是一个可以产生值的closure
                    Return SyntaxImplements.ClosureExpression(tokens, opts)
                End If
            ElseIf tokens(Scan0).name = TokenType.stringInterpolation Then
                Return SyntaxImplements.StringInterpolation(tokens(Scan0), opts)
            Else
                Dim indexer As SyntaxResult = tokens.parseComplexSymbolIndexer(opts)

                If Not indexer Is Nothing Then
                    Return indexer
                End If
            End If

            If tokens(Scan0) = (TokenType.keyword, "function") Then
                Dim code = tokens.SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True)

                If code.isAnonymous Then
                    ' function(xxx) {
                    '   ...
                    ' }

                    Return SyntaxImplements.DeclareAnonymousFunction(code, opts)
                End If
            End If

            If opts.keepsCommentLines AndAlso tokens.All(Function(b) b.name = TokenType.comment) Then
                Return New SyntaxResult(New CodeComment(tokens.Select(Function(t) t.Trim("#"c, " "c)).JoinBy(vbCrLf)))
            Else
                opts = opts.SetCurrentRange(tokens)
            End If

            Dim msg As String = $"Unsure for parse: '{tokens.Select(Function(t) t.text).JoinBy(" ")}'!"

            Return SyntaxResult.CreateError(New NotImplementedException(msg), opts)
        End Function

        <Extension>
        Private Function ObjectInvoke(splitTokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim invokeTarget = splitTokens(Scan0).Skip(1).ToArray
            Dim invoke = splitTokens.Skip(2).IteratesALL.ToArray

            If splitTokens(Scan0)(1) = (TokenType.keyword, "function") Then
                Return SyntaxImplements.AnonymousFunctionInvoke(
                    anonymous:=invokeTarget,
                    invoke:=invoke,
                    opts:=opts
                )
            Else
                Dim target As SyntaxResult = opts.ParseExpression(invokeTarget, opts)

                If target.isException Then
                    Return target
                Else
                    Return target.AnonymousFunctionInvoke(invoke, invokeTarget(Scan0).span.line, opts)
                End If
            End If
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::2d158c378e62036cdd29423ebeafe799, R#\Interpreter\ExecuteEngine\Linq\Syntax\SyntaxImplements.vb"

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

    '     Module SyntaxImplements
    ' 
    '         Function: CreateAggregateQuery, CreateProjectionQuery, GetParameters, GetParameterTokens, GetProjection
    '                   GetSequence, GetVector, IsClosure, IsNumeric, JoinOperators
    '                   ParseExpression, ParseKeywordExpression, ParseToken, ParseValueAssign, PopulateExpressions
    '                   PopulateQueryExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports any = Microsoft.VisualBasic.Scripting
Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

Namespace Interpreter.ExecuteEngine.LINQ.Syntax

    Public Module SyntaxImplements

        ''' <summary>
        ''' target token is a number literal value? 
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        <Extension>
        Private Function IsNumeric(t As Token) As Boolean
            Return t.name = TokenType.integerLiteral OrElse t.name = TokenType.numberLiteral
        End Function

        <Extension>
        Private Function JoinOperators(tokenList As IEnumerable(Of Token)) As IEnumerable(Of Token)
            Dim list As New List(Of Token)(tokenList)
            Dim t As Token

            For i As Integer = 0 To list.Count - 1
                If i >= list.Count Then
                    Exit For
                Else
                    t = list(i)
                End If

                If t = (TokenType.operator, "-") AndAlso list(i + 1).isNumeric Then
                    t = New Token(list(i + 1).name, -Val(list(i + 1).text))
                    list.RemoveAt(i + 1)
                    list.RemoveAt(i)
                    list.Insert(i, t)
                ElseIf t = (TokenType.operator, ">") OrElse t = (TokenType.operator, "<") Then
                    If list(i + 1) = (TokenType.operator, "=") OrElse list(i + 1) = (TokenType.operator, ">") Then
                        t = New Token(TokenType.operator, t.text & list(i + 1).text)
                        list.RemoveAt(i + 1)
                        list.RemoveAt(i)
                        list.Insert(i, t)
                    End If
                End If
            Next

            Return list
        End Function

        ReadOnly sortOrders As Index(Of String) = {"descending", "ascending"}

        ''' <summary>
        ''' the main entry of parse linq expression
        ''' </summary>
        ''' <param name="tokenList"></param>
        ''' <returns>
        ''' <see cref="QueryExpression"/> or an error <see cref="Exception"/>
        ''' </returns>
        <Extension>
        Friend Function PopulateQueryExpression(tokenList As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim blocks As List(Of Token()) = tokenList _
                .JoinOperators _
                .SplitByTopLevelStack _
                .AsList

            For i As Integer = 1 To blocks.Count - 1
                If i >= blocks.Count Then
                    Exit For
                End If

                If blocks(i).Length = 1 AndAlso blocks(i)(Scan0).name = TokenType.keyword Then
                    If blocks(i)(Scan0).text.ToLower Like sortOrders Then
                        blocks(i - 1) = blocks(i - 1) _
                            .JoinIterates(blocks(i)) _
                            .ToArray
                        blocks.RemoveAt(i)
                    End If
                End If
            Next

            If blocks(Scan0).First.isKeywordFrom Then
                Return blocks(Scan0).CreateProjectionQuery(blocks.Skip(1).ToArray, opts)
            ElseIf blocks(Scan0).First.isKeywordAggregate Then
                Return blocks(Scan0).CreateAggregateQuery(blocks.Skip(1).ToArray, opts)
            Else
                Return New SyntaxParserResult(SyntaxError.CreateError(opts.SetCurrentRange(blocks(Scan0)), New SyntaxErrorException(blocks(Scan0).First.ToString)))
            End If
        End Function

        ''' <summary>
        ''' <see cref="ProjectionExpression"/>
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="blocks"></param>
        ''' <returns></returns>
        <Extension>
        Private Function CreateProjectionQuery(symbol As Token(), blocks As Token()(), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim symbolExpr As SyntaxParserResult = symbol.ParseExpression(opts)

            If symbolExpr.isError Then
                Return symbolExpr
            End If

            Dim i As Integer = 0
            Dim seq As SyntaxParserResult = blocks.GetSequence(offset:=i, opts:=opts)
            Dim exec As New List(Of Expression)
            Dim join As DataLeftJoin = Nothing

            If seq.isError Then
                Return seq
            Else
                blocks = blocks.Skip(i).ToArray
            End If

            If blocks(Scan0)(Scan0) = (TokenType.keyword, "join") Then
                Dim joinSymbol As SyntaxParserResult = blocks(Scan0).ParseExpression(opts)
                Dim joinSeq As SyntaxParserResult = blocks.Skip(1).ToArray.GetSequence(offset:=i, opts:=opts)

                If joinSymbol.isError Then
                    Return joinSymbol
                ElseIf joinSeq.isError Then
                    Return joinSeq
                Else
                    blocks = blocks.Skip(i + 1).ToArray
                End If

                If blocks(Scan0)(Scan0) <> (TokenType.keyword, "on") Then
                    Return New SyntaxParserResult(SyntaxError.CreateError(opts.SetCurrentRange(blocks(Scan0)), New SyntaxErrorException("missing 'on' equaliant expression!")))
                End If

                Dim binary As SyntaxParserResult = blocks(Scan0).ParseExpression(opts)

                If binary.isError Then
                    Return binary
                Else
                    join = New DataLeftJoin(joinSymbol.expression, joinSeq.expression).SetKeyBinary(binary.expression)
                End If

                blocks = blocks.Skip(1).ToArray
            End If

            For Each line As SyntaxParserResult In blocks.PopulateExpressions(opts)
                If line.isError Then
                    Return line
                Else
                    exec.Add(line.expression)
                End If
            Next

            Dim proj As Expression = exec _
                .Where(Function(t) TypeOf t Is OutputProjection) _
                .FirstOrDefault
            Dim opt As New Options(exec.Where(Function(t) TypeOf t Is PipelineKeyword))
            Dim execProgram As Expression() = exec _
                .Where(Function(t)
                           Return (Not TypeOf t Is PipelineKeyword) AndAlso (Not TypeOf t Is OutputProjection)
                       End Function) _
                .ToArray
            Dim LINQ As New ProjectionExpression(
                symbol:=symbolExpr.expression,
                sequence:=seq.expression,
                exec:=execProgram,
                proj:=proj,
                opt:=opt
            )

            If Not join Is Nothing Then
                Call LINQ.joins.Add(join)
            End If

            Call LINQ.FixProjection()

            Return LINQ
        End Function

        ''' <summary>
        ''' <see cref="AggregateExpression"/>
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="blocks"></param>
        ''' <returns></returns>
        <Extension>
        Private Function CreateAggregateQuery(symbol As Token(), blocks As Token()(), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim symbolExpr As SyntaxParserResult = symbol.ParseExpression(opts)

            If symbolExpr.isError Then
                Return symbolExpr
            End If

            Return New SyntaxParserResult(SyntaxError.CreateError(opts.SetCurrentRange(symbol), New NotImplementedException))
        End Function

        <Extension>
        Private Iterator Function PopulateExpressions(blocks As IEnumerable(Of Token()), opts As SyntaxBuilderOptions) As IEnumerable(Of SyntaxParserResult)
            For Each blockLine As Token() In blocks
                Yield ParseExpression(blockLine, opts)
            Next
        End Function

        <Extension>
        Private Function GetSequence(blocks As Token()(), ByRef offset As Integer, opts As SyntaxBuilderOptions) As SyntaxParserResult
            If Not blocks(Scan0).First.isKeyword("in") Then
                Return New SyntaxParserResult(SyntaxError.CreateError(opts.SetCurrentRange(blocks(Scan0)), New SyntaxErrorException(blocks(Scan0).First.ToString)))
            ElseIf blocks(Scan0).Length = 1 Then
                offset = 2
                Return blocks(1).ParseExpression(opts)
            Else
                offset = 1
                Return blocks(0).ParseExpression(opts)
            End If
        End Function

        <Extension>
        Friend Function ParseToken(t As Token, opts As SyntaxBuilderOptions) As SyntaxParserResult
            If t.name = TokenType.identifier Then
                Return New SymbolReference(t.text)
            ElseIf t.name = TokenType.booleanLiteral OrElse
                t.name = TokenType.integerLiteral OrElse
                t.name = TokenType.numberLiteral OrElse
                t.name = TokenType.stringLiteral Then

                Return New Literal(t)
            Else
                Return New SyntaxParserResult(SyntaxError.CreateError(opts.SetCurrentRange({t}), New NotImplementedException))
            End If
        End Function

        <Extension>
        Private Function GetProjection(tokenList As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim fields As New List(Of NamedValue(Of Expression))

            For Each item As SyntaxParserResult In tokenList.GetParameters(opts)
                If item.isError Then
                    Return item
                ElseIf TypeOf item.expression Is BinaryExpression Then
                    With DirectCast(item.expression, BinaryExpression)
                        If .LikeValueAssign Then
                            fields.Add(New NamedValue(Of Expression)(DirectCast(.left, SymbolReference).symbolName, .right))
                        Else
                            fields.Add(New NamedValue(Of Expression)(item.ToString, item.expression))
                        End If
                    End With
                ElseIf TypeOf item.expression Is MemberReference Then
                    With DirectCast(item.expression, MemberReference)
                        fields.Add(New NamedValue(Of Expression)(.memberName, item.expression))
                    End With
                ElseIf TypeOf item.expression Is SymbolReference Then
                    fields.Add(New NamedValue(Of Expression)(DirectCast(item.expression, SymbolReference).symbolName, item.expression))
                ElseIf TypeOf item.expression Is Literal Then
                    fields.Add(New NamedValue(Of Expression)(any.ToString(DirectCast(item.expression, Literal).value), item.expression))
                ElseIf TypeOf item.expression Is ValueAssign Then
                    With DirectCast(item.expression, ValueAssign)
                        fields.Add(New NamedValue(Of Expression)(.symbolName, New RunTimeValueExpression(.value)))
                    End With
                ElseIf TypeOf item.expression Is RunTimeValueExpression Then
                    With DirectCast(item.expression, RunTimeValueExpression)
                        fields.Add(New NamedValue(Of Expression)(.GetProjectionName, item.expression))
                    End With
                Else
                    Return New SyntaxParserResult(SyntaxError.CreateError(opts, New SyntaxErrorException($"invalid expression type({item.expression.GetType.FullName})!")))
                End If
            Next

            Return New OutputProjection(fields)
        End Function

        Private Function ParseSymbolDeclare(tokenList As Token(), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim type As String = "any"
            Dim name As Expression

            ' declare new symbol
            ' may be a tuple
            tokenList = tokenList.Skip(1).ToArray

            If tokenList.Length = 1 Then
                ' just a symbol
                name = New Literal(tokenList(Scan0))
            ElseIf tokenList.Length = 3 AndAlso tokenList(1) = (TokenType.keyword, "as") Then
                ' a symbol with type constraint
                name = New Literal(tokenList(Scan0))
                type = tokenList(2).text
            Else
                ' a tuple?
                Dim expVal As SyntaxParserResult = tokenList.ParseExpression(opts)

                If expVal.isError Then
                    Return expVal
                Else
                    name = expVal.expression
                End If
            End If

            Return New SymbolDeclare With {.symbol = name, .typeName = type}
        End Function

        <Extension>
        Private Function ParseKeywordExpression(tokenList As Token(), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim token0 As Token = tokenList(Scan0)

            If token0.isKeywordFrom OrElse
               token0.isKeywordAggregate OrElse
               token0.isKeywordJoin OrElse
               token0.isKeyword("let") Then

                Return ParseSymbolDeclare(tokenList, opts)

            ElseIf token0.isKeyword("where") Then
                Dim bool As SyntaxResult = RExpression.CreateExpression(tokenList.Skip(1), opts)

                If bool.isException Then
                    Return New SyntaxParserResult(bool.error)
                End If

                Return New WhereFilter(New RunTimeValueExpression(bool.expression))
            ElseIf token0.isKeyword("in") Then
                Return New SyntaxParserResult(RExpression.CreateExpression(tokenList.Skip(1), opts))
            ElseIf token0.isKeyword("on") Then
                Dim bin = tokenList.Skip(1).ToArray.ParseBinary(opts)

                If bin.isError Then
                    Return bin
                End If

                Dim binExpr As BinaryExpression = DirectCast(bin.expression, BinaryExpression)

                If Not binExpr.isEquivalent Then
                    Return New SyntaxParserResult(SyntaxError.CreateError(opts, New InvalidExpressionException("operator should be equals")))
                Else
                    Return bin
                End If
            ElseIf token0.isKeyword("select") Then
                Return tokenList.Skip(1).GetProjection(opts)
            ElseIf token0.isKeyword("order") Then
                Dim sortKey = tokenList.Skip(2).ToArray
                Dim desc As Boolean

                If sortKey.Last.name = TokenType.keyword AndAlso sortKey.Last.text.ToLower Like sortOrders Then
                    desc = sortKey.Last.text.TextEquals("descending")
                    sortKey = sortKey.Take(sortKey.Length - 1).ToArray
                End If

                Dim key = ParseExpression(sortKey, opts)

                If key.isError Then
                    Return key
                End If

                Return New OrderBy(key.expression, desc)
            ElseIf token0.isKeyword("take") Then
                Dim n = ParseExpression(tokenList.Skip(1).ToArray, opts)

                If n.isError Then
                    Return n
                End If

                Return New TakeItems(n.expression)
            ElseIf token0.isKeyword("skip") Then
                Dim n = ParseExpression(tokenList.Skip(1).ToArray, opts)

                If n.isError Then
                    Return n
                End If

                Return New SkipItems(n.expression)
            Else
                Throw New SyntaxErrorException
            End If
        End Function

        ''' <summary>
        ''' (...) or {...}
        ''' </summary>
        ''' <param name="tokenList"></param>
        ''' <returns></returns>
        <Extension>
        Private Function IsClosure(tokenList As Token()) As Boolean
            Return tokenList(Scan0).name = TokenType.open AndAlso tokenList.Last.name = TokenType.close
        End Function

        Private Function ParseValueAssign(symbol As Token, value As Token(), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim val As SyntaxResult = New List(Of Token())(value).ParseExpression(opts)

            If val.isException Then
                Return New SyntaxParserResult(val.error)
            End If

            Return New ValueAssign(symbol.text, val.expression)
        End Function

        <Extension>
        Friend Function ParseExpression(tokenList As Token(), opts As SyntaxBuilderOptions) As SyntaxParserResult
            If tokenList.Length = 1 Then
                Return tokenList(Scan0).ParseToken(opts)
            ElseIf tokenList(Scan0).name = TokenType.keyword Then
                Return tokenList.ParseKeywordExpression(opts)
            ElseIf tokenList.Length > 2 AndAlso tokenList(1) = (TokenType.operator, "=") Then
                ' is value assign
                Return ParseValueAssign(tokenList(Scan0), tokenList.Skip(2).ToArray, opts)
            End If

            Dim blocks = tokenList.SplitByTopLevelStack.ToArray

            If blocks.Length = 1 Then
                tokenList = blocks(Scan0)

                If tokenList.First = (TokenType.open, "[") OrElse tokenList.First = (TokenType.open, "{") Then
                    Return tokenList.Skip(1).Take(tokenList.Length - 2).GetVector(opts)
                ElseIf tokenList.First = (TokenType.open, "(") Then
                    tokenList = tokenList _
                        .Skip(1) _
                        .Take(tokenList.Length - 2) _
                        .ToArray
                End If
            ElseIf blocks.Length = 2 Then
                Dim name As SyntaxParserResult = ParseExpression(blocks(Scan0), opts)

                If name.isError Then
                    Return name
                End If

                If TypeOf name.expression Is SymbolReference AndAlso blocks(1).IsClosure Then
                    Dim params As New List(Of RExpression)
                    Dim funcName As String = DirectCast(name.expression, SymbolReference).symbolName
                    Dim sourceMap As StackFrame = opts.GetStackTrace(blocks(Scan0)(Scan0), funcName)

                    For Each expr As SyntaxResult In blocks(1) _
                        .Skip(1) _
                        .Take(blocks(1).Length - 2) _
                        .GetParameterTokens _
                        .Select(Function(t) RExpression.CreateExpression(t, opts))

                        If expr.isException Then
                            Return New SyntaxParserResult(expr)
                        Else
                            params.Add(expr.expression)
                        End If
                    Next

                    Return New RunTimeValueExpression(New FunctionInvoke(funcName, sourceMap, params.ToArray))
                End If
            End If

            If tokenList.Length = 2 Then
                If tokenList(Scan0) <> (TokenType.operator, "-") Then
                    Return New SyntaxParserResult(SyntaxError.CreateError(opts.SetCurrentRange(tokenList), New SyntaxErrorException))
                End If
            ElseIf tokenList.Length = 3 AndAlso tokenList(1) = (TokenType.keyword, "as") Then
                Dim aliasName As New AliasName(
                    old:=tokenList(Scan0).text,
                    [alias]:=tokenList(2).text
                )

                Return New SyntaxParserResult(aliasName)
            End If

            Return tokenList.ParseBinary(opts)
        End Function

        <Extension>
        Private Function GetParameterTokens(tokenList As IEnumerable(Of Token)) As IEnumerable(Of Token())
            Return tokenList _
                .SplitParameters _
                .Select(Function(b)
                            If b(Scan0).name = TokenType.comma Then
                                Return b.Skip(1).ToArray
                            Else
                                Return b
                            End If
                        End Function) _
                .ToArray
        End Function

        <Extension>
        Private Iterator Function GetParameters(tokenList As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As IEnumerable(Of SyntaxParserResult)
            For Each block As Token() In tokenList.GetParameterTokens
                Yield ParseExpression(block, opts)
            Next
        End Function

        <Extension>
        Private Function GetVector(tokenList As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxParserResult
            Dim elements As New List(Of Expression)

            For Each item In tokenList.GetParameters(opts)
                If item.isError Then
                    Return item
                Else
                    elements.Add(item.expression)
                End If
            Next

            Return New VectorLiteral With {
                .elements = elements.ToArray
            }
        End Function
    End Module
End Namespace

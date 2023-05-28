#Region "Microsoft.VisualBasic::b0dd5f08703e8cb004e1b9466258c132, F:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxTree/BinaryExpressionTree/BinaryExpressionTree.vb"

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

    '   Total Lines: 474
    '    Code Lines: 354
    ' Comment Lines: 49
    '   Blank Lines: 71
    '     File Size: 21.38 KB


    '     Module BinaryExpressionTree
    ' 
    '         Function: CreateBinary, CreateBinaryExpression, joinNegatives, joinRemaining, MeasureCurrentLine
    '                   (+2 Overloads) ParseBinaryExpression, processOperators
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser

    Module BinaryExpressionTree

        ''' <summary>
        ''' the math operators
        ''' </summary>
        Friend ReadOnly operatorPriority As String() = {"^", "*/%", "+-"}

        ReadOnly comparisonOperators As String() = {"<", ">", "<=", ">=", "==", "!=", "in", "like", "between"}
        ReadOnly logicalOperators As String() = {"&&", "||", "!"}

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="tokenBlocks"></param>
        ''' <param name="buf"></param>
        ''' <param name="oplist"></param>
        ''' <param name="opts"></param>
        ''' <returns>
        ''' returns error message
        ''' </returns>
        <Extension>
        Private Function joinNegatives(tokenBlocks As List(Of Token()),
                                       ByRef buf As List(Of [Variant](Of SyntaxResult, String)),
                                       ByRef oplist As List(Of String),
                                       opts As SyntaxBuilderOptions) As SyntaxResult

            Dim syntaxResult As SyntaxResult
            Dim index As i32 = Scan0

            If tokenBlocks(Scan0).Length = 1 AndAlso tokenBlocks(Scan0)(Scan0) = (TokenType.operator, {"-", "+"}) Then
                If tokenBlocks.Count > 1 AndAlso tokenBlocks(1).Length = 1 AndAlso tokenBlocks(1)(Scan0).isNumeric Then
                    Dim unary As String = tokenBlocks(Scan0)(Scan0).text
                    Dim number As Double = tokenBlocks(1)(Scan0).text.ParseDouble

                    If unary = "-" Then
                        tokenBlocks(1)(Scan0).text = -number
                    End If

                    tokenBlocks.RemoveAt(Scan0)
                ElseIf tokenBlocks = 2 Then
                    ' UnaryNumeric
                    Dim unaryOp = tokenBlocks(Scan0)(Scan0).text
                    Dim operon = opts.ParseExpression(tokenBlocks(1), opts)

                    If operon.isException Then
                        Return operon
                    Else
                        Return New UnaryNumeric(unaryOp, operon.expression)
                    End If
                Else
                    ' insert a ZERO before
                    tokenBlocks.Insert(Scan0, {New Token With {.name = TokenType.numberLiteral, .text = 0}})
                End If
            End If

            For i As Integer = Scan0 To tokenBlocks.Count - 1
                If ++index Mod 2 = 0 Then
                    If tokenBlocks(i).isOperator("+", "-") Then
                        syntaxResult = opts.ParseExpression(tokenBlocks(i + 1), opts)

                        If syntaxResult.isException Then
                            Return syntaxResult
                        Else
                            ' +/-
                            syntaxResult = New BinaryExpression(
                                left:=New Literal(0),
                                right:=syntaxResult.expression,
                                op:=tokenBlocks(i)(Scan0).text
                            )
                            i += 1

                            Call buf.Add(syntaxResult)
                        End If
                    ElseIf tokenBlocks(i).isOperator("!") Then
                        ' not ...
                        Dim pull As New List(Of Token)
                        Dim delta As Integer = 0

                        For j As Integer = i + 1 To tokenBlocks.Count - 1
                            If tokenBlocks(j).isOperator("$", "::") Then
                                pull.AddRange(tokenBlocks(j))
                            ElseIf Not tokenBlocks(j).isOperator Then
                                pull.AddRange(tokenBlocks(j))
                            Else
                                Exit For
                            End If

                            delta += 1
                        Next

                        syntaxResult = opts.ParseExpression(pull, opts)

                        If syntaxResult.isException Then
                            Return syntaxResult
                        Else
                            syntaxResult = New UnaryNot(syntaxResult.expression)
                            i += delta

                            Call buf.Add(syntaxResult)
                        End If
                    Else
                        Dim t = tokenBlocks(i)

                        If t.Length = 1 AndAlso t(Scan0).name = TokenType.keyword Then
                            t(Scan0).name = TokenType.identifier
                        End If

                        syntaxResult = opts.ParseExpression(t, opts)

                        If syntaxResult.isException Then
                            Return syntaxResult
                        Else
                            Call buf.Add(syntaxResult)
                        End If
                    End If
                ElseIf Not tokenBlocks(i).isOperator Then
                    Dim list = tokenBlocks(i)

                    If list.First = (TokenType.open, "(") AndAlso list.Last = (TokenType.close, ")") Then
                        If buf.Last.TryCast(Of SyntaxResult) Like GetType(SymbolReference) Then
                            Dim params As New List(Of Expression)
                            Dim trace = opts.GetStackTrace(list.First, buf.Last.ToString)
                            Dim temp As SyntaxResult

                            For Each par As Token() In list _
                                .Skip(1) _
                                .Take(list.Length - 2) _
                                .SplitByTopLevelDelimiter(TokenType.comma, includeKeyword:=True)

                                If par.isComma Then
                                    Continue For
                                End If

                                temp = opts.ParseExpression(par, opts)

                                If temp.isException Then
                                    Return temp
                                Else
                                    params.Add(temp.expression)
                                End If
                            Next

                            Call buf.Add(New SyntaxResult(New FunctionInvoke(buf.Pop.TryCast(Of SyntaxResult).expression, trace, params.ToArray)))
                        End If
                    Else
                        Return New SyntaxResult(SyntaxError.CreateError(opts, New SyntaxErrorException()))
                    End If
                Else
                    Call buf.Add(tokenBlocks(i)(Scan0).text)
                    Call oplist.Add(buf.Last.VB)
                End If
            Next

            Return Nothing
        End Function

        <Extension>
        Public Function ParseBinaryExpression(tokenBlocks As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim buf As New List(Of [Variant](Of SyntaxResult, String))
            Dim syntaxResult As New Value(Of SyntaxResult)
            Dim oplist As New List(Of String)
            Dim lineNum As Integer = tokenBlocks(Scan0)(Scan0).span.line

            If tokenBlocks.Count = 2 Then
                If tokenBlocks(Scan0).Length = 1 Then
                    Dim unary = tokenBlocks(Scan0)(Scan0)

                    If unary = (TokenType.operator, {"-", "+"}) Then
                        If Not SyntaxResult = tokenBlocks.joinNegatives(buf, oplist, opts) Is Nothing Then
                            Return SyntaxResult
                        ElseIf buf = 1 Then
                            Return buf(Scan0).TryCast(Of SyntaxResult)
                        End If
                    ElseIf unary = (TokenType.operator, "!") Then
                        Dim value As SyntaxResult = opts.ParseExpression(tokenBlocks(1), opts)

                        If value.isException Then
                            Return value
                        Else
                            Return New UnaryNot(value.expression)
                        End If
                    End If
                End If

                Dim index As SyntaxResult = parseSymbolIndex(tokenBlocks, opts)

                If Not index Is Nothing Then
                    Return index
                End If
            End If

            If Not syntaxResult = tokenBlocks.joinNegatives(buf, oplist, opts) Is Nothing Then
                Return syntaxResult
            Else
                opts.currentLine = tokenBlocks.MeasureCurrentLine

                Return buf.ParseBinaryExpression(opts, oplist:=oplist, lineNum:=lineNum)
            End If
        End Function

        <Extension>
        Public Function ParseBinaryExpression(buf As List(Of [Variant](Of SyntaxResult, String)),
                                              opts As SyntaxBuilderOptions,
                                              Optional oplist As List(Of String) = Nothing,
                                              Optional lineNum As Integer = -1) As SyntaxResult

            Dim syntaxResult As New Value(Of SyntaxResult)
            Dim processors As GenericSymbolOperatorProcessor() = {
                New NameMemberReferenceProcessor(),
                New ArrayVectorLoopProcessor(),
                New NamespaceReferenceProcessor(),
                New PipelineProcessor(opts.pipelineSymbols), ' pipeline操作符是优先度最高的                
                New VectorAppendProcessor()                  ' append操作符
            }

            If buf = 1 Then
                Return buf(Scan0).TryCast(Of SyntaxResult)
            Else
                oplist = If(oplist, New List(Of String))
            End If

            Dim queue As New SyntaxQueue With {.buf = buf}

            For Each process As GenericSymbolOperatorProcessor In processors
                If Not (syntaxResult = process.JoinBinaryExpression(queue, oplist, opts)) Is Nothing Then
                    Return syntaxResult
                End If
                If queue.Count = 1 Then
                    Exit For
                End If
            Next

            If buf = 1 Then
                Return buf(Scan0)
            End If

            ' 算数操作符以及字符串操作符按照操作符的优先度进行构建
            If Not (syntaxResult = buf.processOperators(oplist, operatorPriority, test:=Function(op, o) op.IndexOf(o) > -1, opts)) Is Nothing Then
                Return syntaxResult
            End If

            ' 然后处理字符串操作符
            If Not (syntaxResult = buf.processOperators(oplist, {"&"}, test:=Function(op, o) op = o, opts)) Is Nothing Then
                Return syntaxResult
            End If

            ' 之后处理比较操作符
            If Not (syntaxResult = buf.processOperators(oplist, comparisonOperators, test:=Function(op, o) op = o, opts)) Is Nothing Then
                Return syntaxResult
            End If

            ' 最后处理逻辑操作符
            If Not (syntaxResult = buf.processOperators(oplist, logicalOperators, test:=Function(op, o) op = o, opts)) Is Nothing Then
                Return syntaxResult
            End If

            If buf > 1 Then
                Return buf.joinRemaining(lineNum, opts)
            Else
                Return buf(Scan0)
            End If
        End Function

        <Extension>
        Private Function MeasureCurrentLine(tokenBlocks As List(Of Token())) As Integer
            For Each block In tokenBlocks
                For Each t In block
                    If Not t.span Is Nothing Then
                        Return t.span.line
                    End If
                Next
            Next

            Return 0
        End Function

        <Extension>
        Private Function joinRemaining(buf As List(Of [Variant](Of SyntaxResult, String)), lineNum%, opts As SyntaxBuilderOptions) As SyntaxResult
            For Each a As [Variant](Of SyntaxResult, String) In buf
                If a.VA IsNot Nothing AndAlso a.VA.isException Then
                    Return a.VA
                End If
            Next

            Dim tokens As [Variant](Of Expression, String)() = buf _
                .Select(Function(a)
                            If a.VA Is Nothing Then
                                Return New [Variant](Of Expression, String)(a.VB)
                            Else
                                Return New [Variant](Of Expression, String)(a.VA.expression)
                            End If
                        End Function) _
                .ToArray

            If tokens.isByRefCall Then
                Dim sourceMap As New StackFrame With {
                    .File = opts.source.fileName,
                    .Line = lineNum,
                    .Method = New Method With {
                        .Method = tokens(Scan0).TryCast(Of Expression).ToString,
                        .[Module] = "n/a",
                        .[Namespace] = SyntaxBuilderOptions.R_runtime
                    }
                }

                Return New ByRefFunctionCall(tokens(Scan0), tokens(2), sourceMap)
            ElseIf tokens.isNamespaceReferenceCall Then
                Dim calls As FunctionInvoke = buf(2).TryCast(Of Expression)
                Dim [namespace] As Expression = buf(Scan0).TryCast(Of Expression)

                Return SyntaxResult.CreateError(New NotImplementedException, opts)
            ElseIf buf = 3 AndAlso
                (tokens(1) Like GetType(String)) AndAlso
                (tokens(1).TryCast(Of String) Like ExpressionSignature.valueAssignOperatorSymbols OrElse tokens(1).TryCast(Of String) Like iterateAssign) Then

                Dim target As Expression = tokens(Scan0).TryCast(Of Expression)
                Dim value As Expression = tokens(2)

                If tokens(1).TryCast(Of String) Like iterateAssign Then
                    value = BinaryExpressionTree.CreateBinary(target, value, tokens(1).TryCast(Of String).First, opts)
                End If

                ' set value by name
                If TypeOf tokens(Scan0).TryCast(Of Expression) Is BinaryExpression Then
                    Return New ValueAssignExpression({target}, value)
                ElseIf TypeOf target Is SymbolIndexer Then
                    Return New MemberValueAssign(target, value)
                Else
                    Return New ValueAssignExpression({target}, value)
                End If
            ElseIf tokens.isLambdaFunction Then
                Return SyntaxImplements.DeclareLambdaFunction(tokens(Scan0).VA, tokens(2).VA, lineNum, opts)
            End If

            Return SyntaxResult.CreateError(New SyntaxErrorException, opts)
        End Function

        <Extension>
        Public Function CreateBinaryExpression(buf As IEnumerable(Of [Variant](Of Expression, String)), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim raw = buf _
                .Select(Function(a)
                            If a Like GetType(String) Then
                                Return New [Variant](Of SyntaxResult, String)(a.TryCast(Of String))
                            Else
                                Return New [Variant](Of SyntaxResult, String)(New SyntaxResult(a.TryCast(Of Expression)))
                            End If
                        End Function) _
                .AsList
            Dim oplist As New List(Of String)(From t
                                              In raw
                                              Where t Like GetType(String)
                                              Select t.TryCast(Of String))

            If raw = 3 AndAlso raw(1).TryCast(Of String) = "~" Then
                ' formula expression: a ~ b
                Return New FormulaExpression(
                    y:=ValueAssignExpression.GetSymbol(raw(0).TryCast(Of SyntaxResult).expression),
                    formula:=raw(2).TryCast(Of SyntaxResult).expression
                )
            End If

            Return raw.ParseBinaryExpression(opts, oplist, 1)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="buf">
        ''' a token list for process the operators, this list input value
        ''' may be changed after the operator scanning in this function
        ''' </param>
        ''' <param name="oplist"></param>
        ''' <param name="operators">A set of the operators</param>
        ''' <param name="test">test(op, o)</param>
        ''' <returns>
        ''' this function returns the syntax error if some syntax error has been found,
        ''' otherwise returns nothing if has no error!
        ''' </returns>
        ''' <remarks>
        ''' the expression change is generated via the <paramref name="buf"/> parameter
        ''' </remarks>
        <Extension>
        Private Function processOperators(buf As List(Of [Variant](Of SyntaxResult, String)),
                                          oplist As List(Of String),
                                          operators$(),
                                          test As Func(Of String, String, Boolean),
                                          opts As SyntaxBuilderOptions) As SyntaxResult
            If buf = 1 Then
                Return Nothing
            End If

            For Each op As String In operators
                Dim nop As Integer = oplist _
                    .AsEnumerable _
                    .Count(Function(o) test(op, o))

                ' 从左往右计算
                For i As Integer = 0 To nop - 1
                    For j As Integer = 0 To buf.Count - 1
                        If buf(j) Like GetType(String) AndAlso test(op, buf(j).VB) Then
                            ' j-1 and j+1
                            Dim a As SyntaxResult = If(
                                j - 1 < 0,
                                SyntaxResult.CreateError(New SyntaxErrorException, opts),
                                buf(j - 1).TryCast(Of SyntaxResult)
                            )
                            Dim b As SyntaxResult = If(
                                j + 1 >= buf.Count,
                                SyntaxResult.CreateError(New SyntaxErrorException, opts),
                                buf(j + 1).TryCast(Of SyntaxResult)
                            )

                            If a.isException Then
                                Return a
                            ElseIf b.isException Then
                                Return b
                            End If

                            Dim opToken As String = buf(j).VB
                            Dim be As Expression = CreateBinary(a.expression, b.expression, opToken, opts)

                            Call buf.RemoveRange(j - 1, 3)
                            Call buf.Insert(j - 1, New SyntaxResult(be))

                            Exit For
                        End If
                    Next
                Next
            Next

            Return Nothing
        End Function

        Friend Function CreateBinary(a As Expression, b As Expression, opToken As String, opts As SyntaxBuilderOptions) As Expression
            If opToken = "in" Then
                Return New BinaryInExpression(a, b)
            ElseIf opToken = "between" Then
                Return New BinaryBetweenExpression(a, b)
            ElseIf opToken = "||" Then
                Return New BinaryOrExpression(a, b)
            ElseIf opToken = "|>" OrElse opToken = ":>" OrElse opToken = "→" Then
                Return PipelineProcessor.buildPipeline(a, b, opts)
            ElseIf opToken = "->" AndAlso TypeOf a Is Literal Then
                ' is a lambda expression?
                Dim unknow = StackFrame.FromUnknownLocation("CreateBinary")
                Dim symbol As New DeclareNewSymbol(DirectCast(a, Literal).ValueStr, unknow)
                Dim lambda As New DeclareLambdaFunction($"f({a}) -> {b}", symbol, b, unknow)

                ' 20221014 handling for the syntax implements:
                ' 
                ' dataframe |> rename(
                '     "#OTU ID" -> OTU_num
                ' );
                '
                Return lambda
            Else
                Return New BinaryExpression(a, b, opToken)
            End If
        End Function
    End Module
End Namespace

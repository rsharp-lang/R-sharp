Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine

    Module ExpressionTree

        <Extension>
        Public Function CreateTree(tokens As Token()) As Expression
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.comma)

            If blocks = 1 Then
                ' 是一个复杂的表达式
                Return blocks(Scan0).ParseExpressionTree
            Else
                Throw New NotImplementedException
            End If
        End Function

        <Extension>
        Private Function ParseExpressionTree(tokens As Token()) As Expression
            Dim blocks As List(Of Token())

            If tokens.Length = 1 Then
                If tokens(Scan0).name = TokenType.stringInterpolation Then
                    Return New StringInterpolation(tokens(Scan0))
                ElseIf tokens(Scan0).name = TokenType.cliShellInvoke Then
                    Return New CommandLine(tokens(Scan0))
                Else
                    blocks = New List(Of Token()) From {tokens}
                End If
            Else
                blocks = tokens.SplitByTopLevelDelimiter(TokenType.operator)
            End If

            If blocks = 1 Then
                Dim complexSequence = tokens.SplitByTopLevelDelimiter(TokenType.sequence)

                If complexSequence.Count = 3 OrElse complexSequence.Count = 5 Then
                    Return New SequenceLiteral(complexSequence(0), complexSequence(2), complexSequence.ElementAtOrDefault(4))
                End If

                ' 简单的表达式
                If tokens.isFunctionInvoke Then
                    Return New FunctionInvoke(tokens)
                ElseIf tokens.isSymbolIndexer Then
                    Return New SymbolIndexer(tokens)
                ElseIf tokens(Scan0).name = TokenType.open Then
                    Dim openSymbol = tokens(Scan0).text

                    If openSymbol = "[" Then
                        Return New VectorLiteral(tokens)
                    ElseIf openSymbol = "(" Then
                        ' 是一个表达式
                        Return tokens _
                            .Skip(1) _
                            .Take(tokens.Length - 2) _
                            .SplitByTopLevelDelimiter(TokenType.operator) _
                            .DoCall(AddressOf ParseBinaryExpression)
                    ElseIf openSymbol = "{" Then
                        ' 是一个可以产生值的closure
                        Throw New NotImplementedException
                    End If
                ElseIf tokens(Scan0).name = TokenType.stringInterpolation Then
                    Return New StringInterpolation(tokens(Scan0))
                End If
            Else
                Return ParseBinaryExpression(blocks)
            End If

            Throw New NotImplementedException
        End Function

        ReadOnly operatorPriority As String() = {"^", "*/", "+-"}
        ReadOnly comparisonOperators As String() = {"<", ">", "<=", ">=", "==", "!=", "in", "like"}
        ReadOnly logicalOperators As String() = {"&&", "||", "!"}

        <Extension>
        Public Function ParseBinaryExpression(tokenBlocks As List(Of Token())) As Expression
            Dim buf As New List(Of [Variant](Of Expression, String))
            Dim oplist As New List(Of String)

            For i As Integer = 0 To tokenBlocks.Count - 1
                If i Mod 2 = 0 Then
                    Call buf.Add(Expression.CreateExpression(tokenBlocks(i)))
                Else
                    Call buf.Add(tokenBlocks(i)(Scan0).text)
                    Call oplist.Add(buf.Last.VB)
                End If
            Next

            Call buf.processNameMemberReference(oplist)

            Call buf.processNamespaceReference(oplist)

            ' pipeline操作符是优先度最高的
            Call buf.processPipeline(oplist)

            ' 算数操作符以及字符串操作符按照操作符的优先度进行构建
            Call buf.processOperators(oplist, operatorPriority, test:=Function(op, o) op.IndexOf(o) > -1)

            ' 然后处理字符串操作符
            Call buf.processOperators(oplist, {"&"}, test:=Function(op, o) op = o)

            ' 之后处理比较操作符
            Call buf.processOperators(oplist, comparisonOperators, test:=Function(op, o) op = o)

            ' 最后处理逻辑操作符
            Call buf.processOperators(oplist, logicalOperators, test:=Function(op, o) op = o)

            If buf > 1 Then
                If buf.isByrefCall Then
                    Return New ByRefFunctionCall(buf(Scan0), buf(2))
                ElseIf buf.isNamespaceReferenceCall Then
                    Dim calls As FunctionInvoke = buf(2).TryCast(Of Expression)
                    Dim [namespace] As Expression = buf(Scan0).TryCast(Of Expression)

                    Throw New NotImplementedException
                End If

                Throw New SyntaxErrorException
            Else
                Return buf(Scan0)
            End If
        End Function

        <Extension>
        Private Sub processNameMemberReference(buf As List(Of [Variant](Of Expression, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:="$",
                expression:=Function(a, b)
                                Dim symbolRef As New SymbolIndexer(a, b)
                                Return symbolRef
                            End Function)
        End Sub

        <Extension>
        Private Sub processNamespaceReference(buf As List(Of [Variant](Of Expression, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:="::",
                expression:=Function(a, b)
                                Dim refCalls As FunctionInvoke

                                If TypeOf b.VA Is FunctionInvoke Then
                                    refCalls = b.VA
                                ElseIf TypeOf b.VA Is SymbolReference Then
                                    refCalls = New FunctionInvoke(DirectCast(b.VA, SymbolReference).symbol, a.VA)
                                Else
                                    Throw New SyntaxErrorException
                                End If

                                refCalls.namespace = a.TryCast(Of SymbolReference).symbol

                                Return refCalls
                            End Function)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Sub processPipeline(buf As List(Of [Variant](Of Expression, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:=":>",
                expression:=Function(a, b)
                                Dim pip As FunctionInvoke

                                If TypeOf b.VA Is FunctionInvoke Then
                                    pip = b.VA
                                    pip.parameters.Insert(Scan0, a.VA)
                                ElseIf TypeOf b.VA Is SymbolReference Then
                                    pip = New FunctionInvoke(DirectCast(b.VA, SymbolReference).symbol, a.VA)
                                Else
                                    Throw New SyntaxErrorException
                                End If

                                Return pip
                            End Function)
        End Sub

        <Extension>
        Private Sub genericSymbolOperatorProcessor(buf As List(Of [Variant](Of Expression, String)),
                                                   oplist As List(Of String),
                                                   opSymbol$,
                                                   expression As Func(Of [Variant](Of Expression, String), [Variant](Of Expression, String), Expression))
            If buf = 1 Then
                Return
            End If

            Dim nop As Integer = oplist _
                .AsEnumerable _
                .Count(Function(op) op = opSymbol)

            ' 从左往右计算
            For i As Integer = 0 To nop - 1
                For j As Integer = 0 To buf.Count - 1
                    If buf(j) Like GetType(String) AndAlso opSymbol = buf(j).VB Then
                        ' j-1 and j+1
                        Dim a = buf(j - 1) ' parameter
                        Dim b = buf(j + 1) ' function invoke
                        Dim exp As Expression = expression(a, b)

                        Call buf.RemoveRange(j - 1, 3)
                        Call buf.Insert(j - 1, exp)

                        Exit For
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="oplist"></param>
        ''' <param name="operators$"></param>
        ''' <param name="test">test(op, o)</param>
        <Extension>
        Private Sub processOperators(buf As List(Of [Variant](Of Expression, String)), oplist As List(Of String), operators$(), test As Func(Of String, String, Boolean))
            If buf = 1 Then
                Return
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
                            Dim a = buf(j - 1)
                            Dim b = buf(j + 1)
                            Dim be As Expression

                            If buf(j).VB = "in" Then
                                be = New FunctionInvoke("any", New BinaryExpression(a, b, "=="))
                            Else
                                be = New BinaryExpression(a, b, buf(j).VB)
                            End If

                            Call buf.RemoveRange(j - 1, 3)
                            Call buf.Insert(j - 1, be)

                            Exit For
                        End If
                    Next
                Next
            Next
        End Sub
    End Module
End Namespace
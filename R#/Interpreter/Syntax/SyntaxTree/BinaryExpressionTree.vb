#Region "Microsoft.VisualBasic::7a74ac4725a8e71d2b4c325322a6a7c8, R#\Interpreter\ExecuteEngine\BinaryExpressionTree.vb"

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

'     Module BinaryExpressionTree
' 
'         Function: buildPipeline, isFunctionTuple, ParseBinaryExpression
' 
'         Sub: genericSymbolOperatorProcessor, processAppendData, processNameMemberReference, processNamespaceReference, processOperators
'              processPipeline
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser

    Module BinaryExpressionTree

        ReadOnly operatorPriority As String() = {"^", "*/", "+-"}
        ReadOnly comparisonOperators As String() = {"<", ">", "<=", ">=", "==", "!=", "in", "like"}
        ReadOnly logicalOperators As String() = {"&&", "||", "!"}

        <Extension>
        Public Function ParseBinaryExpression(tokenBlocks As List(Of Token())) As SyntaxResult
            Dim buf As New List(Of [Variant](Of SyntaxResult, String))
            Dim oplist As New List(Of String)
            Dim syntaxResult As SyntaxResult

            If tokenBlocks(Scan0).Length = 1 AndAlso tokenBlocks(Scan0)(Scan0) = (TokenType.operator, {"-", "+"}) Then
                ' insert a ZERO before
                tokenBlocks.Insert(Scan0, {New Token With {.name = TokenType.numberLiteral, .text = 0}})
            End If

            For i As Integer = Scan0 To tokenBlocks.Count - 1
                If i Mod 2 = 0 Then
                    syntaxResult = Expression.CreateExpression(tokenBlocks(i))

                    If syntaxResult.isException Then
                        Return syntaxResult
                    Else
                        Call buf.Add(syntaxResult)
                    End If
                Else
                    Call buf.Add(tokenBlocks(i)(Scan0).text)
                    Call oplist.Add(buf.Last.VB)
                End If
            Next

            Call buf.processNameMemberReference(oplist)

            Call buf.processNamespaceReference(oplist)

            ' pipeline操作符是优先度最高的
            Call buf.processPipeline(oplist)

            ' append操作符
            Call buf.processAppendData(oplist)

            ' 算数操作符以及字符串操作符按照操作符的优先度进行构建
            Call buf.processOperators(oplist, operatorPriority, test:=Function(op, o) op.IndexOf(o) > -1)

            ' 然后处理字符串操作符
            Call buf.processOperators(oplist, {"&"}, test:=Function(op, o) op = o)

            ' 之后处理比较操作符
            Call buf.processOperators(oplist, comparisonOperators, test:=Function(op, o) op = o)

            ' 最后处理逻辑操作符
            Call buf.processOperators(oplist, logicalOperators, test:=Function(op, o) op = o)

            If buf > 1 Then
                For Each a As [Variant](Of SyntaxResult, String) In buf
                    If a.VA IsNot Nothing AndAlso a.VA.isException Then
                        Return a.VA
                    End If
                Next

                Dim tokens = buf _
                    .Select(Function(a)
                                If a.VA Is Nothing Then
                                    Return New [Variant](Of Expression, String)(a.VB)
                                Else
                                    Return New [Variant](Of Expression, String)(a.VA.expression)
                                End If
                            End Function) _
                    .ToArray

                If tokens.isByRefCall Then
                    Return New ByRefFunctionCall(tokens(Scan0), tokens(2))
                ElseIf tokens.isNamespaceReferenceCall Then
                    Dim calls As FunctionInvoke = buf(2).TryCast(Of Expression)
                    Dim [namespace] As Expression = buf(Scan0).TryCast(Of Expression)

                    Return New SyntaxResult(New NotImplementedException)
                ElseIf buf = 3 AndAlso tokens(1) Like GetType(String) AndAlso tokens(1).TryCast(Of String) Like ExpressionSignature.valueAssignOperatorSymbols Then
                    ' set value by name
                    Return New MemberValueAssign(tokens(Scan0), tokens(2))
                End If

                Return New SyntaxResult(New SyntaxErrorException)
            Else
                Return buf(Scan0)
            End If
        End Function

        <Extension>
        Private Sub processNameMemberReference(buf As List(Of [Variant](Of SyntaxResult, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:="$",
                expression:=Function(a, b)
                                Dim nameSymbol As String
                                Dim typeofName As Type = b.GetUnderlyingType

                                If a.VA.isException Then
                                    Return a
                                ElseIf b.VA.isException Then
                                    Return b
                                End If

                                If typeofName Is GetType(SymbolReference) Then
                                    nameSymbol = DirectCast(b.VA.expression, SymbolReference).symbol
                                ElseIf typeofName Is GetType(Literal) Then
                                    nameSymbol = DirectCast(b.VA.expression, Literal).value
                                ElseIf typeofName Is GetType(FunctionInvoke) Then
                                    Dim invoke As FunctionInvoke = b.VA.expression
                                    Dim funcVar As New SymbolIndexer(a.VA.expression, invoke.funcName)

                                    Return New SyntaxResult(New FunctionInvoke(funcVar, invoke.parameters.ToArray))
                                Else
                                    Return New SyntaxResult(New NotImplementedException)
                                End If

                                ' a$b symbol reference
                                Dim symbolRef As New SymbolIndexer(a.VA.expression, New Literal(nameSymbol))
                                Return New SyntaxResult(symbolRef)
                            End Function)
        End Sub

        <Extension>
        Private Sub processNamespaceReference(buf As List(Of [Variant](Of SyntaxResult, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:="::",
                expression:=Function(a, b)
                                Dim namespaceRef As Expression
                                Dim syntaxTemp As SyntaxResult = a.TryCast(Of SyntaxResult)

                                If syntaxTemp.isException Then
                                    Return syntaxTemp
                                ElseIf b.VA.isException Then
                                    Return b.VA
                                End If

                                Dim nsSymbol$ = DirectCast(syntaxTemp.expression, SymbolReference).symbol

                                If TypeOf b.VA.expression Is FunctionInvoke Then
                                    ' a::b() function invoke
                                    Dim calls As FunctionInvoke = b.VA.expression
                                    calls.namespace = nsSymbol
                                    namespaceRef = calls
                                ElseIf TypeOf b.VA.expression Is SymbolReference Then
                                    ' a::b view function help info
                                    namespaceRef = New NamespaceFunctionSymbolReference(nsSymbol, b.VA.expression)
                                Else
                                    Return New SyntaxResult(New SyntaxErrorException)
                                End If

                                Return New SyntaxResult(namespaceRef)
                            End Function)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function isFunctionTuple(b As Expression) As Boolean
            If Not TypeOf b Is VectorLiteral Then
                Return False
            ElseIf Not DirectCast(b, VectorLiteral) _
                .All(Function(e)
                         Return TypeOf e Is FunctionInvoke OrElse TypeOf e Is SymbolReference
                     End Function) Then

                Return False
            End If

            Return True
        End Function

        Private Function buildPipeline(a As Expression, b As Expression) As Expression
            Dim pip As FunctionInvoke

            If TypeOf a Is VectorLiteral Then
                With DirectCast(a, VectorLiteral)
                    If .length = 1 AndAlso TypeOf .First Is ValueAssign Then
                        a = .First
                    End If
                End With
            End If

            If TypeOf b Is FunctionInvoke Then
                pip = b
                pip.parameters.Insert(Scan0, a)
            ElseIf TypeOf b Is SymbolReference Then
                pip = New FunctionInvoke(DirectCast(b, SymbolReference).symbol, a)
            Else
                pip = Nothing
            End If

            Return pip
        End Function


        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Sub processAppendData(buf As List(Of [Variant](Of SyntaxResult, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:="<<",
                expression:=Function(a, b)
                                If a.VA.isException Then
                                    Return a
                                ElseIf b.VA.isException Then
                                    Return b
                                Else
                                    Return New SyntaxResult(New AppendOperator(a.VA.expression, b.VA.expression))
                                End If
                            End Function)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Sub processPipeline(buf As List(Of [Variant](Of SyntaxResult, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:=":>",
                expression:=Function(a, b)
                                Dim pip As Expression

                                If a.VA.isException Then
                                    Return a
                                ElseIf b.VA.isException Then
                                    Return b
                                Else
                                    pip = buildPipeline(a.VA.expression, b.VA.expression)
                                End If

                                If pip Is Nothing Then
                                    If b.VA.expression.isFunctionTuple Then
                                        Dim invokes = b.TryCast(Of VectorLiteral)
                                        Dim calls As New List(Of Expression)

                                        For Each [call] As Expression In invokes
                                            calls += buildPipeline(a.VA.expression, [call])
                                        Next

                                        Return New SyntaxResult(New VectorLiteral(calls))
                                    Else
                                        Return New SyntaxResult(New SyntaxErrorException)
                                    End If
                                Else
                                    Return New SyntaxResult(pip)
                                End If
                            End Function)
        End Sub

        <Extension>
        Private Sub genericSymbolOperatorProcessor(buf As List(Of [Variant](Of SyntaxResult, String)),
                                                   oplist As List(Of String),
                                                   opSymbol$,
                                                   expression As Func(Of [Variant](Of SyntaxResult, String), [Variant](Of SyntaxResult, String), SyntaxResult))
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
                        Dim exp As SyntaxResult = expression(a, b)

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
        Private Sub processOperators(buf As List(Of [Variant](Of SyntaxResult, String)), oplist As List(Of String), operators$(), test As Func(Of String, String, Boolean))
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
                            Dim a As SyntaxResult = buf(j - 1)
                            Dim b As SyntaxResult = buf(j + 1)
                            Dim be As Expression
                            Dim opToken As String = buf(j).VB

                            If opToken = "in" Then
                                be = New FunctionInvoke("any", New BinaryExpression(a.expression, b.expression, "=="))
                            ElseIf opToken = "||" Then
                                be = New BinaryOrExpression(a.expression, b.expression)
                            Else
                                be = New BinaryExpression(a.expression, b.expression, buf(j).VB)
                            End If

                            Call buf.RemoveRange(j - 1, 3)
                            Call buf.Insert(j - 1, New SyntaxResult(be))

                            Exit For
                        End If
                    Next
                Next
            Next
        End Sub
    End Module
End Namespace

#Region "Microsoft.VisualBasic::832963c97dc0ef1b05c39882a8f84c56, R#\Interpreter\ExecuteEngine\BinaryExpressionTree.vb"

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
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine

    Module BinaryExpressionTree

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
                If buf.isByRefCall Then
                    Return New ByRefFunctionCall(buf(Scan0), buf(2))
                ElseIf buf.isNamespaceReferenceCall Then
                    Dim calls As FunctionInvoke = buf(2).TryCast(Of Expression)
                    Dim [namespace] As Expression = buf(Scan0).TryCast(Of Expression)

                    Throw New NotImplementedException
                ElseIf buf = 3 AndAlso buf(1) Like GetType(String) AndAlso buf(1).TryCast(Of String) Like ExpressionSignature.valueAssignOperatorSymbols Then
                    ' set value by name
                    Return New MemberValueAssign(buf(Scan0), buf(2))
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
                                Dim nameSymbol As String
                                Dim typeofName As Type = b.GetUnderlyingType

                                If typeofName Is GetType(SymbolReference) Then
                                    nameSymbol = DirectCast(b.VA, SymbolReference).symbol
                                ElseIf typeofName Is GetType(Literal) Then
                                    nameSymbol = DirectCast(b.VA, Literal).value
                                ElseIf typeofName Is GetType(FunctionInvoke) Then
                                    Dim invoke As FunctionInvoke = b
                                    Dim funcVar As New SymbolIndexer(a.VA, invoke.funcName)

                                    Return New FunctionInvoke(funcVar, invoke.parameters.ToArray)
                                Else
                                    Throw New NotImplementedException
                                End If

                                ' a$b symbol reference
                                Dim symbolRef As New SymbolIndexer(a.VA, New Literal(nameSymbol))
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
        Private Sub processAppendData(buf As List(Of [Variant](Of Expression, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:="<<",
                expression:=Function(a, b)
                                Return New Append(a, b)
                            End Function)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Sub processPipeline(buf As List(Of [Variant](Of Expression, String)), oplist As List(Of String))
            Call buf.genericSymbolOperatorProcessor(
                oplist:=oplist,
                opSymbol:=":>",
                expression:=Function(a, b)
                                Dim pip As Expression = buildPipeline(a, b)

                                If pip Is Nothing Then
                                    If b.VA.isFunctionTuple Then
                                        Dim invokes = b.TryCast(Of VectorLiteral)
                                        Dim calls As New List(Of Expression)

                                        For Each [call] As Expression In invokes
                                            calls += buildPipeline(a, [call])
                                        Next

                                        Return New VectorLiteral(calls)
                                    Else
                                        Throw New SyntaxErrorException
                                    End If
                                Else
                                    Return pip
                                End If
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

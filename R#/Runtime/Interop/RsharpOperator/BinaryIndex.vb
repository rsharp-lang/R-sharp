#Region "Microsoft.VisualBasic::bfb6d1af0d1a454b07dfa04903223377, R#\Runtime\Interop\RsharpOperator\BinaryIndex.vb"

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

    '   Total Lines: 224
    '    Code Lines: 153 (68.30%)
    ' Comment Lines: 41 (18.30%)
    '    - Xml Docs: 95.12%
    ' 
    '   Blank Lines: 30 (13.39%)
    '     File Size: 8.94 KB


    '     Class BinaryIndex
    ' 
    '         Properties: symbol
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate, hasOperator, leftNull, noneValue, rightNull
    '                   ToString, typeOfImpl
    ' 
    '         Sub: addOperator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop.Operator

    ''' <summary>
    ''' execute a binary expression based on the value type selector
    ''' </summary>
    Public Class BinaryIndex : Implements IReadOnlyId

        ''' <summary>
        ''' the operator symbol text
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property symbol As String Implements IReadOnlyId.Identity

        ReadOnly operators As New List(Of BinaryOperator)

        ''' <summary>
        ''' key=hascode1|hashcode2
        ''' </summary>
        ''' <remarks>
        ''' ### 20221110 due to the reason of run parallel, so we 
        ''' needs to lock this cache list for avoid lock error.
        ''' 
        ''' System.InvalidOperationException: 
        ''' Operations that change non-concurrent collections must 
        ''' have exclusive access. A concurrent update was performed 
        ''' on this collection and corrupted its state. The
        ''' collection's state is no longer correct.
        ''' </remarks>
        ReadOnly hashIndexCache As New Dictionary(Of String, BinaryOperator)

        Sub New(symbol As String)
            Me.symbol = symbol
        End Sub

        Public Function hasOperator(left As RType, right As RType) As Boolean
            Dim key As String = $"{left}|{right}"

            SyncLock hashIndexCache
                Return hashIndexCache.ContainsKey(key)
            End SyncLock
        End Function

        ''' <summary>
        ''' 请注意，因为这个函数不会进行重复判断，所以在调用这个函数之前可以通过<see cref="hasOperator(RType, RType)"/>
        ''' 函数来判断是否重复从而决定是否对之前的操作符进行覆盖还是抛出错误信息
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="[operator]"></param>
        ''' <param name="env"></param>
        Public Sub addOperator(left As RType, right As RType, [operator] As IBinaryOperator, env As Environment)
            Dim hashKey As String = $"{left}|{right}"
            Dim bin As New BinaryOperator([operator]) With {
                .left = left,
                .right = right,
                .operatorSymbol = symbol
            }

            SyncLock hashIndexCache
                If hashIndexCache.ContainsKey(hashKey) Then
                    If Not env Is Nothing Then
                        Call env.AddMessage({
                            $"operator '{hashKey}' is replace by {bin}",
                            $"hash key: {hashKey}",
                            $"binary: {bin}"
                        }, MSG_TYPES.WRN)
                    End If

                    hashIndexCache.Remove(hashKey)
                End If

                Call operators.Add(bin)
                Call hashIndexCache.Add(hashKey, bin)
            End SyncLock
        End Sub

        Public Function Evaluate(left As Object, right As Object, traceExpr As String, env As Environment) As Object
            If left Is Nothing Then
                If Not right Is Nothing Then
                    Return leftNull(right, traceExpr, env)
                Else
                    Return noneValue(traceExpr, env)
                End If
            ElseIf right Is Nothing Then
                Return rightNull(left, traceExpr, env)
            ElseIf base.isEmpty(left) Then
                Return New Object() {}
            ElseIf base.isEmpty(right) Then
                Return New Object() {}
            Else
                Dim t1 As RType = typeOfImpl(left)
                Dim t2 As RType = typeOfImpl(right)
                Dim hashKey As String = $"{t1}|{t2}"
                Dim op_get As BinaryOperator

                SyncLock hashIndexCache
                    If hashIndexCache.ContainsKey(hashKey) Then
                        op_get = hashIndexCache(hashKey)
                    Else
                        op_get = Nothing
                    End If
                End SyncLock

                If op_get Is Nothing Then
                    ' do type match and then create hashKey index cache
                    For Each op As BinaryOperator In operators
                        If t1 Like op.left AndAlso t2 Like op.right Then
                            SyncLock hashIndexCache
                                hashIndexCache.Add(hashKey, op)
                                op_get = hashIndexCache(hashKey)
                            End SyncLock

                            Return op_get.Execute(left, right, env)
                        End If
                    Next

                    Return Internal.debug.stop({
                        $"operator symbol '{symbol}' is not defined for binary expression ({t1} {symbol} {t2})",
                        $"symbol: {symbol}",
                        $"typeof left: {t1}",
                        $"typeof right: {t2}",
                        $"traceback: {traceExpr}"
                    }, env)
                Else
                    Return op_get.Execute(left, right, env)
                End If
            End If
        End Function

        Private Shared Function typeOfImpl(x As Object) As RType
            Dim type As RType

            If TypeOf x Is vector Then
                type = DirectCast(x, vector).elementType
            Else
                type = x.GetType.DoCall(AddressOf RType.GetRSharpType)
            End If

            If type.raw Is GetType(Byte) OrElse type.raw Is GetType(Byte()) Then
                Return RType.GetRSharpType(GetType(Integer))
            Else
                Return type
            End If
        End Function

        ''' <summary>
        ''' execute a binary expression when the value of right is nothing
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function rightNull(left As Object, traceExpr As String, env As Environment) As Object
            Dim t As RType = typeOfImpl(left)

            For Each op As BinaryOperator In operators
                If t Like op.left Then
                    Return op.Execute(left, Nothing, env)
                End If
            Next

            Return Internal.debug.stop({
                $"operator symbol '{symbol}' is not defined for binary expression ({t} {symbol} NA)",
                $"symbol: {symbol}",
                $"typeof left: {t}",
                $"typeof right: NA",
                $"traceback: {traceExpr}"
            }, env)
        End Function

        Private Function noneValue(traceExpr As String, env As Environment) As Object
            Dim tVoid As RType = RType.GetRSharpType(GetType(Void))
            Dim hashKey As String = $"{tVoid}|{tVoid}"
            Dim op As BinaryOperator

            SyncLock hashIndexCache
                If hashIndexCache.ContainsKey(hashKey) Then
                    op = hashIndexCache(hashKey)
                Else
                    Return Internal.debug.stop({
                         $"operator symbol '{symbol}' is not defined for binary expression (NULL {symbol} NULL)",
                         $"symbol: {symbol}",
                         $"traceback: {traceExpr}"
                    }, env)
                End If
            End SyncLock

            Return op.Execute(Nothing, Nothing, env)
        End Function

        ''' <summary>
        ''' execute a binary expression when the value of left is nothing
        ''' </summary>
        ''' <param name="right"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function leftNull(right As Object, traceExpr As String, env As Environment) As Object
            Dim t As RType = typeOfImpl(right)

            For Each op As BinaryOperator In operators
                If t Like op.right Then
                    Return op.Execute(Nothing, right, env)
                End If
            Next

            Return Internal.debug.stop({
                $"operator symbol '{symbol}' is not defined for binary expression (NA {symbol} {t})",
                $"symbol: {symbol}",
                $"typeof left: NA",
                $"typeof right: {t}",
                $"traceback: {traceExpr}"
            }, env)
        End Function

        Public Overrides Function ToString() As String
            Return symbol
        End Function
    End Class
End Namespace

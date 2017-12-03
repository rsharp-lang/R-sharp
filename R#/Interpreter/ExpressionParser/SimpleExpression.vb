#Region "Microsoft.VisualBasic::b4fa48d3bd7999188236f813540e149b, ..\R-sharp\R#\Interpreter\ExpressionParser\SimpleExpression.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.CodeDOM
Imports SMRUCC.Rsharp.Runtime.PrimitiveTypes

Namespace Interpreter.Expression

    ''' <summary>
    ''' A class object stand for a very simple mathematic expression that have no bracket or function.
    ''' It only contains limited operator such as +-*/\%!^ in it.
    ''' (一个用于表达非常简单的数学表达式的对象，在这个所表示的简单表达式之中不能够包含有任何括号或者函数，
    ''' 其仅包含有有限的计算符号在其中，例如：+-*/\%^!)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SimpleExpression : Implements IEnumerable(Of MetaExpression)

        ''' <summary>
        ''' A simple expression can be view as a list collection of meta expression.
        ''' (可以将一个简单表达式看作为一个元表达式的集合)
        ''' </summary>
        ''' <remarks></remarks>
        Dim MetaList As New List(Of MetaExpression)

        ''' <summary>
        ''' The last operator of this expression.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LastOperator As Char
            Get
                Return MetaList.Last.Operator
            End Get
        End Property

        ''' <summary>
        ''' 通过比较引用深度来决定在系统初始化的时候表达式对象的计算的先后顺序，深度越小的约优先计算
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ReferenceDepth As Integer
            Get
                Return Me.Sum(Function(x) x.ReferenceDepth)
            End Get
        End Property

        Public ReadOnly Property IsNullOrEmpty As Boolean
            Get
                If MetaList.Count = 0 Then
                    Return True
                Else
                    If MetaList.Count = 1 Then
                        Dim first As MetaExpression = MetaList.First
                        Return first.IsValue AndAlso
                        first.LEFT = 0R AndAlso
                        first.Operator = "+"c
                    Else
                        Return False
                    End If
                End If
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(n As Double)
            MetaList += New MetaExpression With {
                .LEFT = n, .Operator = "+"
            }
        End Sub

        Public Sub Add(n As Double, o As Char)
            MetaList += New MetaExpression With {
                .LEFT = n,
                .Operator = o
            }
        End Sub

        Public Sub Add(meta As MetaExpression)
            MetaList += meta
        End Sub

        ''' <summary>
        ''' Debugging displaying in VS IDE
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return String.Join("", (From item In MetaList Let s As String = item.ToString Select s).ToArray)
        End Function

        ''' <summary>
        ''' Evaluate the specific simple expression class object.
        ''' (计算一个特定的简单表达式对象的值，**这个<see cref="SimpleExpression"/>简单表达式对象可以被重复利用的**，
        ''' 因为引用了变量或者函数的话<see cref="MetaExpression"/>会使用lambda表达式进行求值，
        ''' 所以只需要使用方法<see cref="Environment.Value(String)"/>改变引擎之中的环境变量就行了) 
        ''' </summary>
        ''' <returns>
        ''' Return the value of the specific simple expression object.
        ''' (返回目标简单表达式对象的值)
        ''' </returns>
        ''' <remarks></remarks>
        Public Function Evaluate(envir As Environment) As TempValue
            If Me.MetaList.Count = 0 Then
                Return Nothing
            End If

            ' 将数据隔绝开，这样子这个表达式对象可以被重复使用
            Dim metaList As New List(Of MetaExpression)(Me.MetaList)
            Dim leftValue = Function() As TempValue
                                With metaList.First
                                    Dim value As Object = .LEFT

                                    If .LeftType = TypeCodes.ref Then
                                        Dim var = envir(CStrSafe(value))
                                        Return TempValue.Tuple(var.Value, var.TypeCode)
                                    ElseIf Not value Is Nothing AndAlso value.GetType Is Core.TypeDefine(Of TempValue).BaseType Then
                                        Return DirectCast(value, TempValue)
                                    Else
                                        Return TempValue.Tuple(value, .LeftType)
                                    End If
                                End With
                            End Function

            ' When the list object only contains one element, that means this class object 
            ' only stands For a number, return this number directly. 
            If metaList.Count = 1 Then
                Return leftValue()
            Else
                CalculatorInternal({"^"}, metaList, envir)
                CalculatorInternal({"*", "/", "\", "%"}, metaList, envir)
                CalculatorInternal({"+", "-"}, metaList, envir)
                CalculatorInternal({"and", "or"}, metaList, envir)

                Return leftValue()
            End If
        End Function

        Private Shared Sub CalculatorInternal(operators$(), ByRef tokenList As List(Of MetaExpression), envir As Environment)
            ' Defines a LINQ query use for select the meta element that 
            ' Contains target operator..Count
            Dim countOp% = tokenList _
                .Where(Function(mep) operators.IndexOf(mep.Operator) > -1) _
                .Count

            If countOp = 0 Then
                Return
            End If

            Dim left, right As MetaExpression
            Dim x As Object

            ' Scan the expression object and do the calculation at the mean time
            For index As Integer = 0 To tokenList.Count - 1

                ' No more calculation could be done since there is only 
                ' one Number In the expression, break at this situation.
                If countOp = 0 OrElse tokenList.Count = 1 Then
                    Return

                ElseIf operators.IndexOf(tokenList(index).Operator) <> -1 Then

                    ' We find a meta expression element that contains target operator, then 
                    ' we do calculation on this element and the element next to it.  

                    ' Get current element and the element that next to him
                    left = tokenList(index)
                    right = tokenList(index + 1)

                    ' Do some calculation of type target operator 
                    x = ExecuteEngine.EvaluateBinary(envir, left, right, left.Operator)

                    ' When the current element is calculated, it is no use anymore, 
                    ' we Remove it
                    tokenList.RemoveAt(index)
                    tokenList(index) = New MetaExpression(x, right.Operator)

                    ' Keep the reading position order
                    index -= 1

                    ' If the target operator is position at the front side of the expression, 
                    ' Using this flag will make the For Loop Exit When all Of the target 
                    ' Operator Is calculated To improve the performance As no needs To scan 
                    ' All Of the expression at this situation. 
                    countOp -= 1
                End If
            Next
        End Sub

        Public Function RemoveLast() As MetaExpression
            Dim last As MetaExpression = MetaList.Last
            Call MetaList.RemoveLast
            Return last
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of MetaExpression) Implements IEnumerable(Of MetaExpression).GetEnumerator
            For Each x As MetaExpression In MetaList
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(meta As MetaExpression) As SimpleExpression
            Return New SimpleExpression With {
                .MetaList = New List(Of MetaExpression) From {meta}
            }
        End Operator
    End Class
End Namespace

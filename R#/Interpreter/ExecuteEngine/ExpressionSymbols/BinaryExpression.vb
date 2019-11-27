#Region "Microsoft.VisualBasic::96b63464c72ebb78d0a0a6d6146f2857, R#\Interpreter\ExecuteEngine\ExpressionSymbols\BinaryExpression.vb"

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

'     Class BinaryExpression
' 
'         Properties: type
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: DoStringJoin, Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class BinaryExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim left, right As Expression
        Dim [operator] As String

        Sub New(left As Expression, right As Expression, op$)
            Me.left = left
            Me.right = right
            Me.operator = op
        End Sub

        Friend Shared ReadOnly integers As Index(Of Type) = {
            GetType(Integer), GetType(Integer()),
            GetType(Long), GetType(Long())
        }

        Friend Shared ReadOnly floats As Index(Of Type) = {
            GetType(Single), GetType(Single()),
            GetType(Double), GetType(Double())
        }

        Friend Shared ReadOnly logicals As Index(Of Type) = {
            GetType(Boolean),
            GetType(Boolean())
        }

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim a As Object = left.Evaluate(envir)
            Dim b As Object = right.Evaluate(envir)
            Dim ta = a.GetType
            Dim tb = b.GetType

            If Program.isException(a) Then
                Return a
            ElseIf Program.isException(b) Then
                Return b
            End If

            If ta Like integers Then
                If tb Like integers Then
                    Select Case [operator]
                        Case "+" : Return Runtime.Core.Add(Of Long, Long, Long)(a, b).ToArray
                        Case "-" : Return Runtime.Core.Minus(Of Long, Long, Long)(a, b).ToArray
                        Case "*" : Return Runtime.Core.Multiply(Of Long, Long, Long)(a, b).ToArray
                        Case "/" : Return Runtime.Core.Divide(Of Long, Long, Double)(a, b).ToArray
                        Case "^" : Return Runtime.Core.Power(Of Long, Long, Double)(a, b).ToArray
                        Case ">" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x > y).ToArray
                        Case "<" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x < y).ToArray
                        Case "!=" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x <> y).ToArray
                        Case "==" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x = y).ToArray
                        Case ">=" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x >= y).ToArray
                        Case "<=" : Return Runtime.Core.BinaryCoreInternal(Of Long, Long, Boolean)(a, b, Function(x, y) x <= y).ToArray
                    End Select
                ElseIf tb Is GetType(Double) OrElse tb Is GetType(Double()) Then
                    Select Case [operator]
                        Case "+" : Return Runtime.Core.Add(Of Long, Double, Double)(a, b).ToArray
                        Case "-" : Return Runtime.Core.Minus(Of Long, Double, Double)(a, b).ToArray
                        Case "*" : Return Runtime.Core.Multiply(Of Long, Double, Double)(a, b).ToArray
                        Case "/" : Return Runtime.Core.Divide(Of Long, Double, Double)(a, b).ToArray
                        Case "^" : Return Runtime.Core.Power(Of Long, Double, Double)(a, b).ToArray
                    End Select
                End If
            ElseIf ta Like floats Then
                If tb Like integers Then

                    Select Case [operator]
                        Case "+" : Return Runtime.Core.Add(Of Double, Long, Double)(a, b).ToArray
                        Case "-" : Return Runtime.Core.Minus(Of Double, Long, Double)(a, b).ToArray
                        Case "*" : Return Runtime.Core.Multiply(Of Double, Long, Double)(a, b).ToArray
                        Case "/" : Return Runtime.Core.Divide(Of Double, Long, Double)(a, b).ToArray

                    End Select
                ElseIf tb Like floats Then
                    Select Case [operator]
                        Case "+" : Return Runtime.Core.Add(Of Double, Double, Double)(a, b).ToArray
                    End Select
                End If
            ElseIf ta Is GetType(String) OrElse tb Is GetType(String) Then
                If [operator] = "&" Then
                    Return DoStringJoin(a, b)
                Else
                    Throw New InvalidExpressionException
                End If
            Else

                If [operator] = "||" OrElse [operator] = "&&" Then
                    Dim op As Func(Of Object, Object, Object)

                    If [operator] = "||" Then
                        op = Function(x, y) x Or y
                    Else
                        op = Function(x, y) x And y
                    End If

                    Return Runtime.Core _
                        .BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                            x:=Core.asLogical(a),
                            y:=Core.asLogical(b),
                            [do]:=op
                        ).ToArray
                End If

                If ta Like logicals AndAlso tb Like logicals Then
                    Select Case [operator]
                        Case "=="
                            Return Runtime.Core.BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                                x:=Runtime.asVector(Of Boolean)(a),
                                y:=Runtime.asVector(Of Boolean)(b),
                                [do]:=Function(x, y) x = y
                            ).ToArray
                        Case "!="
                            Return Runtime.Core.BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                                x:=Runtime.asVector(Of Boolean)(a),
                                y:=Runtime.asVector(Of Boolean)(b),
                                [do]:=Function(x, y) x <> y
                            ).ToArray
                    End Select
                End If

            End If

            Throw New NotImplementedException($"<{ta.FullName}> {[operator]} <{tb.FullName}>")
        End Function

        Public Shared Function DoStringJoin(a As Object, b As Object) As String()
            Dim va = getStringArray(a).ToArray
            Dim vb = getStringArray(b).ToArray
            Dim append = Function(x, y) x & y

            Return Runtime.Core _
                .BinaryCoreInternal(Of String, String, String)(va, vb, append) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function getStringArray(a As Object) As IEnumerable(Of String)
            Return From element As Object
                   In Runtime _
                       .asVector(Of Object)(a) _
                       .AsQueryable
                   Let str As String = Scripting.ToString(element, "NULL")
                   Select str
        End Function

        Public Overrides Function ToString() As String
            Return $"{left} {[operator]} {right}"
        End Function
    End Class
End Namespace

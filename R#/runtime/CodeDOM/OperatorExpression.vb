#Region "Microsoft.VisualBasic::e38a68ca34c93293e81e832cde335d3b, ..\R-sharp\R#\runtime\CodeDOM\OperatorExpression.vb"

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

Namespace Runtime.CodeDOM

    ''' <summary>
    ''' Logical and arithmetic expression
    ''' </summary>
    Public MustInherit Class OperatorExpression : Inherits PrimitiveExpression

        Public Property [Operator] As String

    End Class

    ''' <summary>
    ''' 单目运算符表达式
    ''' </summary>
    Public Class UnaryOperator : Inherits OperatorExpression

        Public Property Expression As PrimitiveExpression

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class BinaryOperator : Inherits OperatorExpression

        Public Property left As PrimitiveExpression
        Public Property right As PrimitiveExpression

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Throw New NotImplementedException()
        End Function
    End Class

    ''' <summary>
    ''' ```R
    ''' a &lt;- b
    ''' ```
    ''' </summary>
    Public Class ValueAssign : Inherits BinaryOperator

        Public Property IsByRef As Boolean = False

        Sub New()
        End Sub

        Sub New(var$)
            left = New VariableReference(var)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            ' a是对变量的引用
            ' b是变量表达式
            Dim var As Variable = DirectCast(left, VariableReference).Evaluate(envir).value
            Dim value = right.Evaluate(envir)

            var.Value = value.value

            If Not var.ConstraintValid Then
                Throw New InvalidCastException
            Else
                Return value
            End If
        End Function

        Public Overrides Function ToString() As String
            If IsByRef Then
                Return $"{left} = {right}"
            Else
                Return $"{left} <- {right}"
            End If
        End Function
    End Class
End Namespace

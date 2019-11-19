#Region "Microsoft.VisualBasic::d28e8de3cc8c48e4f42f440323587aea, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\SequenceLiteral.vb"

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

    '     Class SequenceLiteral
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ' from:to:steps

    Public Class SequenceLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim from As Expression
        Dim [to] As Expression
        Dim steps As Expression

        Sub New(from As Token, [to] As Token, steps As Token)
            Me.from = Expression.CreateExpression({from})
            Me.to = Expression.CreateExpression({[to]})

            If steps Is Nothing Then
                Me.steps = New Literal(1)
            ElseIf steps.name = TokenType.numberLiteral OrElse steps.name = TokenType.integerLiteral Then
                Me.steps = New Literal(steps)
            Else
                Me.steps = Expression.CreateExpression({steps})
            End If
        End Sub

        Sub New(from As Token(), [to] As Token(), steps As Token())
            Me.from = Expression.CreateExpression(from)
            Me.to = Expression.CreateExpression([to])

            If steps.IsNullOrEmpty Then
                Me.steps = New Literal(1)
            ElseIf steps.isLiteral Then
                Me.steps = New Literal(steps(Scan0))
            Else
                Me.steps = Expression.CreateExpression(steps)
            End If
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim init = from.Evaluate(envir)
            Dim stops = [to].Evaluate(envir)
            Dim offset = steps.Evaluate(envir)

            If {init, stops, offset}.Any(Function(num)
                                             Dim ntype As Type = num.GetType

                                             If ntype Is GetType(Double) OrElse ntype Is GetType(Double()) Then
                                                 Return True
                                             Else
                                                 Return False
                                             End If
                                         End Function) Then
                Dim start As Double = Runtime.getFirst(init)
                Dim steps As Double = Runtime.getFirst(offset)
                Dim ends As Double = Runtime.getFirst(stops)
                Dim seq As New List(Of Double)

                Do While start <= ends
                    seq += start
                    start += steps
                Loop

                Return seq.ToArray
            Else
                Dim start As Long = Runtime.getFirst(init)
                Dim steps As Long = Runtime.getFirst(offset)
                Dim ends As Integer = Runtime.getFirst(stops)
                Dim seq As New List(Of Long)

                Do While start <= ends
                    seq += start
                    start += steps
                Loop

                Return seq.ToArray
            End If
        End Function
    End Class
End Namespace

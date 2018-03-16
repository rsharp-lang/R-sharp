#Region "Microsoft.VisualBasic::7f9b5aac14b8e3a67bdec1bca462814a, R#\Interpreter\ExpressionParser\ExecuteEngine.vb"

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

    '     Module ExecuteEngine
    ' 
    '         Function: (+2 Overloads) EvaluateBinary, (+2 Overloads) EvaluateUnary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.Expression

    ''' <summary>
    ''' Execute engine for operators
    ''' </summary>
    Public Module ExecuteEngine

        ''' <summary>
        ''' Operator expression evaluate
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="operator$"></param>
        ''' <returns></returns>
        <Extension> Public Function EvaluateBinary(envir As Environment, left As Variable, right As Variable, operator$) As Object
            Dim typeA As RType = envir.RType(left)
            Dim typeB As RType = envir.RType(right)
            Dim op_method As New Value(Of MethodInfo)

            ' find operator based on the type schema
            If (op_method = typeA.GetBinaryOperator1([operator], typeB)) Is Nothing Then
                op_method = typeB.GetBinaryOperator2([operator], typeA)
            End If

            With op_method?.Value
                If .IsNothing Then
                    Throw New InvalidOperationException($"Operator `{[operator]}` between type {typeA} and {typeB} is undefined!")
                Else
                    Return .Invoke(Nothing, BindingFlags.Static, Nothing, {left.Value, right.Value}, Nothing)
                End If
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function EvaluateBinary(envir As Environment, left As MetaExpression, right As MetaExpression, operator$) As Object
            Return envir.EvaluateBinary(envir.GetValue(left), envir.GetValue(right), [operator])
        End Function

        <Extension> Public Function EvaluateUnary(envir As Environment, x As Variable, operator$) As Object
            Dim type As RType = envir.RType(x)
            Dim method = type.GetUnaryOperator([operator])

            If method Is Nothing Then
                Throw New NotImplementedException($"Operator '{[operator]}' is not defined!")
            Else
                Return method.Invoke(Nothing, BindingFlags.Static, Nothing, {x.Value, Nothing}, Nothing)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function EvaluateUnary(envir As Environment, x As MetaExpression, operator$) As Object
            Return envir.EvaluateUnary(envir.GetValue(x), [operator])
        End Function
    End Module
End Namespace

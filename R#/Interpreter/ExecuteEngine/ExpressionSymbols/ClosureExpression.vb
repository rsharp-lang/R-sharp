#Region "Microsoft.VisualBasic::8ef4535e5cc5a4935638f191f6c959ca, R#\Interpreter\ExecuteEngine\ExpressionSymbols\ClosureExpression.vb"

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

    '     Class ClosureExpression
    ' 
    '         Properties: isEmpty, type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate, ParseExpressionTree
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' 其实就是一段拥有自己的<see cref="Environment"/>的没有名称的匿名函数
    ''' </summary>
    Public Class ClosureExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public ReadOnly Property isEmpty As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Get
                Return program.execQueue.IsNullOrEmpty
            End Get
        End Property

        Dim program As Program

        Sub New(tokens As IEnumerable(Of Token))
            program = tokens _
                .ToArray _
                .DoCall(AddressOf Program.CreateProgram)
        End Sub

        <DebuggerStepThrough>
        Sub New(code As Expression())
            program = New Program With {
                .execQueue = code
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return program.Execute(envir)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function ParseExpressionTree(tokens As IEnumerable(Of Token)) As ClosureExpression
            Return New ClosureExpression(tokens)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Widening Operator CType(code As Expression()) As ClosureExpression
            Return New ClosureExpression(code)
        End Operator
    End Class
End Namespace

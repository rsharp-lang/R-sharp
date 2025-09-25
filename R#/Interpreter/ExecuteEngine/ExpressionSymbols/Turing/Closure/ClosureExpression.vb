﻿#Region "Microsoft.VisualBasic::17a3c48837337b8d75b7b0f805aab289, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\ClosureExpression.vb"

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

    '   Total Lines: 87
    '    Code Lines: 65 (74.71%)
    ' Comment Lines: 9 (10.34%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (14.94%)
    '     File Size: 3.08 KB


    '     Class ClosureExpression
    ' 
    '         Properties: bodySize, expressionName, isEmpty, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumerateCodeLines, Evaluate, ParseExpressionTree, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File
Imports Microsoft.VisualBasic.Language.[Default]

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' Is a kind of expression collection which consist with multiple 
    ''' expression lines.
    ''' (其实就是一段拥有自己的<see cref="Environment"/>的没有名称的匿名函数)
    ''' </summary>
    Public Class ClosureExpression : Inherits Expression
        Implements IsEmpty

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.ClosureDeclare
            End Get
        End Property

        Public ReadOnly Property isEmpty As Boolean Implements IsEmpty.IsEmpty
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Get
                Return program.lines = 0
            End Get
        End Property

        Public ReadOnly Property bodySize As Integer
            Get
                Return program.lines
            End Get
        End Property

        Protected Friend ReadOnly program As Program

        ''' <summary>
        ''' construct closure from a collection of the multiple expression
        ''' </summary>
        ''' <param name="code"></param>
        <DebuggerStepThrough>
        Sub New(ParamArray code As Expression())
            program = New Program(code)
        End Sub

        Public Const FunctionExitCode = "$(on.exit)"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As Object = program.Execute(envir)
            Dim on_exit As Expression = TryCast(envir.acceptorArguments.TryGetValue(FunctionExitCode, mute:=True), Expression)

            If Not on_exit Is Nothing Then
                Dim result2 As Object = on_exit.Evaluate(envir)

                If Program.isException(result2) Then
                    Return result2
                End If

                If TypeOf result Is RsharpDataObject Then
                    Call DirectCast(result, RsharpDataObject).setAttribute("on.exit", result2)
                End If
            End If

            Return result
        End Function

        Public Iterator Function EnumerateCodeLines() As IEnumerable(Of Expression)
            For Each line As Expression In program
                Yield line
            Next
        End Function

        Public Overrides Function ToString() As String
            If program.Rscript Is Nothing Then
                Return program.ToString
            Else
                Return program.Rscript.ToString & ": " & vbCrLf &
                       program.ToString
            End If
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

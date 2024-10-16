﻿#Region "Microsoft.VisualBasic::e6eb8774d9ff35937db7f5463285566b, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\If\IIfExpression.vb"

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

    '   Total Lines: 65
    '    Code Lines: 43 (66.15%)
    ' Comment Lines: 12 (18.46%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (15.38%)
    '     File Size: 2.29 KB


    '     Class IIfExpression
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    ''' <summary>
    ''' ifelse expression
    ''' </summary>
    Public Class IIfExpression : Inherits Expression
        Implements IRuntimeTrace

        Friend ReadOnly ifTest As Expression
        Friend ReadOnly trueResult As Expression
        Friend ReadOnly falseResult As Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return trueResult.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.IIf
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(iftest As Expression, trueResult As Expression, falseResult As Expression, stackFrame As StackFrame)
            Me.ifTest = iftest
            Me.trueResult = trueResult
            Me.falseResult = falseResult
            Me.stackFrame = stackFrame
        End Sub

        ''' <summary>
        ''' if test true, then returns true part else returns false part
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim ifTestResult = ifTest.Evaluate(envir)
            Dim test As Boolean = CLRVector.asLogical(ifTestResult)(Scan0)

            If test = True Then
                Return trueResult.Evaluate(envir)
            Else
                Return falseResult.Evaluate(envir)
            End If
        End Function

        ''' <summary>
        ''' javascript ifelse syntax
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"({ifTest} ? {trueResult} : {falseResult})"
        End Function
    End Class
End Namespace

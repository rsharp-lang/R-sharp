﻿#Region "Microsoft.VisualBasic::ac3de0fdf88a17d3e2c41175c522f23c, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\ReturnValue.vb"

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

    '   Total Lines: 57
    '    Code Lines: 41 (71.93%)
    ' Comment Lines: 7 (12.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (15.79%)
    '     File Size: 1.89 KB


    '     Class ReturnValue
    ' 
    '         Properties: expressionName, IsRuntimeFunctionReturnWrapper, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' returns the value from the function stack and break the executation loop of the function
    ''' </summary>
    Public Class ReturnValue : Inherits Expression

        Friend ReadOnly value As Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                If value Is Nothing Then
                    Return TypeCodes.NA
                Else
                    Return value.type
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Return
            End Get
        End Property

        Public ReadOnly Property IsRuntimeFunctionReturnWrapper As Boolean
            Get
                Return Not value Is Nothing AndAlso TypeOf value Is RuntimeValueLiteral
            End Get
        End Property

        ''' <summary>
        ''' create a new function return expression
        ''' </summary>
        ''' <param name="value"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(value As Expression)
            Me.value = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Me.value.Evaluate(envir)
        End Function

        Public Overrides Function ToString() As String
            Return $"return {value.ToString}"
        End Function
    End Class
End Namespace

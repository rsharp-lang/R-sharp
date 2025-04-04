﻿#Region "Microsoft.VisualBasic::e20bd13ffaf6df9e2bee2bfcf8204de8, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\UnaryNumeric.vb"

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

    '   Total Lines: 69
    '    Code Lines: 52 (75.36%)
    ' Comment Lines: 6 (8.70%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (15.94%)
    '     File Size: 2.43 KB


    '     Class UnaryNumeric
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop.Operator

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class UnaryNumeric : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If numeric.type = TypeCodes.generic Then
                    Return TypeCodes.double
                Else
                    Return numeric.type
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ExpressionTypes.UnaryNot
            End Get
        End Property

        ''' <summary>
        ''' +-!
        ''' </summary>
        Friend ReadOnly [operator] As String
        ''' <summary>
        ''' 可能是一个符号，也可以能是一个对象引用
        ''' </summary>
        Friend ReadOnly numeric As Expression

        Sub New(op As String, number As Expression)
            Me.operator = op
            Me.numeric = number
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object = numeric.Evaluate(envir)

            If Program.isException(value) Then
                Return value
            End If

            Select Case [operator]
                Case "-"
                    Dim handleResult As [Variant](Of BinaryIndex, Message) = BinaryOperatorEngine.getOperator([operator], envir)

                    If handleResult Like GetType(Message) Then
                        Return handleResult.TryCast(Of Message)
                    Else
                        Return handleResult.TryCast(Of BinaryIndex).Evaluate(0, value, Me.ToString, envir)
                    End If
                Case Else
                    Throw New NotImplementedException([operator])
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"{[operator]}{numeric}"
        End Function
    End Class
End Namespace

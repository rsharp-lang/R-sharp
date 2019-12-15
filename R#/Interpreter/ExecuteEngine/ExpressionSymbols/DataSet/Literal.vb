#Region "Microsoft.VisualBasic::4aee4a7a8a8f0d27009d4682df16bd1e, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\Literal.vb"

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

    '     Class Literal
    ' 
    '         Properties: [FALSE], [TRUE], NULL, type
    ' 
    '         Constructor: (+5 Overloads) Sub New
    '         Function: Evaluate, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class Literal : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Friend value As Object

        Public Shared ReadOnly Property NULL As Literal
            Get
                Return New Literal With {.value = Nothing}
            End Get
        End Property

        Public Shared ReadOnly Property [TRUE] As Literal
            Get
                Return New Literal(True)
            End Get
        End Property

        Public Shared ReadOnly Property [FALSE] As Literal
            Get
                Return New Literal(False)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(token As Token)
            Select Case token.name
                Case TokenType.booleanLiteral
                    type = TypeCodes.boolean
                    value = token.text.ParseBoolean
                Case TokenType.integerLiteral
                    type = TypeCodes.integer
                    value = CLng(token.text.ParseInteger)
                Case TokenType.numberLiteral
                    type = TypeCodes.double
                    value = token.text.ParseDouble
                Case TokenType.stringLiteral, TokenType.cliShellInvoke
                    type = TypeCodes.string
                    value = token.text
                Case TokenType.missingLiteral
                    type = TypeCodes.generic

                    Select Case token.text
                        Case "NULL" : value = Nothing
                        Case "NA" : value = GetType(Void)
                        Case "Inf" : value = Double.PositiveInfinity
                        Case Else
                            Throw New SyntaxErrorException
                    End Select

                Case Else
                    Throw New InvalidExpressionException(token.ToString)
            End Select
        End Sub

        Sub New(value As Double)
            Me.type = TypeCodes.double
            Me.value = value
        End Sub

        Sub New(value As String)
            Me.type = TypeCodes.string
            Me.value = value
        End Sub

        Sub New(value As Boolean)
            Me.type = TypeCodes.boolean
            Me.value = value
        End Sub

        Sub New(value As Integer)
            Me.type = TypeCodes.integer
            Me.value = CLng(value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return value
        End Function

        ''' <summary>
        ''' Get string representation of the literal object value
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        Public Overrides Function ToString() As String
            If value Is Nothing Then
                Return "NULL"
            Else
                Return value.ToString
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(exp As Literal, literal As String) As Boolean
            Return DirectCast(exp.value, String) = literal
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(exp As Literal, literal As String) As Boolean
            Return Not exp = literal
        End Operator
    End Class
End Namespace

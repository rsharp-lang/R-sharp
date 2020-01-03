#Region "Microsoft.VisualBasic::0981e23029019a2c10198ccaf842efb3, R#\Interpreter\ExecuteEngine\ExpressionSymbols\ReturnValue.vb"

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

'     Class ReturnValue
' 
'         Properties: type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine

    Public Class ReturnValue : Inherits Expression

        ReadOnly value As Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                If value Is Nothing Then
                    Return TypeCodes.NA
                Else
                    Return value.type
                End If
            End Get
        End Property

        Public ReadOnly Property IsRuntimeFunctionReturnWrapper As Boolean
            Get
                Return Not value Is Nothing AndAlso TypeOf value Is RuntimeValueLiteral
            End Get
        End Property

        Sub New(value As IEnumerable(Of Token))
            With value.ToArray
                ' just return
                ' no value
                If .Length = 0 Then
                    Me.value = Literal.NULL
                Else
                    Me.value = .DoCall(AddressOf Expression.CreateExpression)
                End If
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

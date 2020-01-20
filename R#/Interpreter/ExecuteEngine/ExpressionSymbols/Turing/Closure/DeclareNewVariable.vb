#Region "Microsoft.VisualBasic::b4fdbfab911f7f084277410614ebabd3, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\DeclareNewVariable.vb"

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

'     Class DeclareNewVariable
' 
'         Properties: type
' 
'         Constructor: (+4 Overloads) Sub New
' 
'         Function: Evaluate, getNames, getParameterView, PushNames, ToString
' 
'         Sub: getInitializeValue, PushTuple
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class DeclareNewVariable : Inherits Expression

        ''' <summary>
        ''' 对于tuple类型，会存在多个变量
        ''' </summary>
        Friend names As String()
        ''' <summary>
        ''' 初始值
        ''' </summary>
        Friend value As Expression
        Friend hasInitializeExpression As Boolean = False

        Public Overrides ReadOnly Property type As TypeCodes

        Sub New()
        End Sub

        ''' <summary>
        ''' Push current variable into the target environment ``<paramref name="envir"/>``.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object

            If Me.value Is Nothing Then
                value = Nothing
            Else
                value = Me.value.Evaluate(envir)
            End If

            If Program.isException(value) Then
                Return value
            Else
                ' add new symbol into the given environment stack
                ' and then returns the value result
                Call PushNames(names, value, type, envir)

                Return value
            End If
        End Function

        Friend Shared Function PushNames(names$(), value As Object, type As TypeCodes, envir As Environment) As Environment
            If names.Length = 1 Then
                Call envir.Push(names(Scan0), value, type)
            Else
                ' tuple
                Call PushTuple(names, value, type, envir)
            End If

            Return envir
        End Function

        Private Shared Sub PushTuple(names$(), value As Object, type As TypeCodes, envir As Environment)
            If Not value Is Nothing AndAlso value.GetType.IsArray Then
                Dim vector As Array = value

                If vector.Length = 1 Then
                    ' all set with one value
                    For Each name As String In names
                        Call envir.Push(name, value)
                    Next
                ElseIf vector.Length = names.Length Then
                    ' declare one by one
                    For i As Integer = 0 To vector.Length - 1
                        Call envir.Push(names(i), vector.GetValue(i))
                    Next
                Else
                    Throw New SyntaxErrorException
                End If
            Else
                ' all set with one value
                For Each name As String In names
                    Call envir.Push(name, value)
                Next
            End If
        End Sub

        Public Overrides Function ToString() As String
            If names.Length > 1 Then
                Return $"Dim [{names.JoinBy(", ")}] As {type.Description} = {Scripting.ToString(value, "NULL")}"
            Else
                Return $"Dim {names(Scan0)} As {type.Description} = {Scripting.ToString(value, "NULL")}"
            End If
        End Function

        Friend Shared Function getParameterView(declares As DeclareNewVariable) As String
            Dim a$

            If declares.names.Length = 1 Then
                a = declares.names(Scan0)
            Else
                a = declares.names.JoinBy(", ")
                a = $"[{a}]"
            End If

            If declares.hasInitializeExpression Then
                Return $"{a} = {declares.value}"
            Else
                Return a
            End If
        End Function
    End Class
End Namespace

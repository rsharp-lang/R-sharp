#Region "Microsoft.VisualBasic::67f4b7a744aacdbf981bc9c582b12886, R#\Interpreter\Program.vb"

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

'     Class Program
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: BuildProgram, CreateProgram, Execute, GetEnumerator, IEnumerable_GetEnumerator
'                   isException, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter

    Public Class Program : Implements IEnumerable(Of Expression)

        Friend execQueue As Expression()
        Friend debug As Boolean = False

        Sub New()
        End Sub

        Public Function Execute(envir As Environment) As Object
            Dim last As Object = Nothing
            Dim breakLoop As Boolean = False

            For Each expression As Expression In execQueue
                last = ExecuteCodeLine(expression, envir, breakLoop, debug)

                If breakLoop Then
                    Exit For
                End If
            Next

            Return last
        End Function

        ''' <summary>
        ''' For execute lambda function
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <param name="envir"></param>
        ''' <param name="breakLoop"></param>
        ''' <returns></returns>
        Public Shared Function ExecuteCodeLine(expression As Expression, envir As Environment,
                                               Optional ByRef breakLoop As Boolean = False,
                                               Optional debug As Boolean = False) As Object

            Dim last = expression.Evaluate(envir)

            If debug Then
                Call Console.WriteLine(expression.ToString)
            End If
            If TypeOf expression Is ReturnValue Then
                ' return keyword will break the function
                last = New ReturnValue(New Literal With {.value = last})
            End If

            If Not last Is Nothing Then
                If last.GetType Is GetType(Message) Then
                    If DirectCast(last, Message).level = MSG_TYPES.ERR Then
                        ' throw error will break the expression loop
                        breakLoop = True
                        ' populate out this error message to the top stack
                        ' and then print errors
                        Return last
                    ElseIf DirectCast(last, Message).level = MSG_TYPES.DEBUG Then
                    ElseIf DirectCast(last, Message).level = MSG_TYPES.WRN Then
                    Else

                    End If
                ElseIf last.GetType Is GetType(IfBranch.IfPromise) Then
                    envir.ifPromise.Add(last)
                    last = DirectCast(last, IfBranch.IfPromise).Value

                    If envir.ifPromise.Last.Result Then
                        If Not last Is Nothing AndAlso last.GetType Is GetType(ReturnValue) Then
                            breakLoop = True
                        End If
                    End If
                End If
            End If

            Return last
        End Function

        Public Overrides Function ToString() As String
            Return execQueue.Select(Function(exp) exp.ToString & ";").JoinBy(vbCrLf)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function CreateProgram(tokens As IEnumerable(Of Token), Optional debug As Boolean = False) As Program
            Return New Program With {
                .debug = debug,
                .execQueue = tokens.ToArray _
                    .GetExpressions _
                    .ToArray
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function isException(result As Object) As Boolean
            If result Is Nothing Then
                Return False
            ElseIf result.GetType Is GetType(Message) Then
                Return DirectCast(result, Message).level = MSG_TYPES.ERR
            Else
                Return False
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function BuildProgram(scriptText As String) As Program
            Return New Scanner(scriptText) _
                .GetTokens _
                .DoCall(AddressOf CreateProgram)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Expression) Implements IEnumerable(Of Expression).GetEnumerator
            For Each line As Expression In execQueue
                Yield line
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace

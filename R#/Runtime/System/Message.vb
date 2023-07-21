#Region "Microsoft.VisualBasic::4044955e4980ef17ba66a09d6970c75a, D:/GCModeller/src/R-sharp/R#//Runtime/System/Message.vb"

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

    '   Total Lines: 142
    '    Code Lines: 85
    ' Comment Lines: 38
    '   Blank Lines: 19
    '     File Size: 5.55 KB


    '     Class Message
    ' 
    '         Properties: environmentStack, level, message, source, trace
    ' 
    '         Function: [Error], GenericEnumerator, GetEnumerator, InCompatibleType, SymbolNotFound
    '                   SyntaxNotImplemented, ToException, ToString
    ' 
    '         Sub: ThrowCLRError
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Components

    ''' <summary>
    ''' The warning message and exception message
    ''' </summary>
    Public Class Message : Implements Enumeration(Of String)

        ''' <summary>
        ''' the message content about this error or warning
        ''' </summary>
        ''' <returns></returns>
        Public Property message As String()
        ''' <summary>
        ''' the .NET log levels of current message object, value could be warning and error 
        ''' </summary>
        ''' <returns></returns>
        Public Property level As MSG_TYPES

        ''' <summary>
        ''' [R# runtime] the R# scripting environment runtime stack trace
        ''' </summary>
        ''' <returns></returns>
        Public Property environmentStack As StackFrame()

        ''' <summary>
        ''' [VB.NET runtime] the R# engine stack trace
        ''' </summary>
        ''' <returns></returns>
        Public Property trace As StackFrame()

        ''' <summary>
        ''' the source R# expression that cause this error or warning message
        ''' </summary>
        ''' <returns></returns>
        <SoapIgnore>
        <XmlIgnore>
        Public Property source As Expression

        ''' <summary>
        ''' Convert R# error message to VB.NET exception object
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToException() As RuntimeError
            Return New RuntimeError(Me)
        End Function

        ''' <summary>
        ''' throw the <see cref="Exception"/> data that cast from <see cref="ToException()"/>
        ''' </summary>
        Public Sub ThrowCLRError()
            Throw ToException()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [Error](msg As String, ParamArray stack As StackFrame()) As Message
            Return New Message With {
                .message = {msg},
                .environmentStack = stack,
                .level = MSG_TYPES.ERR,
                .source = Nothing,
                .trace = {}
            }
        End Function

        ''' <summary>
        ''' create a symbol not found error message
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="symbolName$"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function SymbolNotFound(envir As Environment, symbolName$, type As TypeCodes) As Message
            Dim exception$

            Select Case type
                Case TypeCodes.closure
                    exception = $"Not able to found any invokable symbol '{symbolName}' in environment stack: [{envir.ToString}]"
                Case Else
                    exception = $"Unable found symbol '{symbolName}' in environment stack: [{envir.ToString}]"
            End Select

            Dim messages As String() = {
                exception,
                "environment: " & envir.ToString,
                "symbol: " & symbolName
            }

            Return Internal.debug.stop(messages, envir)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function SyntaxNotImplemented(envir As Environment, operation$) As Message
            Return {
                "The specific syntax is not yet implemented...",
                "operation: " & operation
            }.DoCall(Function(msg)
                         Return Internal.debug.stop(msg, envir)
                     End Function)
        End Function

        Public Shared Function InCompatibleType(require As Type, given As Type, envir As Environment,
                                                Optional message$ = "The given type is incompatible with the required type!",
                                                Optional paramName$ = Nothing,
                                                Optional suppress As Boolean = False) As Message
            Return {
                message,
                "required: " & require.FullName,
                "given: " & given.FullName
            }.DoCall(Function(msg)
                         If Not paramName.StringEmpty Then
                             msg = msg.JoinIterates({$"parameter: {paramName}"})
                         End If

                         Return Internal.debug.stop(msg, envir, suppress)
                     End Function)
        End Function

        Public Overrides Function ToString() As String
            Return $"[{level.Description}] {message(Scan0)}"
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of String) Implements Enumeration(Of String).GenericEnumerator
            For Each msg As String In message.SafeQuery
                Yield msg
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of String).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace

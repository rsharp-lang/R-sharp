#Region "Microsoft.VisualBasic::78c93d7e275537022bdc955a107542a4, R#\Runtime\System\Message.vb"

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

    '     Class Message
    ' 
    '         Properties: environmentStack, level, message, source, trace
    ' 
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, InCompatibleType, SymbolNotFound, SyntaxNotImplemented
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Components

    ''' <summary>
    ''' The warning message and exception message
    ''' </summary>
    Public Class Message : Implements IEnumerable(Of String)

        Public Property message As String()
        Public Property level As MSG_TYPES
        Public Property environmentStack As StackFrame()
        Public Property trace As StackFrame()

        <SoapIgnore>
        <XmlIgnore>
        Public Property source As Expression

        Public Function ToException() As Exception

        End Function

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

        Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
            For Each msg As String In message
                Yield msg
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace

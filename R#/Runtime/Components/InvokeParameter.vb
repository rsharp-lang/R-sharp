#Region "Microsoft.VisualBasic::310d7c14a1502e1fa0a4a88a51d6eff5, R#\Runtime\Components\InvokeParameter.vb"

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

    '     Class InvokeParameter
    ' 
    '         Properties: haveSymbolName, name, value
    ' 
    '         Function: Create, CreateArguments, Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Components

    Public Class InvokeParameter

        Public Property value As Expression

        Public ReadOnly Property name As String
            Get
                If value Is Nothing Then
                    Return ""
                ElseIf TypeOf value Is SymbolReference Then
                    Return DirectCast(value, SymbolReference).symbol
                ElseIf TypeOf value Is ValueAssign Then
                    Return DirectCast(value, ValueAssign) _
                        .targetSymbols(Scan0) _
                        .DoCall(AddressOf ValueAssign.GetSymbol)
                Else
                    Return value.ToString
                End If
            End Get
        End Property

        Public ReadOnly Property haveSymbolName As Boolean
            Get
                If value Is Nothing Then
                    Return False
                ElseIf (TypeOf value Is SymbolReference OrElse TypeOf value Is ValueAssign) Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(envir As Environment) As Object
            If value Is Nothing Then
                Return Nothing
            ElseIf Not TypeOf value Is ValueAssign Then
                Return value.Evaluate(envir)
            Else
                Return DirectCast(value, ValueAssign).value.Evaluate(envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Create(expressions As IEnumerable(Of Expression)) As InvokeParameter()
            Return expressions _
                .Select(Function(e)
                            Return New InvokeParameter With {
                                .value = e
                            }
                        End Function) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateArguments(envir As Environment, arguments As IEnumerable(Of InvokeParameter)) As Dictionary(Of String, Object)
            Return arguments _
                .SeqIterator _
                .ToDictionary(Function(a)
                                  If a.value.haveSymbolName Then
                                      Return a.value.name
                                  Else
                                      Return "$" & a.i
                                  End If
                              End Function,
                              Function(a)
                                  Return a.value.Evaluate(envir)
                              End Function)
        End Function
    End Class
End Namespace

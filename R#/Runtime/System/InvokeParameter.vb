#Region "Microsoft.VisualBasic::866e8f8e51d07d083f3a5bd838ba1d5d, R#\Runtime\System\InvokeParameter.vb"

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
    '         Properties: haveSymbolName, index, isProbablyVectorNameTuple, isSymbolAssign, name
    '                     value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Create, CreateArguments, Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    Public Class InvokeParameter

        Public Property value As Expression

        Public ReadOnly Property index As Integer

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
                ElseIf TypeOf value Is VectorLiteral Then
                    With DirectCast(value, VectorLiteral)
                        If .length = 1 AndAlso TypeOf .First Is ValueAssign Then
                            ' [a = b] :> func(...)
                            Return DirectCast(.First, ValueAssign).targetSymbols(Scan0).ToString
                        Else
                            Return .ToString
                        End If
                    End With
                Else
                    Return value.ToString
                End If
            End Get
        End Property

        ''' <summary>
        ''' 主要是应用于生成list的参数列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isSymbolAssign As Boolean
            Get
                If value Is Nothing Then
                    Return False
                Else
                    Return TypeOf value Is ValueAssign
                End If
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="hasObjectList">
        ''' If allows hasObjectList, then <see cref="SymbolReference"/> will be used as list slot symbol name
        ''' otherwise only allows get symbol name from parameter name reference pattern ``a &lt;- b``.
        ''' </param>
        ''' <returns></returns>
        Public ReadOnly Property haveSymbolName(hasObjectList As Boolean) As Boolean
            Get
                If value Is Nothing Then
                    Return False
                ElseIf TypeOf value Is ValueAssign Then
                    Return True
                ElseIf hasObjectList AndAlso TypeOf value Is SymbolReference Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Public ReadOnly Property isProbablyVectorNameTuple As Boolean
            Get
                If TypeOf value Is VectorLiteral Then
                    Return DirectCast(value, VectorLiteral).type = TypeCodes.string
                Else
                    Return False
                End If
            End Get
        End Property

        Private Sub New()
        End Sub

        ''' <summary>
        ''' get value part
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
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
                .Select(Function(e, i)
                            Return New InvokeParameter With {
                                ._value = e,
                                ._index = i + 1
                            }
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="arguments"></param>
        ''' <param name="hasObjectList">
        ''' If has object list argument, then use the symbol name as slot name
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateArguments(envir As Environment, arguments As IEnumerable(Of InvokeParameter), hasObjectList As Boolean) As Dictionary(Of String, Object)
            Return arguments _
                .SeqIterator _
                .ToDictionary(Function(a)
                                  If a.value.haveSymbolName(hasObjectList) Then
                                      Return a.value.name
                                  Else
                                      Return "$" & a.i
                                  End If
                              End Function,
                              Function(a)
                                  Return a.value.Evaluate(envir)
                              End Function)
        End Function

        Public Shared Function Create(ParamArray args As Object()) As InvokeParameter()
            Return args _
                .Select(Function(a, i)
                            Return New InvokeParameter() With {
                                ._value = New RuntimeValueLiteral(a),
                                ._index = i + 1
                            }
                        End Function) _
                .ToArray
        End Function
    End Class
End Namespace

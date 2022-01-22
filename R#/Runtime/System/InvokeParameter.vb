#Region "Microsoft.VisualBasic::3ba9757020aaf163959a2ddc803fe971, R#\Runtime\System\InvokeParameter.vb"

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
'         Properties: haveSymbolName, index, isAcceptor, isProbablyVectorNameTuple, isSymbolAssign
'                     name, value
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Create, CreateArguments, CreateLiterals, Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
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
                ElseIf TypeOf value Is ValueAssignExpression Then
                    Return DirectCast(value, ValueAssignExpression) _
                        .targetSymbols(Scan0) _
                        .DoCall(AddressOf ValueAssignExpression.GetSymbol)
                ElseIf TypeOf value Is VectorLiteral Then
                    With DirectCast(value, VectorLiteral)
                        If .length = 1 AndAlso TypeOf .First Is ValueAssignExpression Then
                            ' [a = b] :> func(...)
                            Return DirectCast(.First, ValueAssignExpression).targetSymbols(Scan0).ToString
                        Else
                            Return .ToString
                        End If
                    End With
                Else
                    Return value.ToString
                End If
            End Get
        End Property

        Public ReadOnly Property isAcceptor As Boolean
            Get
                Return TypeOf value Is AcceptorClosure
            End Get
        End Property

        ''' <summary>
        ''' is syntax of ``a &lt;- b``?
        ''' 
        ''' (主要是应用于生成list的参数列表)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isSymbolAssign As Boolean
            Get
                If value Is Nothing Then
                    Return False
                Else
                    Return TypeOf value Is ValueAssignExpression
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
                ElseIf TypeOf value Is ValueAssignExpression Then
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

        Friend Sub New()
        End Sub

        Sub New(name As String, runtimeValue As Object, index As Integer)
            Me.index = index
            Me.value = New ValueAssignExpression({name}, New RuntimeValueLiteral(runtimeValue))
        End Sub

        Sub New(runtimeValue As Object, Optional index As Integer = Scan0)
            Me.index = index
            Me.value = New RuntimeValueLiteral(runtimeValue)
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
            ElseIf Not TypeOf value Is ValueAssignExpression Then
                Return value.Evaluate(envir)
            Else
                Return DirectCast(value, ValueAssignExpression).value.Evaluate(envir)
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
        ''' <param name="env"></param>
        ''' <param name="arguments"></param>
        ''' <param name="hasObjectList">
        ''' If has object list argument, then use the symbol name as slot name
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateArguments(env As Environment,
                                               arguments As IEnumerable(Of InvokeParameter),
                                               hasObjectList As Boolean) As Dictionary(Of String, InvokeParameter)

            Dim argVals As New Dictionary(Of String, InvokeParameter)
            Dim allArgs As InvokeParameter() = arguments.ToArray
            Dim acceptor As AcceptorClosure = Nothing

            If allArgs.Length > 0 AndAlso allArgs(Scan0).isAcceptor Then
                acceptor = allArgs(Scan0).value
                allArgs = allArgs _
                    .Skip(1) _
                    .ToArray
            End If

            For Each arg As SeqValue(Of InvokeParameter) In allArgs.SeqIterator(offset:=If(acceptor Is Nothing, 0, 1))
                Dim keyName As String
                Dim argVal As InvokeParameter

                If arg.value.haveSymbolName(hasObjectList) Then
                    keyName = arg.value.name
                Else
                    keyName = "$" & arg.i
                End If

                argVal = arg.value ' .Evaluate(env)

                'If Program.isException(argVal) Then
                '    Return DirectCast(argVal, Message)
                'Else
                If Not argVals.ContainsKey(keyName) Then
                    Call argVals.Add(keyName, argVal)
                End If

                If Not acceptor Is Nothing Then
                    If arg.value.isSymbolAssign Then
                        Call env.acceptorArguments.Add(keyName, argVal)
                    End If
                End If
                ' End If
            Next

            If acceptor Is Nothing Then
                Return argVals
            End If

            Dim newAlignArgs As New Dictionary(Of String, InvokeParameter) From {
                {"$0", New InvokeParameter() With {.value = acceptor}}' .Evaluate(env)}
            }

            For Each keyName As String In argVals.Keys
                newAlignArgs.Add(keyName, argVals(keyName))
            Next

            Return newAlignArgs
        End Function

        ''' <summary>
        ''' create argument by runtime literal values.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Shared Function CreateLiterals(ParamArray args As Object()) As InvokeParameter()
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

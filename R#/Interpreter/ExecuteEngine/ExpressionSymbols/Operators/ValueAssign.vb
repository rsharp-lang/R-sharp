#Region "Microsoft.VisualBasic::193012cf416bc3201086330e220aa509, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\ValueAssign.vb"

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

    '     Class ValueAssign
    ' 
    '         Properties: symbolSize, type
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: assignSymbol, assignTuples, doValueAssign, DoValueAssign, Evaluate
    '                   GetSymbol, setFromObjectList, setFromVector, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' Set variable or tuple
    ''' </summary>
    Public Class ValueAssign : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return value.type
            End Get
        End Property

        ''' <summary>
        ''' 可能是对tuple做赋值
        ''' 所以应该是多个变量名称
        ''' </summary>
        Friend targetSymbols As Expression()
        Friend isByRef As Boolean
        Friend value As Expression

        Public ReadOnly Property symbolSize As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return targetSymbols.Length
            End Get
        End Property

        Sub New(tokens As List(Of Token()))
            targetSymbols = DeclareNewVariable _
                .getNames(tokens(Scan0)) _
                .Select(Function(name) New Literal(name)) _
                .ToArray
            isByRef = tokens(Scan0)(Scan0).text = "="
            value = tokens.Skip(2).AsList.DoCall(AddressOf Expression.ParseExpression)
        End Sub

        Sub New(targetSymbols$(), value As Expression)
            Me.targetSymbols = targetSymbols _
                .Select(Function(name) New Literal(name)) _
                .ToArray
            Me.value = value
        End Sub

        Sub New(target As Expression(), value As Expression)
            Me.targetSymbols = target
            Me.value = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return DoValueAssign(envir, value.Evaluate(envir))
        End Function

        Public Function DoValueAssign(envir As Environment, value As Object) As Object
            If Not value Is Nothing AndAlso value.GetType Is GetType(IfBranch.IfPromise) Then
                DirectCast(value, IfBranch.IfPromise).assignTo = Me
                Return value
            Else
                Return doValueAssign(envir, targetSymbols, isByRef, value)
            End If
        End Function

        Friend Shared Function doValueAssign(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Object
            Dim message As Message

            If targetSymbols.Length = 1 Then
                message = assignSymbol(envir, targetSymbols(Scan0), isByRef, value)
            Else
                ' assign tuples
                message = assignTuples(envir, targetSymbols, isByRef, value)
            End If

            If message Is Nothing Then
                Return value
            Else
                Return message
            End If
        End Function

        Public Overrides Function ToString() As String
            If symbolSize = 1 Then
                Return $"{targetSymbols(0)} <- {value.ToString}"
            Else
                Return $"[{targetSymbols.JoinBy(", ")}] <- {value.ToString}"
            End If
        End Function

        Private Shared Function setFromVector(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Object
            Dim message As New Value(Of Message)
            Dim array As Array

            If value.GetType Is GetType(vector) Then
                array = DirectCast(value, vector).data
            Else
                array = value
            End If

            If array.Length = 1 Then
                ' all assign the same value result
                For Each name As Expression In targetSymbols
                    If Not (message = assignSymbol(envir, name, isByRef, value)) Is Nothing Then
                        Return message
                    End If
                Next
            ElseIf array.Length = targetSymbols.Length Then
                ' one by one
                For i As Integer = 0 To array.Length - 1
                    If Not (message = assignSymbol(envir, targetSymbols(i), isByRef, array.GetValue(i))) Is Nothing Then
                        Return message
                    End If
                Next
            Else
                ' 数量不对
                Return Internal.stop(New InvalidCastException, envir)
            End If

            Return Nothing
        End Function

        Private Shared Function setFromObjectList(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object)
            Dim list As list = value
            Dim message As New Value(Of Message)

            If list.length = 1 Then
                ' 设置tuple的值的时候
                ' list必须要有相同的元素数量
                Return Internal.stop("Number of list element is not identical to the tuple elements...", envir)
            ElseIf list.length = targetSymbols.Length Then
                Dim name$

                ' one by one
                For i As Integer = 0 To list.length - 1
                    name = GetSymbol(targetSymbols(i))

                    If list.slots.ContainsKey(name) Then
                        value = list.slots(name)
                    Else
                        ' R中的元素下标都是从1开始的
                        value = list.slots($"{i + 1}")
                    End If

                    If Not (message = assignSymbol(envir, targetSymbols(i), isByRef, value)) Is Nothing Then
                        Return message
                    End If
                Next
            Else
                ' 数量不对
                Return Internal.stop(New InvalidCastException, envir)
            End If

            Return Nothing
        End Function

        Private Shared Function assignTuples(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Message
            Dim message As New Value(Of Message)
            Dim type As Type

            If value Is Nothing Then
                ' all assign the same value result
                ' literal NULL
                For Each name As Expression In targetSymbols
                    If Not (message = assignSymbol(envir, name, isByRef, value)) Is Nothing Then
                        Return message
                    End If
                Next

                Return Nothing
            Else
                type = value.GetType
            End If

            If type.IsInheritsFrom(GetType(Array)) OrElse type Is GetType(vector) Then
                Return setFromVector(envir, targetSymbols, isByRef, value)

            ElseIf type Is GetType(list) Then
                Return setFromObjectList(envir, targetSymbols, isByRef, value)

            Else
                Return Internal.stop(New NotImplementedException, envir)
            End If

            Return Nothing
        End Function

        Friend Shared Function GetSymbol(symbolName As Expression) As String
            Select Case symbolName.GetType
                Case GetType(Literal)
                    Return DirectCast(symbolName, Literal).value
                Case GetType(SymbolReference)
                    Return DirectCast(symbolName, SymbolReference).symbol
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Shared Function assignSymbol(envir As Environment, symbolName As Expression, isByRef As Boolean, value As Object) As Message
            Dim target As Variable = Nothing

            Select Case symbolName.GetType
                Case GetType(Literal)
                    target = envir.FindSymbol(DirectCast(symbolName, Literal).value)
                Case GetType(SymbolReference)
                    target = envir.FindSymbol(DirectCast(symbolName, SymbolReference).symbol)
                Case GetType(SymbolIndexer)
                    Throw New NotImplementedException
                Case Else
                    Throw New InvalidExpressionException
            End Select

            If target Is Nothing Then
                Return Message.SymbolNotFound(envir, symbolName.ToString, TypeCodes.generic)
            End If

            If isByRef Then
                target.value = value
            Else
                If Not value Is Nothing AndAlso value.GetType.IsInheritsFrom(GetType(Array)) Then
                    target.value = DirectCast(value, Array).Clone
                Else
                    target.value = value
                End If
            End If

            Return Nothing
        End Function
    End Class
End Namespace

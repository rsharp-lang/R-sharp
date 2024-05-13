#Region "Microsoft.VisualBasic::b116f0d17f8f47658c41697c5b197307, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\ValueAssignOperator\ValueAssignExpression.vb"

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

    '   Total Lines: 363
    '    Code Lines: 265
    ' Comment Lines: 44
    '   Blank Lines: 54
    '     File Size: 15.26 KB


    '     Class ValueAssignExpression
    ' 
    '         Properties: expressionName, symbolSize, targetSymbols, type, value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: assignSymbol, assignTuples, doValueAssign, DoValueAssign, Evaluate
    '                   GetSymbol, getSymbols, setFromDataFrame, setFromObjectList, setFromVector
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop.CType
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' Set variable or tuple
    ''' </summary>
    Public Class ValueAssignExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return value.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolAssign
            End Get
        End Property

        ''' <summary>
        ''' 可能是对tuple做赋值
        ''' 所以应该是多个变量名称
        ''' </summary>
        Public ReadOnly Property targetSymbols As Expression()
        Public ReadOnly Property value As Expression

        Friend isByRef As Boolean

        Public ReadOnly Property symbolSize As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return targetSymbols.Length
            End Get
        End Property

        Sub New(symbol As String, value As Expression)
            Call Me.New({symbol}, value)
        End Sub

        Sub New(targetSymbols$(), value As Expression)
            Call Me.New(getSymbols(targetSymbols), value)
        End Sub

        Sub New(target As Expression(), value As Expression)
            Me.targetSymbols = target
            Me.value = value

            If targetSymbols.Length = 1 AndAlso TypeOf value Is DeclareNewFunction Then
                Dim newFunc As DeclareNewFunction = DirectCast(value, DeclareNewFunction)

                If newFunc.funcName.StartsWith("<$anonymous_") Then
                    newFunc.SetSymbol($"{targetSymbols(Scan0).ToString.Trim(""""c)}{newFunc.funcName}")
                    newFunc.stackFrame.Method.Method = newFunc.funcName
                End If
            End If
        End Sub

        Private Shared Function getSymbols(targetSymbols$()) As Expression()
            Return targetSymbols _
                .Select(Function(name) New Literal(name)) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return DoValueAssign(envir, value.Evaluate(envir))
        End Function

        Public Function DoValueAssign(envir As Environment, value As Object) As Object
            If TypeOf value Is IfPromise Then
                ' 如果是if分支返回的结果，则将if分支的赋值对象设置为
                ' 当前的赋值操作的目标对象符号
                DirectCast(value, IfPromise).assignTo = Me
                Return value
            ElseIf Program.isException(value) Then
                Return value
            Else
                If Not value Is Nothing AndAlso TypeOf value Is invisible Then
                    ' value assign will break the invisible
                    value = DirectCast(value, invisible).value
                End If

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

        Private Shared Function setFromVector(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Message
            Dim message As New Value(Of Message)
            Dim array As Array

            If value.GetType Is GetType(vector) Then
                array = DirectCast(value, vector).data
            Else
                array = DirectCast(value, Array)
            End If

            If array.Length = 1 Then
                ' all assign the same value result
                For Each name As Expression In targetSymbols
                    If Not (message = assignSymbol(envir, name, isByRef, value)) Is Nothing Then
                        Return message.Value
                    End If
                Next
            ElseIf array.Length = targetSymbols.Length Then
                ' one by one
                For i As Integer = 0 To array.Length - 1
                    If Not (message = assignSymbol(envir, targetSymbols(i), isByRef, array.GetValue(i))) Is Nothing Then
                        Return message.Value
                    End If
                Next
            Else
                ' 数量不对
                Return Internal.debug.stop(New InvalidCastException, envir)
            End If

            Return Nothing
        End Function

        Public Shared Function setFromDataFrame(env As Environment, targetSymbols As Expression(), isbyRef As Boolean, value As Object) As Message
            Dim data As dataframe = DirectCast(value, dataframe)
            Dim message As New Value(Of Message)

            If data.ncols < targetSymbols.Length Then
                ' 设置tuple的值的时候
                ' dataframe必须要有相同的列数量
                Return Internal.debug.stop("Number of dataframe column element is not identical to the tuple elements...", env)
            Else
                Dim name As String

                ' one by one
                For Each symbol As Expression In targetSymbols
                    name = GetSymbol(symbol)

                    If data.hasName(name) Then
                        value = data.getColumnVector(name)
                    Else
                        Return Internal.debug.stop({
                            $"missing symbol name '{name}' in your dataframe!",
                            $"symbol: {name}",
                            $"columns: {data.colnames.JoinBy(", ")}"
                        }, env)
                    End If

                    If Not (message = assignSymbol(env, symbol, isbyRef, value)) Is Nothing Then
                        Return message.Value
                    End If
                Next
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 使用左边的变量符号的名称从右边的列表之中按名称取出值，不存在的名字则赋值为空值
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="targetSymbols"></param>
        ''' <param name="isByRef"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Private Shared Function setFromObjectList(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Message
            Dim list As list = DirectCast(value, list)
            Dim message As New Value(Of Message)

            If list.length = 1 Then
                ' 设置tuple的值的时候
                ' list必须要有相同的元素数量
                Return Internal.debug.stop("Number of list element is not identical to the tuple elements...", envir)
            Else
                ' one by one
                For Each symbol As Expression In targetSymbols
                    Dim name As String = GetSymbol(symbol)

                    If list.slots.ContainsKey(name) Then
                        value = list.slots(name)
                    Else
                        value = Nothing
                        Call envir.AddMessage({$"target symbol '{name}' is not exists in tuple!"}, MSG_TYPES.WRN)
                    End If

                    If Not (message = assignSymbol(envir, symbol, isByRef, value)) Is Nothing Then
                        Return message.Value
                    End If
                Next
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
                        Return message.Value
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

            ElseIf type Is GetType(dataframe) Then
                Return setFromDataFrame(envir, targetSymbols, isByRef, value)

            ElseIf type.ImplementInterface(Of ITupleConstructor) Then

                Throw New NotImplementedException
            Else
                Return Internal.debug.stop(New NotImplementedException, envir)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' handling get symbol from the given expression types:
        ''' 
        ''' 1. <see cref="Literal"/>: get string literal value as symbol text
        ''' 2. <see cref="SymbolReference"/>: get symbol name as the symbol text
        ''' 
        ''' </summary>
        ''' <param name="symbolName"></param>
        ''' <returns>
        ''' this function returns nothing if the given expression type 
        ''' is not implements for get symbol text
        ''' </returns>
        Public Shared Function GetSymbol(symbolName As Expression) As String
            Select Case symbolName.GetType
                Case GetType(Literal)
                    With DirectCast(symbolName, Literal)
                        If .type = TypeCodes.string Then
                            Return CStr(.value)
                        Else
                            ' other literal value, example like number,
                            ' logical, is not a name
                            Return Nothing
                        End If
                    End With
                Case GetType(SymbolReference)
                    Return DirectCast(symbolName, SymbolReference).symbol
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Shared Function assignSymbol(envir As Environment, symbolName As Expression, isByRef As Boolean, value As Object) As Message
            Dim target As Symbol = Nothing

            Select Case symbolName.GetType
                Case GetType(Literal)
                    Dim symbol As String = any.ToString(DirectCast(symbolName, Literal).value)

                    Select Case symbol
                        Case "NA", "NULL", "TRUE", "FALSE", "NaN"
                            Return Internal.debug.stop({
                                "invalid (do_set) left-hand side to assignment",
                                "constant symbol is not allowed!",
                                "symbol: " & symbol
                            }, envir)
                        Case Else
                            If Not symbol.IsPattern(Scanner.RSymbol) Then
                                Return Internal.debug.stop({
                                    "invalid (do_set) left-hand side to assignment",
                                    "constant literal is not allowed!",
                                    "literal: " & symbol
                                }, envir)
                            End If
                    End Select

                    target = envir.FindSymbol(symbol)
                Case GetType(SymbolReference)
                    target = envir.FindSymbol(DirectCast(symbolName, SymbolReference).symbol)
                Case GetType(SymbolIndexer)
                    Return MemberValueAssign.setByNameIndex(symbolName, envir, value)
                Case Else
                    Return Internal.debug.stop(New InvalidExpressionException, envir)
            End Select

            If target Is Nothing Then
                If Not envir.globalEnvironment.Rscript.strict Then
                    Dim name As String

                    If TypeOf symbolName Is Literal Then
                        name = DirectCast(symbolName, Literal).value.ToString
                    ElseIf TypeOf symbolName Is SymbolReference Then
                        name = DirectCast(symbolName, SymbolReference).symbol
                    Else
                        Return Message.SymbolNotFound(envir, symbolName.ToString, TypeCodes.generic)
                    End If

                    Call envir.Push(name, value, [readonly]:=False)

                    Return Nothing
                Else
                    Return Message.SymbolNotFound(envir, symbolName.ToString, TypeCodes.generic)
                End If
            End If

            If isByRef Then
                Return target.setValue(value, envir)
            Else
                If Not value Is Nothing AndAlso value.GetType.IsInheritsFrom(GetType(Array)) Then
                    Return target.setValue(DirectCast(value, Array).Clone, envir)
                Else
                    Return target.setValue(value, envir)
                End If
            End If
        End Function
    End Class
End Namespace

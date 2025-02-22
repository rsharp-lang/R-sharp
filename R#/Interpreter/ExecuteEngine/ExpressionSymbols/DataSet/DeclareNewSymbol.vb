﻿#Region "Microsoft.VisualBasic::3f41c39cedc72cd3ddb2e1296af683fc, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\DeclareNewSymbol.vb"

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

    '   Total Lines: 448
    '    Code Lines: 295 (65.85%)
    ' Comment Lines: 92 (20.54%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 61 (13.62%)
    '     File Size: 16.87 KB


    '     Class DeclareNewSymbol
    ' 
    '         Properties: expressionName, hasInitializeExpression, isTuple, names, stackFrame
    '                     symbolSize, type, unit, value
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: Evaluate, getParameterView, GetSymbolName, PushNames, PushTuple
    '                   ToString
    ' 
    '         Sub: AddCustomAttributes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' An expressin for create a new symbol inside the runtime environment
    ''' </summary>
    Public Class DeclareNewSymbol : Inherits SymbolExpression
        Implements IRuntimeTrace

        ''' <summary>
        ''' tuple names.
        ''' (对于tuple语法，会存在多个变量)
        ''' </summary>
        Public ReadOnly Property names As IReadOnlyCollection(Of String)
            Get
                Return m_names.ToArray
            End Get
        End Property

        ''' <summary>
        ''' true for if the <see cref="value"/> expression is not nothing
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property hasInitializeExpression As Boolean = False

        Friend m_names As String()
        Friend m_value As Expression
        Friend m_type As TypeCodes
        Friend is_readonly As Boolean

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolDeclare
            End Get
        End Property

        ''' <summary>
        ''' get name by index
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property getName(i As Integer) As String
            Get
                If m_names.TryCount <= i Then
                    Return Nothing
                Else
                    Return m_names(i)
                End If
            End Get
        End Property

        ''' <summary>
        ''' get object value type for current new symbol
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property type As TypeCodes
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                ' if the type constrain is nothing
                ' then use the default value type if 
                ' has the default value

                ' 20230616 use the data type comes from the default value
                ' will case the function parameter un-wanted type cast error
                ' removes such type extract code 
                '
                ' DO NOT USE THE TYPE FROM THE DEFAULT VALUE EXPRESSION
                '
                'If m_type = TypeCodes.generic Then
                '    If Not value Is Nothing Then
                '        Return value.type
                '    End If
                'End If

                Return m_type
            End Get
        End Property

        ''' <summary>
        ''' the size of the symbol names
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property symbolSize As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_names.Length
            End Get
        End Property

        ''' <summary>
        ''' does current symbol declaration contains multiple symbol targets?
        ''' </summary>
        ''' <returns>
        ''' TRUE means contains multiple symbol target, it is a new tuple syntax;
        ''' FALSE means just has one symbol target to create in the context.
        ''' </returns>
        Public ReadOnly Property isTuple As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return (Not names.IsNullOrEmpty) AndAlso symbolSize > 1
            End Get
        End Property

        ''' <summary>
        ''' The symbol initial value.(初始值)
        ''' </summary>
        Public ReadOnly Property value As Expression
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_value
            End Get
        End Property

        ''' <summary>
        ''' the unit name of the symbol value if it is a vector
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property unit As String
            Get
                Return GetAttributeValue("unit").FirstOrDefault
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        ''' <summary>
        ''' create new symbol with a specific initial value
        ''' </summary>
        ''' <param name="assign"></param>
        ''' <param name="stack"></param>
        Sub New(assign As ValueAssignExpression, stack As StackFrame)
            Call Me.New(
                names:=assign.targetSymbols _
                    .Select(Function(xi) ValueAssignExpression.GetSymbol(xi)) _
                    .ToArray,
                value:=assign.value,
                type:=assign.value.type,
                [readonly]:=False,
                stackFrame:=stack
            )
        End Sub

        ''' <summary>
        ''' create new symbol without initial value
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="stack"></param>
        Sub New(symbol As SymbolReference, stack As StackFrame)
            Call Me.New(
                names:={symbol.symbol},
                value:=Nothing,
                type:=TypeCodes.generic,
                [readonly]:=False,
                stackFrame:=stack
            )
        End Sub

        Sub New(names$(), value As Expression, type As TypeCodes, [readonly] As Boolean, stackFrame As StackFrame)
            Me.m_names = names.ToArray
            Me.m_value = value
            Me.m_type = type
            Me.is_readonly = [readonly]
            Me.stackFrame = stackFrame

            If Not value Is Nothing Then
                hasInitializeExpression = True
            End If
        End Sub

        Sub New(symbol As String, stackFrame As StackFrame,
                Optional value As Expression = Nothing,
                Optional type As TypeCodes = TypeCodes.generic,
                Optional [readonly] As Boolean = False)

            Call Me.New({symbol}, value, type, [readonly], stackFrame)
        End Sub

        ''' <summary>
        ''' get symbol name string
        ''' </summary>
        ''' <returns>
        ''' usually be the first element of the names array. For multiple names, 
        ''' a string json array text will be generates from this function.
        ''' </returns>
        Public Overrides Function GetSymbolName() As String
            If m_names.Length = 1 Then
                Return m_names(0)
            Else
                Return names.ToArray.GetJson
            End If
        End Function

        Protected Friend Overrides Sub AddCustomAttributes(attrs As IEnumerable(Of NamedValue(Of String())))
            Call MyBase.AddCustomAttributes(attrs)

            If value Is Nothing Then
                Return
            End If

            Dim argumentType As ArgumentGetters = CommandLineSyntax.IsArgumentGetter(assert:=value)

            If argumentType <> ArgumentGetters.FALSE AndAlso hasAttribute("config") Then
                Dim argument As ArgumentValue
                Dim config As String = attributes("config").First

                Select Case argumentType
                    Case ArgumentGetters.GetArgument
                        argument = value
                    Case Else
                        argument = DirectCast(value, BinaryOrExpression).left
                End Select

                Call argument.SetConfigName(name:=config)
            End If
        End Sub

        ''' <summary>
        ''' Push current variable into the target environment ``<paramref name="envir"/>``.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object
            Dim type As TypeCodes = Me.type

            If Me.value Is Nothing Then
                value = Nothing
            Else
                value = Me.value.Evaluate(envir)
            End If

            If Program.isException(value) Then
                Return value
            ElseIf TypeOf value Is IfPromise Then
                ' 20220623 try to make bugs fixed of the if else branch
                ' if else promise value will cause the tuple syntax failured
                value = DirectCast(value, IfPromise).Value
            End If

            If type = TypeCodes.boolean AndAlso TypeOf value Is String Then
                value = DirectCast(value, String).ParseBoolean
            End If

            If Not value Is Nothing Then
                If (Not value.GetType.IsArray) AndAlso (Not TypeOf value Is vector) Then
                    If type = TypeCodes.generic Then
                        type = value.GetType.GetRTypeCode
                    End If

                    If type = TypeCodes.boolean OrElse
                        type = TypeCodes.double OrElse
                        type = TypeCodes.integer OrElse
                        type = TypeCodes.string Then

                        value = REnv.TryCastGenericArray({value}, envir)
                    End If
                End If

                If value.GetType.IsArray Then
                    If hasAttribute("unit") Then
                        value = New vector(value, RType.GetType(type)) With {
                            .unit = New unit With {.name = unit}
                        }
                    End If
                End If
            End If

            Try
                Dim err As Message = Nothing
                ' add new symbol into the given environment stack
                ' and then returns the value result
                Call PushNames(names, value, type, is_readonly, envir, err:=err)

                If Not err Is Nothing Then
                    value = err
                End If
            Catch ex As Exception
                value = Internal.debug.stop({
                    ex.Message,
                    "symbols: " & names.JoinBy(","),
                    "value: " & If(value Is Nothing, GetType(Void), value.GetType).FullName,
                    "required: " & type.Description,
                    "is_readonly: " & is_readonly
                }, envir)
            End Try

            Return value
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="names">The variable names</param>
        ''' <param name="value">.NET runtime object value.</param>
        ''' <param name="type"></param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' 用于初始化环境对象
        ''' </returns>
        Friend Shared Function PushNames(names$(),
                                         value As Object,
                                         type As TypeCodes,
                                         [readonly] As Boolean,
                                         envir As Environment,
                                         ByRef err As Message) As Environment

            If Not value Is Nothing AndAlso TypeOf value Is invisible Then
                value = DirectCast(value, invisible).value
            End If

            If names.Length = 1 Then
                value = envir.Push(names(Scan0), value, [readonly], type)
            Else
                ' tuple
                value = PushTuple(names, value, type, [readonly], envir)
            End If

            If Program.isException(value) Then
                err = value
            Else
                err = Nothing
            End If

            Return envir
        End Function

        Private Shared Function PushTuple(names$(),
                                          value As Object,
                                          type As TypeCodes,
                                          [readonly] As Boolean,
                                          envir As Environment) As Object
            Dim rtvl As Object

            If TypeOf value Is IfPromise Then
                value = DirectCast(value, IfPromise).Value
            End If

            If Not value Is Nothing AndAlso TypeOf value Is vector Then
                value = DirectCast(value, vector).data
            End If

            If Not value Is Nothing AndAlso value.GetType.IsArray Then
                Dim vector As Array = value

                If vector.Length = 1 Then
                    ' all set with one value
                    For Each name As String In names
                        rtvl = envir.Push(name, value, [readonly])

                        If Program.isException(rtvl) Then
                            Return rtvl
                        End If
                    Next
                ElseIf vector.Length = names.Length Then
                    ' declare one by one
                    For i As Integer = 0 To vector.Length - 1
                        rtvl = envir.Push(names(i), vector.GetValue(i), [readonly])

                        If Program.isException(rtvl) Then
                            Return rtvl
                        End If
                    Next
                Else
                    Return Internal.debug.stop({
                        $"the value length is not equals to the tuple names!",
                        $"tuple names: [{names.Length}] {names.GetJson}",
                        $"value size: {vector.Length}"
                    }, envir)
                End If
            ElseIf Not value Is Nothing AndAlso TypeOf value Is dataframe Then
                Dim data As dataframe = DirectCast(value, dataframe)

                For Each name As String In names
                    rtvl = envir.Push(name, data.getColumnVector(name), [readonly])

                    If Program.isException(rtvl) Then
                        Return rtvl
                    End If
                Next
            ElseIf TypeOf value Is list Then
                Dim list As list = DirectCast(value, list)

                For Each name As String In names
                    rtvl = envir.Push(name, list.getByName(name), [readonly])

                    If Program.isException(rtvl) Then
                        Return rtvl
                    End If
                Next
            Else
                ' all set with one value
                For Each name As String In names
                    rtvl = envir.Push(name, value, [readonly])

                    If Program.isException(rtvl) Then
                        Return rtvl
                    End If
                Next
            End If

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            If symbolSize > 1 Then
                Return $"let [{names.JoinBy(", ")}] as {type.Description} = {If(value Is Nothing, "NULL", value.Indent).Trim};"
            ElseIf symbolSize = 0 Then
                Return "# syntax_error!!!"
            Else
                Return $"let {names(Scan0)} as {type.Description} = {If(value Is Nothing, "NULL", value.Indent).Trim}"
            End If
        End Function

        Friend Shared Function getParameterView(declares As DeclareNewSymbol) As String
            Dim a$

            If declares.symbolSize = 1 Then
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

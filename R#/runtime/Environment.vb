#Region "Microsoft.VisualBasic::4dab7ebab2296f80fa34a6e23718add1, ..\R-sharp\R#\runtime\Environment.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Abstract
Imports SMRUCC.Rsharp.Interpreter.Expression
Imports SMRUCC.Rsharp.Runtime.CodeDOM
Imports SMRUCC.Rsharp.Runtime.PrimitiveTypes

Namespace Runtime

    ''' <summary>
    ''' 某一个closure之中的变量环境
    ''' </summary>
    Public Class Environment

        Public ReadOnly Property Variables As Dictionary(Of Variable)
        ''' <summary>
        ''' 最顶层的closure环境的parent是空值来的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Parent As Environment
        Public ReadOnly Property Stack As String

        ''' <summary>
        ''' Key is <see cref="RType.Identity"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Types As New Dictionary(Of String, RType)
        Public ReadOnly Property PrimitiveTypes As New Dictionary(Of TypeCodes, RType)

        ''' <summary>
        ''' 函数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Closures As New Dictionary(Of String, Func(Of NamedValue(Of Object)(), Object))

        ''' <summary>
        ''' 当前的环境是否为最顶层的全局环境？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsGlobal As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Parent Is Nothing
            End Get
        End Property

        Public ReadOnly Property GlobalEnvironment As Environment
            Get
                If IsGlobal Then
                    Return Me
                Else
                    Return Parent.GlobalEnvironment
                End If
            End Get
        End Property

        ''' <summary>
        ''' Get/set variable value
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' If the current stack does not contains the target variable, then the program will try to find the variable in his parent
        ''' if variable in format like [var], then it means a global or parent environment variable
        ''' </remarks>
        Default Public Property Value(name$) As Variable
            Get
                If (name.First = "["c AndAlso name.Last = "]"c) Then
                    Return GlobalEnvironment(name.GetStackValue("[", "]"))
                End If

                If Variables.ContainsKey(name) Then
                    Return Variables(name)
                ElseIf Not Parent Is Nothing Then
                    Return Parent(name)
                Else
                    Throw New EntryPointNotFoundException(name & " was not found in any stack enviroment!")
                End If
            End Get
            Set(value As Variable)
                If name.First = "["c AndAlso name.Last = "]"c Then
                    GlobalEnvironment(name.GetStackValue("[", "]")) = value
                Else
                    Variables(name) = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' 每一个closure可以看作为一个函数对象
        ''' 则parent的closure和他的child closure之间相互通信，最直接的方法就是参数传递
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="parameters">closure函数的参数传递列表</param>
        ''' <param name="stack">每一个函数内部都会有自己的局部stack堆栈环境，这个参数就是相当于函数的名称</param>
        Sub New(parent As Environment, parameters As NamedValue(Of PrimitiveExpression)(), <CallerMemberName> Optional stack$ = Nothing)
            Me.Parent = parent
            Me.Variables = parameters _
                .Select(Function(expression)
                            Dim expr As PrimitiveExpression = expression.Value
                            Return New Variable(TypeCodes.generic) With {
                                .Name = expression.Name,
                                .Value = expr.Evaluate(envir:=Me)
                            }
                        End Function) _
                .ToDictionary
            Me.Stack = stack

            ' imports PrimitiveTypes
            PrimitiveTypes(TypeCodes.char) = New PrimitiveTypes.character
            PrimitiveTypes(TypeCodes.integer) = New PrimitiveTypes.integer
            PrimitiveTypes(TypeCodes.boolean) = New PrimitiveTypes.logical
            PrimitiveTypes(TypeCodes.double) = New PrimitiveTypes.numeric
            PrimitiveTypes(TypeCodes.string) = New PrimitiveTypes.string
            PrimitiveTypes(TypeCodes.uinteger) = New PrimitiveTypes.uinteger
            PrimitiveTypes(TypeCodes.list) = New PrimitiveTypes.list

            Call [Imports](PrimitiveTypes(TypeCodes.boolean))
            Call [Imports](PrimitiveTypes(TypeCodes.char))
            Call [Imports](PrimitiveTypes(TypeCodes.double))
            Call [Imports](PrimitiveTypes(TypeCodes.integer))
            Call [Imports](PrimitiveTypes(TypeCodes.string))
            Call [Imports](PrimitiveTypes(TypeCodes.uinteger))
            Call [Imports](PrimitiveTypes(TypeCodes.list))
        End Sub

        ''' <summary>
        ''' Construct the global environment
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub New()
            Call Me.New(Nothing, {})
        End Sub

        Public Overrides Function ToString() As String
            If IsGlobal Then
                Return $"Global({NameOf(Environment)})"
            Else
                Return Parent?.ToString & "->" & Stack
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetValue() As GetValue
            Return Function(name$)
                       Return Me(name)
                   End Function
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate() As FunctionEvaluate
            Return Function(funcName, args)
                       Throw New NotImplementedException
                   End Function
        End Function

        Public Function GetValue(x As MetaExpression) As Variable
            If x.LeftType = TypeCodes.ref Then
                Dim o = x.LEFT

                If o Is Nothing Then
                    Return Nothing

                ElseIf o.GetType Is GetType(String) Then

                    ' 为变量引用
                    Dim ref$ = CStr(o)
                    Dim var As Variable = Me(ref)

                    Return var
                Else

                    Throw New InvalidExpressionException
                End If
            Else

                Dim value = x.LEFT

                If value IsNot Nothing AndAlso value.GetType Is Core.TypeDefine(Of TempValue).GetSingleType Then
                    Dim temp = DirectCast(value, TempValue)
                    Return New Variable(temp.type) With {
                        .Name = App.NextTempName,
                        .Value = temp.value
                    }
                Else
                    ' temp variable
                    Return New Variable(x.LeftType) With {
                        .Name = App.NextTempName,
                        .Value = value
                    }
                End If
            End If
        End Function

        ''' <summary>
        ''' Add new variable into current stack environment.
        ''' 
        ''' ```
        ''' var x as type &amp;- &lt;expression>
        ''' ```
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="value"></param>
        ''' <param name="type">``R#``类型约束</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Push(name$, value As Object, type$) As Integer
            Return Push(name, value, type.GetRTypeCode)
        End Function

        Const ConstraintInvalid$ = "Value can not match the type constraint!!! ({0} <--> {1})"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub [Imports](type As RType)
            Types(type.Identity) = type
        End Sub

        Public Function RType(var As Variable) As RType
            If var.TypeCode.IsPrimitive Then
                ' all of the primitive type in R# language is vector type
                Dim base As Type = var.TypeCode.DotNETtype
                Return Types(base.FullName)
            Else
                Return Types(var.TypeID)
            End If
        End Function

        Const AlreadyExists$ = "Variable ""{0}"" is already existed, can not declare it again!"

        ''' <summary>
        ''' Variable declare
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="value"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function Push(name$, value As Object, Optional type As TypeCodes = TypeCodes.generic) As Integer
            If Variables.ContainsKey(name) Then
                Throw New Exception(String.Format(AlreadyExists, name))
            End If

            With New Variable(type) With {
                .Name = name,
                .Value = value
            }
                If Not .ConstraintValid Then
                    Throw New Exception(String.Format(ConstraintInvalid, .TypeCode, type))
                Else
                    Call Variables.Add(.ref)
                End If

                ' 位置值，相当于函数指针
                Return Variables.Count - 1
            End With
        End Function
    End Class
End Namespace

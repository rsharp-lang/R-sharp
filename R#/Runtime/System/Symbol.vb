#Region "Microsoft.VisualBasic::1e2b287461b06cb349886860924c8d49, R-sharp\R#\Runtime\System\Symbol.vb"

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


     Code Statistics:

        Total Lines:   204
        Code Lines:    130
        Comment Lines: 47
        Blank Lines:   27
        File Size:     7.11 KB


    '     Class Symbol
    ' 
    '         Properties: [readonly], [typeof], constraint, constraintValid, isCallable
    '                     length, name, stacktrace, typeCode, typeId
    '                     value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetValueViewString, SetValue, ToString, ToVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Components

    ''' <summary>
    ''' The variable model in R# language
    ''' </summary>
    Public Class Symbol : Implements INamedValue, Value(Of Object).IValueOf

        Public Property name As String Implements IKeyedEntity(Of String).Key
        ''' <summary>
        ''' which runtime stack that this variable symbol is created?
        ''' </summary>
        ''' <returns></returns>
        Public Property stacktrace As StackFrame()

        Dim m_val As Object

        ''' <summary>
        ''' 变量值对于基础类型而言，都是以数组的形式存储的
        ''' 非基础类型则为其值本身
        ''' </summary>
        ''' <returns></returns>
        Public Property value As Object Implements Value(Of Object).IValueOf.Value
            Get
                Return m_val
            End Get
            Private Set(value As Object)
                ' do nothing
            End Set
        End Property

        ''' <summary>
        ''' 当前的这个变量被约束的类型
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property constraint As TypeCodes

        Public Property [readonly] As Boolean

        ''' <summary>
        ''' <see cref="RType.fullName"/>, key for <see cref="GlobalEnvironment.types"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property typeId As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return [typeof].FullName
            End Get
        End Property

        ''' <summary>
        ''' Get the type of the current object <see cref="Value"/>.
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property [typeof] As Type
            Get
                If value Is Nothing Then
                    Return GetType(Object)
                ElseIf constraint = TypeCodes.closure Then
                    Return value.GetType
                ElseIf constraint <> TypeCodes.generic Then
                    Return Runtime.GetType(constraint)
                Else
                    Return value.GetType
                End If
            End Get
        End Property

        ''' <summary>
        ''' 当前的这个变量的值所具有的类型代码
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property typeCode As TypeCodes
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.typeof.GetRTypeCode
            End Get
        End Property

        ''' <summary>
        ''' The vector length
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property length As Integer
            Get
                If typeCode.IsPrimitive AndAlso value.GetType.IsInheritsFrom(GetType(Array)) Then
                    Return DirectCast(value, Array).Length
                Else
                    Return 1
                End If
            End Get
        End Property

        ''' <summary>
        ''' 当前的这个符号值是否是一个可以被调用的函数对象？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isCallable As Boolean
            Get
                Return REnv.isCallable(value)
            End Get
        End Property

        ''' <summary>
        ''' 当前的变量值的类型代码是否满足类型约束条件
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property constraintValid As Boolean
            Get
                If value Is Nothing Then
                    ' nothing 可以转换为任意数据类型
                    Return True
                ElseIf constraint = TypeCodes.generic Then
                    ' 没有类型约束，则肯定是有效的
                    Return True
                Else
                    Return constraint = typeCode
                End If
            End Get
        End Property

        ''' <summary>
        ''' NULL
        ''' </summary>
        ''' <param name="constraint"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(Optional constraint As TypeCodes = TypeCodes.generic)
            Me.constraint = constraint
            Me.m_val = Nothing
        End Sub

        Sub New(value As Object, Optional constraint As TypeCodes = TypeCodes.generic)
            Me.New(constraint)
            Me.m_val = value
        End Sub

        Sub New(name As String, value As Object,
                Optional constraint As TypeCodes = TypeCodes.generic,
                Optional [readonly] As Boolean = False)

            Call Me.New(constraint)

            Me.readonly = [readonly]
            Me.name = name
            Me.m_val = value
        End Sub

        Public Function SetValue(x As Object, env As Environment) As Message
            If [readonly] Then
                Return Internal.debug.stop($"cannot change value of locked binding for '{name}'", env)
            Else
                m_val = x
            End If

            Return Nothing
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"Dim {name} As ({typeCode}){Me.typeof.FullName} = {GetValueViewString(Me)}"
        End Function

        Public Shared Function GetValueViewString(var As Symbol) As String
            Dim value = var.ToVector()
            Dim str$

            If value.Length = 1 Then
                str = CStrSafe(value(Scan0), [default]:="NULL")
            Else
                str = Iterator Function() As IEnumerable(Of String)
                          For Each x As Object In value.AsQueryable
                              Yield CStrSafe(x, [default]:="NULL")
                          Next
                      End Function().JoinBy(", ")

                If str.Length > 64 Then
                    str = Mid(str, 1, 60) & "..."
                End If

                str = $"[{str}]"
            End If

            Return str
        End Function

        Public Function ToVector() As Array
            If value Is Nothing Then
                Return {Nothing}
            ElseIf typeCode.IsPrimitive AndAlso value.GetType.IsInheritsFrom(GetType(Array)) Then
                Return DirectCast(value, Array)
            Else
                Return {value}
            End If
        End Function
    End Class
End Namespace

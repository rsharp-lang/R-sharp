#Region "Microsoft.VisualBasic::bffa17d36095f03e3a392bafdc65ac1e, R#\Runtime\System\Variable.vb"

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

    '     Class Variable
    ' 
    '         Properties: [typeof], constraint, constraintValid, length, name
    '                     typeCode, typeId, value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetValueViewString, ToString, ToVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    ''' <summary>
    ''' The variable model in R# language
    ''' </summary>
    Public Class Variable : Implements INamedValue, Value(Of Object).IValueOf

        Public Property name As String Implements IKeyedEntity(Of String).Key

        ''' <summary>
        ''' 变量值对于基础类型而言，都是以数组的形式存储的
        ''' 非基础类型则为其值本身
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property value As Object Implements Value(Of Object).IValueOf.Value

        ''' <summary>
        ''' 当前的这个变量被约束的类型
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property constraint As TypeCodes

        ''' <summary>
        ''' <see cref="RType.fullName"/>, key for <see cref="Environment.types"/>
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(Optional constraint As TypeCodes = TypeCodes.generic)
            Me.constraint = constraint
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"Dim {name} As ({typeCode}){Me.typeof.FullName} = {GetValueViewString(Me)}"
        End Function

        Public Shared Function GetValueViewString(var As Variable) As String
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

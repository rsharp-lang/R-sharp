#Region "Microsoft.VisualBasic::c8557442b7b7073631b2e04ac447c9b1, ..\R-sharp\R#\runtime\Variable.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime.PrimitiveTypes

Namespace Runtime

    ''' <summary>
    ''' The variable model in R# language
    ''' </summary>
    Public Class Variable : Implements INamedValue, Value(Of Object).IValueOf

        Public Property Name As String Implements IKeyedEntity(Of String).Key
        Public Overridable Property Value As Object Implements Value(Of Object).IValueOf.Value

        ''' <summary>
        ''' 当前的这个变量被约束的类型
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Constraint As TypeCodes

        ''' <summary>
        ''' <see cref="RType.Identity"/>, key for <see cref="Environment.Types"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TypeID As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return [TypeOf].FullName
            End Get
        End Property

        ''' <summary>
        ''' Get the type of the current object <see cref="Value"/>.
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property [TypeOf] As Type
            Get
                If Value Is Nothing Then
                    Return GetType(Object)
                Else
                    Return Value.GetType
                End If
            End Get
        End Property

        ''' <summary>
        ''' 当前的这个变量的值所具有的类型代码
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TypeCode As TypeCodes
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.TypeOf.GetRTypeCode
            End Get
        End Property

        Public ReadOnly Property Length As Integer
            Get
                If TypeCode.IsPrimitive Then
                    Return ToVector.Length
                Else
                    Return 1
                End If
            End Get
        End Property

        ''' <summary>
        ''' 当前的变量值的类型代码是否满足类型约束条件
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ConstraintValid As Boolean
            Get
                If Constraint = TypeCodes.generic Then
                    Return True   ' 没有类型约束，则肯定是有效的
                Else
                    Return Constraint = TypeCode
                End If
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(constraint As TypeCodes)
            Me.Constraint = constraint
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Dim value = ToVector()
            Dim str$

            If value.Length = 1 Then
                str = CStrSafe(value(Scan0))
            Else
                str = value.Select(Function(x) CStrSafe(x)).JoinBy(", ")
                str = $"[{str}]"
            End If

            Return $"Dim {Name} As ({TypeCode}){Me.TypeOf.FullName} = {str}"
        End Function

        Public Function ToVector() As Object()
            If TypeCode.IsPrimitive Then

                Select Case [TypeOf]

                    Case Core.TypeDefine(Of Integer).BaseType,
                         Core.TypeDefine(Of Double).BaseType,
                         Core.TypeDefine(Of Boolean).BaseType,
                         Core.TypeDefine(Of ULong).BaseType,
                         Core.TypeDefine(Of Char).BaseType,
                         Core.TypeDefine(Of String).BaseType

                        Return {Value}

                    Case Else
                        Return DirectCast(Value, IEnumerable).ToArray(Of Object)

                End Select
            Else
                Return {Value}
            End If
        End Function
    End Class
End Namespace

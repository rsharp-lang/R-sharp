﻿#Region "Microsoft.VisualBasic::14b64b230b2207f2857540de789f2597, R#\Runtime\Vectorization\GetVectorElement.vb"

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

    '   Total Lines: 318
    '    Code Lines: 204 (64.15%)
    ' Comment Lines: 76 (23.90%)
    '    - Xml Docs: 84.21%
    ' 
    '   Blank Lines: 38 (11.95%)
    '     File Size: 12.00 KB


    '     Class GetVectorElement
    ' 
    '         Properties: [Error], elementType, isNullOrEmpty, Mode, size
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: CastTo, Create, (+2 Overloads) CreateAny, CreateVectorInternal, DoesSizeMatch
    '                   FromCollection, (+2 Overloads) Getter, IsScalar, Populate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime.Vectorization

    ''' <summary>
    ''' helper class object for make a safe vector element visiting
    ''' </summary>
    Public Class GetVectorElement

        ''' <summary>
        ''' is a scalar value
        ''' </summary>
        Friend ReadOnly [single] As Object
        ''' <summary>
        ''' is a vector data
        ''' </summary>
        Friend ReadOnly vector As Array
        ''' <summary>
        ''' method cache for get value by index for unify
        ''' the <see cref="[single]"/> scalar and the 
        ''' <see cref="vector"/> array.
        ''' </summary>
        ReadOnly m_get As Func(Of Integer, Object)

        Public ReadOnly Property [Error] As Exception
        Public ReadOnly Property elementType As Type

        ''' <summary>
        ''' does the given input vector data is nothing or a clr array with zero elements inside?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isNullOrEmpty As Boolean
            Get
                Return vector Is Nothing OrElse vector.Length = 0
            End Get
        End Property

        ''' <summary>
        ''' get elements inside current vector with i index
        ''' </summary>
        ''' <param name="i">zero-based vector element index value</param>
        ''' <returns>
        ''' this property unify the array get element and scalar value getter
        ''' </returns>
        Default Public ReadOnly Property item(i As Integer) As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_get(i)
            End Get
        End Property

        Public ReadOnly Property size As Integer
            Get
                If vector Is Nothing Then
                    Return 0
                Else
                    Return vector.Length
                End If
            End Get
        End Property

        Public Property Mode As VectorTypes

        ''' <summary>
        ''' maybe a scalar value or a array vector
        ''' </summary>
        ''' <param name="vec"></param>
        Private Sub New(vec As Array, type As Type)
            Me.vector = vec
            Me.elementType = type

            If vec Is Nothing OrElse vec.Length = 0 Then
                [single] = Nothing
                Mode = VectorTypes.None
            Else
                [single] = vec.GetValue(Scan0)

                ' vector just has one element
                ' and also the single element is nothing
                ' then the entire vector is a null literal value
                If vec.Length = 1 AndAlso [single] Is Nothing Then
                    Me.vector = Nothing
                    Mode = VectorTypes.None
                ElseIf vec.Length = 1 Then
                    Mode = VectorTypes.Scalar
                Else
                    Mode = VectorTypes.Vector
                End If
            End If

            m_get = Getter()
        End Sub

        Private Sub New(ex As Exception)
            Me.Error = ex
            Me.Mode = VectorTypes.Error
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="scalar">
        ''' this item should never be nothing?
        ''' </param>
        Sub New(scalar As Object, type As Type)
            If (Not type.IsArray) AndAlso (scalar IsNot Nothing) AndAlso scalar.GetType.IsArray Then
                Me.vector = DirectCast(scalar, Array)
            Else
                Me.vector = {scalar}
            End If

            If Me.vector.Length > 0 Then
                Me.[single] = Me.vector.GetValue(Scan0)
                Me.Mode = If(Me.vector.Length = 1, VectorTypes.Scalar, VectorTypes.Vector)
            Else
                Me.single = Nothing
                Me.Mode = VectorTypes.None
            End If

            Me.m_get = Getter()
            Me.elementType = type
        End Sub

        Public Overrides Function ToString() As String
            Select Case Mode
                Case VectorTypes.Scalar
                    Return any.ToString([single], "null")
                Case VectorTypes.Vector
                    Return $"[vector, size={size}] [0]{any.ToString([single], "null")}"
                Case VectorTypes.None
                    Return "null"
                Case Else
                    Return "Error: " & [Error].Message
            End Select
        End Function

        Public Function CastTo(Of T)(cast As Func(Of Object, T)) As GetVectorElement
            If vector Is Nothing OrElse vector.Length = 0 Then
                Return New GetVectorElement({}, GetType(T))
            Else
                Return (From item As Object In vector.AsQueryable Select cast(item)) _
                    .DoCall(Function(n)
                                Return New GetVectorElement(n.ToArray, GetType(T))
                            End Function)
            End If
        End Function

        Public Iterator Function Populate(Of T)(unary As Func(Of Object, Object)) As IEnumerable(Of T)
            If vector Is Nothing Then
                Return
            End If

            For i As Integer = 0 To vector.Length - 1
                Yield DirectCast(unary(vector.GetValue(i)), T)
            Next
        End Function

        Public Function Getter(Of T)() As Func(Of Integer, T)
            Dim gets = Me.Getter
            Dim castDirect = Function(i) As T
                                 Return CType(gets(i), T)
                             End Function

            Return castDirect
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' the lambda function accepts a zero-based index value for
        ''' get element value from a clr vector <see cref="Array"/> 
        ''' object
        ''' </returns>
        Public Function Getter() As Func(Of Integer, Object)
            If isNullOrEmpty OrElse vector.Length = 1 Then
                Return Function() [single]
            Else
                ' R对向量的访问是可以下标越界的
                Return Function(i)
                           If i >= vector.Length Then
                               Return Nothing
                           Else
                               Return vector.GetValue(i)
                           End If
                       End Function
            End If
        End Function

        Public Shared Function DoesSizeMatch(v1 As GetVectorElement, v2 As GetVectorElement) As Boolean
            If v1.size = 1 OrElse v2.size = 1 Then
                Return True
            Else
                Return v2.size = v2.size
            End If
        End Function

        ''' <summary>
        ''' if the target input object <paramref name="x"/>is nothing, then this function
        ''' will returns an instance of <see cref="GetVectorElement"/> with wrap a null
        ''' value
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Function Create(Of T)(x As Object) As GetVectorElement
            If x Is Nothing Then
                Return New GetVectorElement(vec:=Nothing, GetType(T))
            ElseIf TypeOf x Is T() Then
                Return New GetVectorElement(vec:=DirectCast(x, T()), GetType(T))
            ElseIf TypeOf x Is GetVectorElement AndAlso DirectCast(x, GetVectorElement).elementType Is GetType(T) Then
                Return DirectCast(x, GetVectorElement)
            Else
                If TypeOf x Is vector Then
                    x = DirectCast(x, vector).data
                End If

                Return CreateVectorInternal(Of T)(x)
            End If
        End Function

        ''' <summary>
        ''' if the target input object <paramref name="x"/>is nothing, then this function
        ''' will returns an instance of <see cref="GetVectorElement"/> with wrap a null
        ''' value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Function CreateAny(x As Object) As GetVectorElement
            Dim any As Type = GetType(Object)

            If x Is Nothing Then
                Return New GetVectorElement(vec:=Nothing, any)
            ElseIf TypeOf x Is GetVectorElement AndAlso DirectCast(x, GetVectorElement).elementType Is any Then
                Return DirectCast(x, GetVectorElement)
            Else
                If TypeOf x Is vector Then
                    x = DirectCast(x, vector).data
                End If

                Return CreateVectorInternal(Of Object)(x)
            End If
        End Function

        Public Shared Function CreateAny(x As Array) As GetVectorElement
            Dim any As Type = GetType(Object)

            If x Is Nothing Then
                Return New GetVectorElement(vec:=Nothing, any)
            Else
                Return CreateVectorInternal(Of Object)(x)
            End If
        End Function

        Friend Shared Function CreateVectorInternal(Of T)(x As Object) As GetVectorElement
            Dim type As Type = x.GetType

            If type Is typedefine(Of T).baseType Then
                ' is a scalar
                Return New GetVectorElement(scalar:=x, type:=type)
            ElseIf type.ImplementInterface(typedefine(Of T).enumerable) Then
                Return FromCollection(type.IsArray, GetType(T), x)
            Else
                If GetType(T) Is GetType(Object) Then
                    ' string is a kind of special char collection
                    ' filter out the string value
                    If type IsNot GetType(String) AndAlso type.ImplementInterface(Of IEnumerable) Then
                        Return FromCollection(type.IsArray, GetType(T), x)
                    Else
                        Return FromCollection(True, GetType(T), {x})
                    End If
                End If

                ' do type cast?
                Return New GetVectorElement(ex:=New InvalidCastException($"Do we require a type cast for {type} -> {GetType(T)}?"))
            End If
        End Function

        Private Shared Function FromCollection(isArray As Boolean, type As Type, x As Object) As GetVectorElement
            ' is a generic collection
            If isArray Then
                Return New GetVectorElement(DirectCast(x, Array), type)
            Else
                ' cast collection to array
                Dim list As New List(Of Object)

                For Each item As Object In DirectCast(x, IEnumerable)
                    Call list.Add(item)
                Next

                Return New GetVectorElement(list.ToArray, type)
            End If
        End Function

        ''' <summary>
        ''' test the given <paramref name="obj"/> is a single scalar value?
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns>
        ''' this function return true if the given <paramref name="obj"/> is nothing
        ''' orelse is primitive type
        ''' </returns>
        Public Shared Function IsScalar(obj As Object) As Boolean
            If obj Is Nothing Then
                Return True
            ElseIf DataFramework.IsPrimitive(obj.GetType) Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::23b3841e2368ef90b4f1ef5e79b69432, R-sharp\R#\Runtime\Vectorization\GetVectorElement.vb"

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

    '   Total Lines: 158
    '    Code Lines: 109
    ' Comment Lines: 29
    '   Blank Lines: 20
    '     File Size: 5.35 KB


    '     Enum VectorTypes
    ' 
    '         None, Scalar, Vector
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class GetVectorElement
    ' 
    '         Properties: isNullOrEmpty, Mode, size
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CastTo, Create, Getter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Namespace Runtime.Vectorization

    Public Enum VectorTypes
        None
        Scalar
        Vector
    End Enum

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

        Public ReadOnly Property isNullOrEmpty As Boolean
            Get
                Return vector Is Nothing OrElse vector.Length = 0
            End Get
        End Property

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
        Sub New(vec As Array)
            Me.vector = vec

            If vec Is Nothing OrElse vec.Length = 0 Then
                [single] = Nothing
                Mode = VectorTypes.None
            Else
                [single] = vec.GetValue(Scan0)

                If [single] Is Nothing Then
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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="scalar">
        ''' this item should never be nothing?
        ''' </param>
        Private Sub New(scalar As Object)
            Me.vector = {scalar}
            Me.[single] = scalar
            Me.m_get = Getter()
            Me.Mode = VectorTypes.Scalar
        End Sub

        Public Function CastTo(Of T)(cast As Func(Of Object, T)) As GetVectorElement
            If vector Is Nothing OrElse vector.Length = 0 Then
                Return New GetVectorElement({})
            Else
                Return (From item As Object In vector.AsQueryable Select cast(item)) _
                    .DoCall(Function(n)
                                Return New GetVectorElement(n.ToArray)
                            End Function)
            End If
        End Function

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

        Public Shared Function Create(Of T)(x As Object) As GetVectorElement
            If x Is Nothing Then
                Return New GetVectorElement(vec:=Nothing)
            Else
                If TypeOf x Is vector Then
                    x = DirectCast(x, vector).data
                End If

                Dim type As Type = x.GetType

                If type Is typedefine(Of T).baseType Then
                    ' is a scalar
                    Return New GetVectorElement(scalar:=x)
                ElseIf type.ImplementInterface(typedefine(Of T).enumerable) Then
                    ' is a generic collection
                    If type.IsArray Then
                        Return New GetVectorElement(DirectCast(x, Array))
                    Else
                        ' cast collection to array
                        Dim list As New List(Of Object)

                        For Each item As Object In DirectCast(x, IEnumerable)
                            Call list.Add(item)
                        Next

                        Return New GetVectorElement(list.ToArray)
                    End If
                Else
                    ' do type cast?
                    Throw New InvalidCastException($"Do we require a type cast for {type} -> {GetType(T)}?")
                End If
            End If
        End Function
    End Class
End Namespace

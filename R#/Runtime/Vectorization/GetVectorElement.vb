#Region "Microsoft.VisualBasic::0e12f14d2af42d09cc3eee31e22634a2, R-sharp\R#\Runtime\System\GetVectorElement.vb"

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

    '   Total Lines: 68
    '    Code Lines: 54
    ' Comment Lines: 4
    '   Blank Lines: 10
    '     File Size: 2.27 KB


    '     Class GetVectorElement
    ' 
    '         Properties: isNullOrEmpty
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CastTo, Getter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Vectorization

    ''' <summary>
    ''' helper class object for make a safe vector element visiting
    ''' </summary>
    Public Class GetVectorElement

        ''' <summary>
        ''' is a scalar value
        ''' </summary>
        ReadOnly [single] As Object
        ''' <summary>
        ''' is a vector data
        ''' </summary>
        ReadOnly vector As Array
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

        Sub New(vec As Array)
            Me.vector = vec

            If vec Is Nothing Then
                [single] = Nothing
            ElseIf vec.Length = 0 Then
                [single] = Nothing
            Else
                [single] = vec.GetValue(Scan0)
            End If

            m_get = Getter()
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
    End Class
End Namespace

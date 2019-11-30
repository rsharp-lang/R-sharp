#Region "Microsoft.VisualBasic::cf83550b7d755354885e692b2f99eff8, R#\Runtime\RVectorExtensions.vb"

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

    '     Module RVectorExtensions
    ' 
    '         Function: (+2 Overloads) asVector, createArray, fromArray, getFirst
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline

Namespace Runtime

    <HideModuleName> Public Module RVectorExtensions

        ''' <summary>
        ''' Get first element in the input <paramref name="value"/> sequence
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Friend Function getFirst(value As Object) As Object
            Dim valueType As Type

            If value Is Nothing Then
                Return Nothing
            Else
                valueType = value.GetType
            End If

            If valueType.IsInheritsFrom(GetType(Array)) Then
                With DirectCast(value, Array)
                    If .Length = 0 Then
                        Return Nothing
                    Else
                        Return .GetValue(Scan0)
                    End If
                End With
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' Ensure that the input <paramref name="value"/> object is a sequence. 
        ''' (This method will decouple the object instance value from vbObject 
        ''' container unless the required <paramref name="type"/> is 
        ''' <see cref="vbObject"/>.)
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function asVector(value As Object, type As Type) As Array
            Dim arrayType As Type = type.MakeArrayType
            Dim valueType As Type

            If value Is Nothing Then
                Return Nothing
            Else
                valueType = value.GetType
            End If

            If Not valueType Is arrayType Then
                If valueType.IsArray Then
                    Return type.createArray(value)
                ElseIf valueType Is GetType(Group) Then
                    Return type.createArray(DirectCast(value, Group).group)
                Else
                    Dim array As Array = Array.CreateInstance(type, 1)
                    array.SetValue(RConversion.CTypeDynamic(value, type), Scan0)
                    Return array
                End If
            Else
                Return value
            End If
        End Function

        <Extension>
        Private Function createArray(type As Type, value As Object) As Object
            Dim src As Array = value
            Dim array As Array = Array.CreateInstance(type, src.Length)

            For i As Integer = 0 To array.Length - 1
                array.SetValue(RConversion.CTypeDynamic(src.GetValue(i), type), i)
            Next

            Return array
        End Function

        ''' <summary>
        ''' 这个函数会确保返回的输出值都是一个数组
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function asVector(Of T)(value As Object) As Array
            Dim valueType As Type = value.GetType
            Dim typeofT As Type = GetType(T)

            If valueType Is typeofT Then
                Return {DirectCast(value, T)}
            ElseIf valueType.IsInheritsFrom(GetType(Array)) Then
                Return typeofT.fromArray(Of T)(value)
            ElseIf valueType Is GetType(Group) Then
                Return typeofT.fromArray(Of T)(DirectCast(value, Group).group)
            ElseIf valueType Is GetType(T()) Then
                Return value
            ElseIf valueType.IsInheritsFrom(GetType(IEnumerable(Of T))) Then
                Return DirectCast(value, IEnumerable(Of T)).ToArray
            Else
                Return {value}
            End If
        End Function

        <Extension>
        Private Function fromArray(Of T)(typeofT As Type, value As Object) As Object
            If DirectCast(value, Array) _
                .AsObjectEnumerator _
                .All(Function(i)
                         If Not i.GetType.IsInheritsFrom(GetType(Array)) Then
                             Return True
                         Else
                             Return DirectCast(i, Array).Length = 1
                         End If
                     End Function) Then

                value = DirectCast(value, Array) _
                    .AsObjectEnumerator _
                    .Select(Function(o)
                                If Not o.GetType Is typeofT Then
                                    If o.GetType.IsInheritsFrom(GetType(Array)) Then
                                        o = DirectCast(o, Array).GetValue(Scan0)
                                    End If
                                End If
                                If Not o.GetType Is typeofT Then
                                    ' 进行一些类型转换

                                    ' if apply the RConversion.CTypeDynamic
                                    ' then it may decouple object from vbObject container
                                    o = Conversion.CTypeDynamic(o, typeofT)
                                End If

                                Return DirectCast(o, T)
                            End Function) _
                    .ToArray
            End If

            Return value
        End Function
    End Module
End Namespace

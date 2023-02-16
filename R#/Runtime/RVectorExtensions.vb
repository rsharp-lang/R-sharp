#Region "Microsoft.VisualBasic::69f140b31dfbc40c29c061feb4dcbdc8, E:/GCModeller/src/R-sharp/R#//Runtime/RVectorExtensions.vb"

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

    '   Total Lines: 481
    '    Code Lines: 340
    ' Comment Lines: 82
    '   Blank Lines: 59
    '     File Size: 19.05 KB


    '     Module RVectorExtensions
    ' 
    '         Function: [single], (+2 Overloads) asVector, castSingle, createArray, CTypeOfList
    '                   fromArray, getFirst, isVector, MeltArray, TryCastGenericArray
    '                   UnsafeTryCastGenericArray
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime

    <HideModuleName> Public Module RVectorExtensions

        ''' <summary>
        ''' Object ``x`` is an array of <typeparamref name="T"/>?
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns>
        ''' returns a logical value of this type test operation
        ''' </returns>
        Public Function isVector(Of T)(x As Object) As Boolean
            If x Is Nothing Then
                Return False
            ElseIf TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If

            If Not x.GetType.IsArray Then
                If x.GetType Is GetType(T) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Dim type As Type = x.GetType
                Dim array As Array = x

                If type Is GetType(T()) OrElse type.ImplementInterface(GetType(IEnumerable(Of T))) Then
                    Return True
                ElseIf array.Length = 0 Then
                    Return False
                ElseIf array _
                    .AsObjectEnumerator _
                    .All(Function(ti)
                             If ti Is Nothing Then
                                 Return True
                             ElseIf ti Is GetType(T) Then
                                 Return True
                             ElseIf ti.GetType.IsArray Then
                                 Dim first = DirectCast(ti, Array).GetValueOrDefault(Scan0)
                                 Return first Is Nothing OrElse TypeOf first Is T
                             Else
                                 Return False
                             End If
                         End Function) Then

                    Return True
                Else
                    Dim first As Object = array.GetValue(Scan0)
                    Return first IsNot Nothing AndAlso first.GetType Is GetType(T)
                End If
            End If
        End Function

        ''' <summary>
        ''' Get first element in the input <paramref name="value"/> sequence
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function getFirst(value As Object, Optional nonNULL As Boolean = False) As Object
            Dim valueType As Type

            If value Is Nothing Then
                Return Nothing
            Else
                If value.GetType Is GetType(vector) Then
                    value = DirectCast(value, vector).data
                End If

                valueType = value.GetType
            End If

            If valueType.IsArray Then
                With DirectCast(value, Array)
                    If .Length = 0 Then
                        Return Nothing
                    ElseIf nonNULL Then
                        For i As Integer = 0 To .Length - 1
                            If Not .GetValue(i) Is Nothing Then
                                Return .GetValue(i)
                            End If
                        Next

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
        ''' Try get a single element
        ''' </summary>
        ''' <param name="x">
        ''' If the input object x is an array with just one element, 
        ''' then the single value will be populate, otherwise will 
        ''' populate the input x
        ''' </param>
        ''' <param name="forceSingle">
        ''' this function returns the first element in array/vector always
        ''' if this parameter is set to true
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数只会在确认只有一个向量元素的情况下才会返回单个元素
        ''' </remarks>
        Public Function [single](x As Object, Optional forceSingle As Boolean = False) As Object
            If x Is Nothing Then
                Return Nothing
            End If

            If x.GetType.IsArray Then
                If DirectCast(x, Array).Length = 1 Then
                    Return DirectCast(x, Array).GetValue(Scan0)
                ElseIf DirectCast(x, Array).Length = 0 Then
                    Return Nothing
                ElseIf forceSingle Then
                    Return DirectCast(x, Array).GetValue(Scan0)
                End If
            ElseIf x.GetType Is GetType(vector) Then
                If DirectCast(x, vector).length = 1 Then
                    Return DirectCast(x, vector).data.GetValue(Scan0)
                ElseIf DirectCast(x, vector).length = 0 Then
                    Return Nothing
                ElseIf forceSingle Then
                    Return DirectCast(x, vector).data.GetValue(Scan0)
                End If
            End If

            ' x is not single 
            ' OrElse x is not a collection, return x directly
            Return x
        End Function

        ''' <summary>
        ''' Ensure that the input <paramref name="value"/> object is a sequence. 
        ''' (This method will decouple the object instance value from vbObject 
        ''' container unless the required <paramref name="type"/> is 
        ''' <see cref="vbObject"/>.)
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="type">
        ''' should be the element type of the target vector array
        ''' </param>
        ''' <returns>
        ''' 如果执行出错，这个函数会返回一个错误消息
        ''' </returns>
        Public Function asVector(value As Object, type As Type, env As Environment) As Object
            Dim arrayType As Type = type.MakeArrayType
            Dim valueType As Type

            If value Is Nothing Then
                Return Nothing
            Else
                If value.GetType Is GetType(vector) Then
                    value = DirectCast(value, vector).data
                End If

                valueType = value.GetType
            End If

            'If valueType Is GetType(list) Then
            '    ' list value as vector?
            'End If

            If Not valueType Is arrayType Then
                If valueType.IsArray Then
                    If type Is GetType(Void) Then
                        Return value
                    Else
                        Return type.createArray(value, env)
                    End If
                ElseIf valueType Is GetType(Group) Then
                    If type Is GetType(Void) Then
                        Return DirectCast(value, Group).group
                    Else
                        Return type.createArray(DirectCast(value, Group).group, env)
                    End If
                ElseIf type Is GetType(Void) Then
                    Dim one As Object = [single](value)

                    If one Is Nothing Then
                        Return one
                    Else
                        Return {one}
                    End If
                Else
                    Dim array As Array = Array.CreateInstance(type, 1)
                    Dim [single] As Object = RCType.CTypeDynamic(value, type, env)

                    If Program.isException([single]) Then
                        Return [single]
                    Else
                        Call array.SetValue([single], Scan0)
                    End If

                    Return array
                End If
            Else
                Return value
            End If
        End Function

        <Extension>
        Private Function createArray(type As Type, value As Object, env As Environment) As Object
            Dim src As Array = value
            Dim array As Array = Array.CreateInstance(type, src.Length)
            Dim castValue As Object

            For i As Integer = 0 To array.Length - 1
                castValue = RCType.CTypeDynamic(src.GetValue(i), type, env)

                If Program.isException(castValue) Then
                    Return castValue
                Else
                    Call array.SetValue(castValue, i)
                End If
            Next

            Return array
        End Function

        Public Function CTypeOfList(Of T)(list As IDictionary, env As Environment) As Dictionary(Of String, T)
            Dim ofList As New Dictionary(Of String, T)
            Dim elementType As Type = GetType(T)

            For Each key As Object In list.Keys
                ofList(any.ToString(key)) = RCType.CTypeDynamic(list.Item(key), elementType, env)
            Next

            Return ofList
        End Function

        Public Function UnsafeTryCastGenericArray(vec As Array) As Array
            Dim elementType As Type
            Dim generic As Array

            vec = MeltArray(vec)
            elementType = MeasureRealElementType(vec)

            If elementType Is Nothing OrElse elementType Is GetType(Object) Then
                Return vec
            Else
                generic = Array.CreateInstance(elementType, vec.Length)
            End If

            For i As Integer = 0 To vec.Length - 1
                Call generic.SetValue(CTypeDynamic(vec.GetValue(i), elementType), i)
            Next

            Return generic
        End Function

        Private Function MeltArray(vec As Array) As Array
            Dim elementType As Type = vec.GetType.GetElementType

            If elementType IsNot Nothing AndAlso
                elementType IsNot GetType(Object) AndAlso
                Not elementType.IsArray Then

                Return vec
            End If

            If vec _
                .AsObjectEnumerator _
                .Take(100) _
                .All(Function(xi)
                         Return TypeOf xi Is vector AndAlso DirectCast(xi, vector).length = 1
                     End Function) Then

                vec = vec _
                    .AsObjectEnumerator _
                    .Select(Function(xi)
                                Return DirectCast(xi, vector).data.GetValue(Scan0)
                            End Function) _
                    .ToArray
            End If

            Return vec
        End Function

        ''' <summary>
        ''' This function make sure the return array is not a generic type array
        ''' 
        ''' (返回错误消息或者结果向量)
        ''' </summary>
        ''' <param name="vec"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' A class variant type: error message or a generic array
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function TryCastGenericArray(vec As Array, env As Environment) As Object
            vec = MeltArray(vec)
            Return asVector(vec, MeasureRealElementType(vec), env)
        End Function

        ''' <summary>
        ''' 这个函数会确保返回的输出值都是一个数组
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="value">
        ''' 
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ##### 20210526 因为这个函数会涉及到转换类型的操作，所以性能损耗会非常严重
        ''' 
        ''' 所以假若仅仅只需要转换数据对象为数组的话，请避免使用这个函数
        ''' 应该手动编写代码以提升性能
        ''' </remarks>
        Public Function asVector(Of T)(value As Object) As Array
            Dim valueType As Type
            Dim typeofT As Type = GetType(T)

            If value Is Nothing Then
                Return New T() {}
            Else
                If value.GetType Is GetType(vector) Then
                    value = DirectCast(value, vector).data
                End If
                If value.GetType Is GetType(list) AndAlso DirectCast(value, list).length = 0 Then
                    Return New T() {}
                End If

                valueType = value.GetType
            End If

            If GetType(T) Is GetType(Object) Then
                If value.GetType.IsArray Then
                    Return DirectCast(value, Array)
                ElseIf value.GetType.ImplementInterface(Of RIndex) Then
                    Dim index As RIndex = DirectCast(value, RIndex)
                    Dim list As New List(Of Object)

                    For i As Integer = 1 To index.length
                        Call list.Add(index.getByIndex(i))
                    Next

                    Return list.ToArray
                End If
            End If

            If TypeOf value Is String AndAlso Not GetType(T) Is GetType(Char) Then
                Return New T() {Conversion.CTypeDynamic(Of T)(value)}
            End If

            If valueType Is typeofT Then
                Return {DirectCast(value, T)}
            ElseIf valueType Is GetType(T()) Then
                Return DirectCast(value, T())
            ElseIf valueType.IsArray Then
                Return typeofT.fromArray(Of T)(value)
            ElseIf valueType Is GetType(Group) Then
                Return typeofT.fromArray(Of T)(DirectCast(value, Group).group)
            ElseIf valueType.ImplementInterface(GetType(IEnumerable(Of T))) Then
                Return DirectCast(value, IEnumerable(Of T)).ToArray
            ElseIf valueType.ImplementInterface(GetType(IEnumerable)) Then
                Return (From obj In DirectCast(value, IEnumerable) Select DirectCast(obj, T)).ToArray
            Else
                If typeofT Is GetType(Object) Then
                    Return {DirectCast(value, T)}
                ElseIf typeofT Is GetType(Boolean) AndAlso valueType Is GetType(String) Then
                    Return {DirectCast(value, String).ParseBoolean}
                Else
                    Return {Conversion.CTypeDynamic(Of T)(value)}
                End If
            End If
        End Function

        <Extension>
        Private Function fromArray(Of T)(typeofT As Type, value As Object) As Array
            Dim vec As Array = DirectCast(value, Array)

            If vec.AsObjectEnumerator.All(Function(i) i Is Nothing) Then
                Return New T(vec.Length - 1) {}
            End If

            If DirectCast(value, Array) _
                .AsObjectEnumerator _
                .All(Function(i)
                         If i Is Nothing Then
                             Return False
                         End If
                         If Not i.GetType.IsInheritsFrom(GetType(Array)) Then
                             Return True
                         Else
                             Return DirectCast(i, Array).Length = 1
                         End If
                     End Function) Then

                value = DirectCast(value, Array) _
                    .AsObjectEnumerator _
                    .Select(Function(o)
                                Return typeofT.castSingle(Of T)(o)
                            End Function) _
                    .ToArray
            ElseIf TypeOf value Is Object() AndAlso Not GetType(T) Is GetType(Object) Then
                value = DirectCast(value, Array) _
                    .AsObjectEnumerator(Of T) _
                    .ToArray
            End If

            Return value
        End Function

        ''' <summary>
        ''' handling some special type cast situation
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="typeofT"></param>
        ''' <param name="o"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function castSingle(Of T)(typeofT As Type, o As Object) As T
            If Not o.GetType Is typeofT Then
                If o.GetType.IsInheritsFrom(GetType(Array)) Then
                    o = DirectCast(o, Array).GetValue(Scan0)
                End If
            End If

            If Not o.GetType Is typeofT Then
                ' handling some special situation
                If GetType(T) Is GetType(Double) OrElse
                    GetType(T) Is GetType(Single) OrElse
                    GetType(T) Is GetType(Integer) OrElse
                    GetType(T) Is GetType(Long) Then

                    If TypeOf o Is String Then
                        Dim str = CStr(o)

                        If GetType(T) Is GetType(Double) OrElse GetType(T) Is GetType(Single) Then
                            If str = "NA" Then
                                o = Conversion.CTypeDynamic(Double.NaN, typeofT)
                            ElseIf str = "NULL" OrElse str = "" Then
                                o = Conversion.CTypeDynamic(0.0, typeofT)
                            Else
                                o = Conversion.CTypeDynamic(o, typeofT)
                            End If
                        Else
                            If str = "NA" OrElse str = "NULL" OrElse str = "" Then
                                o = Conversion.CTypeDynamic(0, typeofT)
                            Else
                                o = Conversion.CTypeDynamic(o, typeofT)
                            End If
                        End If
                    Else
                        o = Conversion.CTypeDynamic(o, typeofT)
                    End If
                ElseIf typeofT Is GetType(String) AndAlso o.GetType Is GetType(Void) Then
                    Return DirectCast(CObj("NULL"), T)
                ElseIf typeofT Is GetType(String) AndAlso TypeOf o Is Type AndAlso o Is GetType(Void) Then
                    Return DirectCast(CObj("NULL"), T)
                Else
                    ' if apply the RConversion.CTypeDynamic
                    ' then it may decouple object from vbObject container
                    o = Conversion.CTypeDynamic(o, typeofT)
                End If
            End If

            Return DirectCast(o, T)
        End Function
    End Module
End Namespace

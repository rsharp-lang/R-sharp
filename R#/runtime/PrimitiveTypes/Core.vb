#Region "Microsoft.VisualBasic::2d7ea93915c1f4ab13f5ca55b6bcae49, ..\R-sharp\R#\runtime\PrimitiveTypes\Core.vb"

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

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.PrimitiveTypes

    Public Module Core

        ''' <summary>
        ''' Type cache module
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        Public NotInheritable Class TypeDefine(Of T)

            ''' <summary>
            ''' The vector based type
            ''' </summary>
            Public Shared ReadOnly BaseType As Type
            ''' <summary>
            ''' The abstract vector type
            ''' </summary>
            Public Shared ReadOnly EnumerableType As Type

            Private Sub New()
            End Sub

            Shared Sub New()
                BaseType = GetType(T)
                EnumerableType = GetType(IEnumerable(Of T))
            End Sub
        End Class

        ''' <summary>
        ''' The ``In`` operator
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="collection"></param>
        ''' <returns></returns>
        Public Function op_In(Of T)(x As Object, collection As IEnumerable(Of T)) As IEnumerable(Of Boolean)

            If x Is Nothing Then
                Return {}
            End If

            Dim type As Type = x.GetType

            With collection.AsList

                If type Is TypeDefine(Of T).BaseType Then

                    ' Just one element in x, using list indexof is faster than using hash table
                    Return { .IndexOf(DirectCast(x, T)) > -1}

                ElseIf type.ImplementInterface(TypeDefine(Of T).EnumerableType) Then

                    ' This can be optimised by using hash table if the x and collection are both a large collection. 
                    Dim xVector = DirectCast(x, IEnumerable(Of T)).ToArray

                    If x.Vector.Length > 500 AndAlso .Count > 1000 Then

                        ' Using hash table optimised for large n*m situation          
                        With .AsHashSet()
                            Return xVector _
                                .Select(Function(n) .HasKey(n)) _
                                .ToArray
                        End With
                    Else

                        Return xVector _
                            .Select(Function(n) .IndexOf(n) > -1) _
                            .ToArray

                    End If

                Else
                    Throw New InvalidOperationException(type.FullName)
                End If
            End With
        End Function

        ''' <summary>
        ''' Build R# language reflection <see cref="MethodInfo"/>
        ''' </summary>
        ''' <typeparam name="TX"></typeparam>
        ''' <typeparam name="TY"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="[do]"></param>
        ''' <param name="castX"></param>
        ''' <param name="castY"></param>
        ''' <param name="fakeX">
        ''' 假若X参数为Boolean逻辑值类型，而目标的运算对象却是Integer的四则运算，故而需要先转换Boolean为Integer类型，
        ''' 但是经过转换之后获得的结果为Integer数组，很明显和Boolean的类型申明不符合，所以需要使用这个参数来指定一个
        ''' 假定类型
        ''' </param>
        ''' <param name="fakeY"></param>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        Public Function BuildMethodInfo(Of TX As IComparable(Of TX),
                                           TY As IComparable(Of TY), TOut)(
                                         [do] As Func(Of Object, Object, Object),
                                         castX As Func(Of Object, Object),
                                         castY As Func(Of Object, Object),
                                         Optional fakeX As Type = Nothing,
                                         Optional fakeY As Type = Nothing,
                                         <CallerMemberName>
                                         Optional name$ = Nothing) As RMethodInfo
            Dim operatorCall = [do]

            If Not castX Is Nothing Then
                operatorCall = Function(x, y) Core.BinaryCoreInternal(Of TX, TY, TOut)(castX(x), y, [do])
            ElseIf Not castY Is Nothing Then
                operatorCall = Function(x, y) Core.BinaryCoreInternal(Of TX, TY, TOut)(x, castY(y), [do])
            Else
                operatorCall = Function(x, y) Core.BinaryCoreInternal(Of TX, TY, TOut)(x, y, [do])
            End If

            ' using fake type or get type from generic type parameter if fake type is nothing
            fakeX = fakeX Or TypeDefine(Of TX).BaseType.AsDefault
            fakeY = fakeY Or TypeDefine(Of TY).BaseType.AsDefault

            Return New RMethodInfo({fakeX.Argv("x", 0), fakeY.Argv("y", 1)}, operatorCall, name)
        End Function

        Public ReadOnly op_Add As Func(Of Object, Object, Object) = Function(x, y) x + y
        Public ReadOnly op_Minus As Func(Of Object, Object, Object) = Function(x, y) x - y
        Public ReadOnly op_Multiply As Func(Of Object, Object, Object) = Function(x, y) x * y
        Public ReadOnly op_Divided As Func(Of Object, Object, Object) = Function(x, y) x / y
        Public ReadOnly op_Mod As Func(Of Object, Object, Object) = Function(x, y) x Mod y
        Public ReadOnly op_Power As Func(Of Object, Object, Object) = Function(x, y) CDbl(x) ^ CDbl(y)

        ''' <summary>
        ''' Build <see cref="MethodInfo"/> for unary operator in R# language
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="[do]">单目运算符只需要有一个参数，所以y是无用的</param>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        Public Function BuildMethodInfo(Of T As IComparable(Of T), TOut)([do] As Func(Of Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo
            Return New RMethodInfo(
                {
                    TypeDefine(Of T).BaseType.Argv("x", 0),
                    TypeDefine(Of TOut).BaseType.Argv("out", 1)
                },
                api:=Function(x, y) Core.UnaryCoreInternal(Of T, TOut)(x, [do]),
                name:=name)
        End Function

        ''' <summary>
        ''' Generic unary operator core for primitive type.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="[do]"></param>
        ''' <returns></returns>
        Public Function UnaryCoreInternal(Of T As IComparable(Of T), TOut)(x As Object, [do] As Func(Of Object, Object)) As IEnumerable(Of TOut)
            Dim type As Type = x.GetType

            If type Is TypeDefine(Of T).BaseType Then
                Return {DirectCast([do](x), TOut)}
            ElseIf type.ImplementInterface(TypeDefine(Of T).EnumerableType) Then
                Return DirectCast(x, IEnumerable(Of T)) _
                    .Select(Function(o) DirectCast([do](o), TOut)) _
                    .ToArray
            Else
                Throw New InvalidCastException(type.FullName)
            End If
        End Function

        ''' <summary>
        ''' [Vector core] Generic binary operator core for numeric type.
        ''' </summary>
        ''' <typeparam name="TX"></typeparam>
        ''' <typeparam name="TY"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="[do]"></param>
        ''' <returns></returns>
        Public Function BinaryCoreInternal(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x As Object, y As Object, [do] As Func(Of Object, Object, Object)) As IEnumerable(Of TOut)
            Dim xtype As Type = x.GetType
            Dim ytype As Type = y.GetType

            If xtype Is TypeDefine(Of TX).BaseType Then

                If ytype Is TypeDefine(Of TY).BaseType Then
                    Return {DirectCast([do](x, y), TOut)}

                ElseIf ytype.ImplementInterface(TypeDefine(Of TY).EnumerableType) Then

                    Return DirectCast(y, IEnumerable(Of TY)) _
                        .Select(Function(yi) DirectCast([do](x, yi), TOut)) _
                        .ToArray

                Else
                    Throw New InvalidCastException(ytype.FullName)
                End If

            ElseIf xtype.ImplementInterface(TypeDefine(Of TX).EnumerableType) Then

                If ytype Is TypeDefine(Of TY).BaseType Then
                    Return DirectCast(x, IEnumerable(Of TX)) _
                        .Select(Function(xi) DirectCast([do](xi, y), TOut)) _
                        .ToArray

                ElseIf ytype.ImplementInterface(TypeDefine(Of TY).EnumerableType) Then

                    Dim xlist = DirectCast(x, IEnumerable(Of TX)).ToArray
                    Dim ylist = DirectCast(y, IEnumerable(Of TY)).ToArray

                    If xlist.Length = 1 Then
                        x = xlist(0)
                        Return DirectCast(y, IEnumerable(Of TY)) _
                            .Select(Function(yi) DirectCast([do](x, yi), TOut)) _
                            .ToArray
                    ElseIf ylist.Length = 1 Then
                        y = ylist(0)
                        Return DirectCast(x, IEnumerable(Of TX)) _
                            .Select(Function(xi) DirectCast([do](xi, y), TOut)) _
                            .ToArray
                    ElseIf xlist.Length = ylist.Length Then
                        Return xlist _
                            .SeqIterator _
                            .Select(Function(xi)
                                        Return DirectCast([do](CObj(xi.value), CObj(ylist(xi))), TOut)
                                    End Function) _
                            .ToArray
                    Else
                        Throw New InvalidOperationException("Vector length between the X and Y should be equals!")
                    End If
                Else
                    Throw New InvalidCastException(ytype.FullName)
                End If

            Else
                Throw New InvalidCastException(xtype.FullName)
            End If
        End Function

        ''' <summary>
        ''' Generic ``+`` operator for numeric type
        ''' </summary>
        ''' <typeparam name="TX"></typeparam>
        ''' <typeparam name="TY"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a + b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Minus(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a - b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Multiply(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a * b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Divide(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a / b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Module](Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a Mod b)
        End Function
    End Module
End Namespace

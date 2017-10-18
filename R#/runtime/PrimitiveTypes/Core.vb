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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.PrimitiveTypes

    Module Core

        Friend NotInheritable Class TypeDefine(Of T)

            Private Sub New()
            End Sub

            Shared singleType, collectionType As Type

            Public Shared Function GetSingleType() As Type
                If singleType Is Nothing Then
                    singleType = GetType(T)
                End If
                Return singleType
            End Function

            Public Shared Function GetCollectionType() As Type
                If collectionType Is Nothing Then
                    collectionType = GetType(IEnumerable(Of T))
                End If
                Return collectionType
            End Function
        End Class

        ''' <summary>
        ''' The ``In`` operator
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="collection"></param>
        ''' <returns></returns>
        Public Function op_In(Of T)(x As Object, collection As IEnumerable(Of T)) As IEnumerable(Of Boolean)
            With collection.AsList
                If x Is Nothing Then
                    Return {}
                Else
                    Dim type As Type = x.GetType

                    If type Is TypeDefine(Of T).GetSingleType Then
                        Return { .IndexOf(DirectCast(x, T)) > -1}
                    ElseIf type.ImplementsInterface(TypeDefine(Of T).GetCollectionType) Then
                        Return DirectCast(x, IEnumerable(Of T)).Select(Function(n) .IndexOf(n) > -1)
                    Else
                        Throw New InvalidOperationException(type.FullName)
                    End If
                End If
            End With
        End Function

        Public Function BuildMethodInfo(Of TX As IComparable(Of TX),
                                           TY As IComparable(Of TY), TOut)(
                                         [do] As Func(Of Object, Object, Object),
                                         castX As Func(Of Object, Object),
                                         castY As Func(Of Object, Object),
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

            Return New RMethodInfo({GetType(TX).Argv("x", 0), GetType(TY).Argv("y", 1)}, operatorCall, name)
        End Function

        Public ReadOnly op_Add As Func(Of Object, Object, Object) = Function(x, y) x + y
        Public ReadOnly op_Minus As Func(Of Object, Object, Object) = Function(x, y) x - y
        Public ReadOnly op_Multiply As Func(Of Object, Object, Object) = Function(x, y) x * y
        Public ReadOnly op_Divided As Func(Of Object, Object, Object) = Function(x, y) x / y
        Public ReadOnly op_Mod As Func(Of Object, Object, Object) = Function(x, y) x Mod y

        ''' <summary>
        ''' Generic binary operator core for numeric type.
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

            If xtype Is TypeDefine(Of TX).GetSingleType Then

                If ytype Is TypeDefine(Of TY).GetSingleType Then
                    Return {DirectCast([do](x, y), TOut)}

                ElseIf ytype.ImplementsInterface(TypeDefine(Of TY).GetCollectionType) Then
                    Return DirectCast(y, IEnumerable(Of TY)) _
                        .Select(Function(yi) DirectCast([do](x, yi), TOut)) _
                        .ToArray

                Else
                    Throw New InvalidCastException(ytype.FullName)
                End If

            ElseIf xtype.ImplementsInterface(TypeDefine(Of TX).GetCollectionType) Then

                If ytype Is TypeDefine(Of TY).GetSingleType Then
                    Return DirectCast(x, IEnumerable(Of TX)) _
                        .Select(Function(xi) DirectCast([do](xi, y), TOut)) _
                        .ToArray

                ElseIf ytype.ImplementsInterface(TypeDefine(Of TY).GetCollectionType) Then

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

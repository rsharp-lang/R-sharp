﻿#Region "Microsoft.VisualBasic::e76e8fccb6c126cdf8dbc83a65223b3b, R#\Runtime\Vectorization\Core.vb"

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

    '   Total Lines: 260
    '    Code Lines: 163 (62.69%)
    ' Comment Lines: 56 (21.54%)
    '    - Xml Docs: 82.14%
    ' 
    '   Blank Lines: 41 (15.77%)
    '     File Size: 10.40 KB


    '     Delegate Function
    ' 
    ' 
    '     Module Core
    ' 
    '         Function: BinaryCoreInternal, CreateScalarVectorInternal, op_In, safeDivided, safeModule
    '                   safeMultiply, UnaryCoreInternal, VectorAlignment
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter

Namespace Runtime.Vectorization

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x">scalar</param>
    ''' <param name="y">scalar</param>
    ''' <param name="env"></param>
    ''' <returns>this function should populate a single value result or a error message</returns>
    Public Delegate Function op_evaluator(x As Object, y As Object, env As Environment) As Object

    ''' <summary>
    ''' Operator impl core
    ''' </summary>
    Public Module Core

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

                If type Is typedefine(Of T).baseType Then

                    ' Just one element in x, using list indexof is faster than using hash table
                    Return { .IndexOf(DirectCast(x, T)) > -1}

                ElseIf type.ImplementInterface(typedefine(Of T).enumerable) Then

                    ' This can be optimised by using hash table if the x and collection are both a large collection. 
                    Dim xVector = DirectCast(x, IEnumerable(Of T)).ToArray

                    If xVector.Length > 500 AndAlso .Count > 1000 Then

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

        Public ReadOnly op_Add As Func(Of Object, Object, Object) = Function(x, y) x + y
        Public ReadOnly op_Minus As Func(Of Object, Object, Object) = Function(x, y) x - y
        Public ReadOnly op_Multiply As Func(Of Object, Object, Object) = AddressOf safeMultiply
        Public ReadOnly op_Divided As Func(Of Object, Object, Object) = AddressOf safeDivided
        Public ReadOnly op_Mod As Func(Of Object, Object, Object) = AddressOf safeModule
        Public ReadOnly op_Power As Func(Of Object, Object, Object) = Function(x, y) CDbl(x) ^ CDbl(y)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function safeModule(x As Object, y As Object) As Object
            If x = 0.0 OrElse x = 0 Then
                Return 0
            Else
                Return x Mod y
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function safeDivided(x As Object, y As Object) As Object
            If x = 0.0 OrElse x = 0 Then
                Return 0
            Else
                Return x / y
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function safeMultiply(x As Object, y As Object) As Object
            If x = 0.0 OrElse y = 0.0 OrElse x = 0 OrElse y = 0 Then
                Return 0
            Else
                Return x * y
            End If
        End Function

        ''' <summary>
        ''' Generic unary operator core for primitive type.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="[do]"></param>
        ''' <returns></returns>
        Public Function UnaryCoreInternal(Of T As IComparable(Of T), TOut)(x As Object, [do] As Func(Of Object, Object)) As Object
            Dim v As GetVectorElement = GetVectorElement.Create(Of T)(x)

            If v.Mode = VectorTypes.Error Then
                Throw v.Error
            ElseIf v.Mode = VectorTypes.Scalar Then
                Return DirectCast([do](v.single), TOut)
            ElseIf v.Mode = VectorTypes.None Then
                Return Nothing
            Else
                Return v.Populate(Of TOut)(unary:=[do]).ToArray
            End If
        End Function

        Private Function CreateScalarVectorInternal(Of TOut)(x As Object, y As Object, [do] As op_evaluator, env As Environment) As Object
            Dim result As Object = [do](x, y, env)

            If Program.isException(result) Then
                Return result
            Else
                Return New TOut() {DirectCast(result, TOut)}
            End If
        End Function

        ''' <summary>
        ''' this function just apply for the custom operator
        ''' 
        ''' [Vector core] Generic binary operator core for numeric type.
        ''' </summary>
        ''' <typeparam name="TX"></typeparam>
        ''' <typeparam name="TY"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="do"></param>
        ''' <returns>
        ''' error message or array of <typeparamref name="TOut"/>.
        ''' </returns>
        ''' <remarks>
        ''' the <paramref name="do"/> delegate function should be used for measure two scalar element value:
        ''' 
        ''' op_evaluator = function(a,b,env) {...}
        ''' </remarks>
        Public Function BinaryCoreInternal(Of TX, TY, TOut)(x As Object, y As Object, [do] As op_evaluator, env As Environment) As Object
            Dim vx As GetVectorElement = GetVectorElement.Create(Of TX)(x)
            Dim vy As GetVectorElement = GetVectorElement.Create(Of TY)(y)
            Dim result As Object

            If vx.Mode = VectorTypes.Scalar AndAlso vy.Mode = VectorTypes.Scalar Then
                Return CreateScalarVectorInternal(Of TOut)(vx.single, vy.single, [do], env)
            ElseIf vx.Mode = VectorTypes.Scalar AndAlso vy.Mode = VectorTypes.None Then
                Return CreateScalarVectorInternal(Of TOut)(vx.single, Nothing, [do], env)
            ElseIf vx.Mode = VectorTypes.None AndAlso vy.Mode = VectorTypes.Scalar Then
                Return CreateScalarVectorInternal(Of TOut)(Nothing, vy.single, [do], env)
            ElseIf vx.Mode = VectorTypes.None AndAlso vy.Mode = VectorTypes.None Then
                Return CreateScalarVectorInternal(Of TOut)(Nothing, Nothing, [do], env)
            End If

            ' 20230216 the vx and vy has been ensure that not nothing

            Dim populater As New List(Of TOut)

            If vx.Mode = VectorTypes.Scalar Then
                ' scalar do vector
                x = vx.single

                For Each yi As Object In vy.vector
                    result = [do](x, yi, env)

                    If Program.isException(result) Then
                        Return result
                    Else
                        populater.Add(DirectCast(result, TOut))
                    End If
                Next
            ElseIf vy.Mode = VectorTypes.Scalar Then
                ' vector do scalar
                y = vy.single

                For Each xi As Object In vx.vector
                    result = [do](xi, y, env)

                    If Program.isException(result) Then
                        Return result
                    Else
                        populater.Add(DirectCast(result, TOut))
                    End If
                Next
            ElseIf vx.size <> vy.size Then
                Return Internal.debug.stop({
                    $"vector length between the X({vx.size}) and Y({vy.size}) should be equals!",
                    $"sizeof_x: {vx.size}",
                    $"sizeof_y: {vy.size}"
                }, env)
            Else
                ' vector do vector
                Dim nsize As Integer = vx.size

                For i As Integer = 0 To nsize - 1
                    result = [do](vx.vector(i), vy.vector(i), env)

                    If Program.isException(result) Then
                        Return result
                    Else
                        populater.Add(DirectCast(result, TOut))
                    End If
                Next
            End If

            Return populater.ToArray
        End Function

        ''' <summary>
        ''' 将所有的数组都转换为等长的数组
        ''' </summary>
        ''' <param name="a"></param>
        ''' <returns></returns>
        Public Iterator Function VectorAlignment(ParamArray a As Array()) As IEnumerable(Of Array)
            Dim allSize As Integer() = a.Select(Function(v) v.Length).Where(Function(l) l <> 1).ToArray
            Dim alignSize As Integer

            If Not allSize.All(Function(l) l = allSize(Scan0)) Then
                Throw New InvalidCastException
            Else
                alignSize = allSize(Scan0)
            End If

            For Each v As Array In a
                If v.Length = 0 Then
                    Yield Array.CreateInstance(v.GetType.GetElementType, alignSize)
                ElseIf v.Length = 1 Then
                    Dim [single] As Object = v.GetValue(Scan0)
                    Dim full As Array = Array.CreateInstance(v.GetType.GetElementType, alignSize)

                    For i As Integer = 0 To alignSize - 1
                        full.SetValue([single], i)
                    Next

                    Yield full
                Else
                    Yield v
                End If
            Next
        End Function
    End Module
End Namespace

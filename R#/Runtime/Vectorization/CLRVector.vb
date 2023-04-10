#Region "Microsoft.VisualBasic::be51fdd26ad696db988fec612d5e2e5d, D:/GCModeller/src/R-sharp/R#//Runtime/Vectorization/CLRVector.vb"

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

    '   Total Lines: 346
    '    Code Lines: 287
    ' Comment Lines: 20
    '   Blank Lines: 39
    '     File Size: 13.54 KB


    '     Module CLRVector
    ' 
    '         Function: asCharacter, asDate, asInteger, (+2 Overloads) asLogical, asLong
    '                   asNumeric, asRawByte, castVector, parseString, safeCharacters
    '                   testNull
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ValueTypes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Vectorization

    ''' <summary>
    ''' Data cast type helper for the primitive array in CLR function code
    ''' </summary>
    Public Module CLRVector

        Public Function asDate(x As Object) As Date()
            If x Is Nothing Then
                Return Nothing
            ElseIf TypeOf x Is list Then
                x = DirectCast(x, list).data.ToArray
            End If
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If
            If x.GetType.IsArray Then
                x = REnv.UnsafeTryCastGenericArray(x)
            End If

            If TypeOf x Is Date() Then
                Return x
            ElseIf x.GetType.ImplementInterface(Of IEnumerable(Of Date)) Then
                Return DirectCast(x, IEnumerable(Of Date)).ToArray
            End If
            If TypeOf x Is String Then
                Return New Date() {Date.Parse(CStr(x))}
            ElseIf REnv.isVector(Of String)(x) Then
                Return asCharacter(x).Select(AddressOf Date.Parse).ToArray
            End If

            Return asNumeric(x) _
                .Select(Function(d) ValueTypes.FromUnixTimeStamp(d)) _
                .ToArray
        End Function

        Public Function asLong(x As Object) As Long()
            If x Is Nothing Then
                Return Nothing
            End If

            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            End If
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If

            If x.GetType.IsArray Then
                x = REnv.UnsafeTryCastGenericArray(x)
            End If
            If TypeOf x Is String Then
                Return New Long() {Long.Parse(CStr(x))}
            ElseIf REnv.isVector(Of String)(x) Then
                Return asCharacter(x).Select(AddressOf Long.Parse).ToArray
            End If

            If TypeOf x Is Long() Then
                Return x
            ElseIf DataFramework.IsNumericCollection(x.GetType) Then
                Return (From xi As Object
                        In DirectCast(x, IEnumerable).AsQueryable
                        Select CLng(xi)).ToArray
            ElseIf DataFramework.IsNumericType(x.GetType) Then
                Return New Long() {CLng(x)}
            ElseIf x.GetType.ImplementInterface(GetType(IEnumerable(Of Long))) Then
                Return DirectCast(x, IEnumerable(Of Long)).ToArray
            ElseIf TypeOf x Is Integer() OrElse TypeOf x Is List(Of Integer) Then
                Return DirectCast(x, IEnumerable(Of Integer)) _
                    .Select(Function(i) CLng(i)) _
                    .ToArray
            End If

            Throw New NotImplementedException(x.GetType.FullName)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function safeCharacters(x As Object) As String()
            Return asCharacter(x) _
                .Select(Function(i)
                            Return If(i Is Nothing, "", any.ToString(i))
                        End Function) _
                .ToArray
        End Function

        Public Function asCharacter(x As Object) As String()
            If x Is Nothing Then
                Return Nothing
            End If
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If
            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            End If

            If x.GetType.IsArray Then
                x = REnv.UnsafeTryCastGenericArray(x)
            End If

            If TypeOf x Is String Then
                Return New String() {DirectCast(x, String)}
            ElseIf TypeOf x Is String() Then
                Return x
            ElseIf x.GetType.ImplementInterface(Of IEnumerable(Of String)) Then
                Return DirectCast(x, IEnumerable(Of String)).ToArray
            ElseIf TypeOf x Is String()() Then
                Return DirectCast(x, String()()) _
                    .Select(Function(r) r(Scan0)) _
                    .ToArray
            ElseIf x.GetType.IsArray Then
                ' force cast any object to string
                Dim objs = DirectCast(x, Array) _
                    .AsObjectEnumerator _
                    .ToArray

                If objs.All(Function(o) o Is Nothing OrElse o.GetType.IsArray) Then
                    Return objs _
                        .Select(Function(a) any.ToString(DirectCast(a, Array).GetValueOrDefault(Scan0))) _
                        .ToArray
                Else
                    Return objs _
                        .Select(Function(a) any.ToString(a)) _
                        .ToArray
                End If
            Else
                ' is a single value
                Return New String() {any.ToString(x)}
            End If
        End Function

        Public Function asRawByte(x As Object) As Byte()
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If
            If TypeOf x Is Byte Then
                Return New Byte() {DirectCast(x, Byte)}
            End If

            x = REnv.UnsafeTryCastGenericArray(x)

            If TypeOf x Is Byte() Then
                Return DirectCast(x, Byte())
            End If

            Return asInteger(x) _
                .Select(Function(i8) CByte(i8)) _
                .ToArray
        End Function

        Public Function asInteger(x As Object) As Integer()
            If x Is Nothing Then
                Return Nothing
            ElseIf TypeOf x Is String Then
                Return New Integer() {CInt(Val(x))}
            End If
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If
            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            End If

            If x.GetType.IsArray Then
                x = REnv.UnsafeTryCastGenericArray(x)
            End If

            If TypeOf x Is Integer() Then
                Return x
            ElseIf DataFramework.IsNumericCollection(x.GetType) Then
                Return (From xi As Object
                        In DirectCast(x, IEnumerable).AsQueryable
                        Select CInt(xi)).ToArray
            ElseIf DataFramework.IsNumericType(x.GetType) Then
                Return New Integer() {CInt(x)}
            ElseIf x.GetType.ImplementInterface(Of IEnumerable(Of Integer)) Then
                Return DirectCast(x, IEnumerable(Of Integer)).ToArray
            ElseIf TypeOf x Is String() Then
                Return DirectCast(x, String()) _
                    .Select(Function(str) CInt(Val(str))) _
                    .ToArray
            End If

            Throw New NotImplementedException(x.GetType.FullName)
        End Function

        Public Function asNumeric(x As Object) As Double()
            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            ElseIf TypeOf x Is String Then
                Return New Double() {Double.Parse(CStr(x))}
            ElseIf TypeOf x Is TimeSpan Then
                Return New Double() {DirectCast(x, TimeSpan).TotalMilliseconds}
            ElseIf TypeOf x Is Date Then
                Return New Double() {DirectCast(x, Date).UnixTimeStamp}
            ElseIf TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If

            If x.GetType.IsArray Then
                x = REnv.UnsafeTryCastGenericArray(x)
            End If

            If TypeOf x Is Double() Then
                Return x
            ElseIf TypeOf x Is Single() Then
                Return DirectCast(x, Single()).Select(Function(s) CDbl(s)).ToArray
            ElseIf DataFramework.IsNumericCollection(x.GetType) Then
                Return (From xi As Object
                        In DirectCast(x, IEnumerable).AsQueryable
                        Select CDbl(xi)).ToArray
            ElseIf DataFramework.IsNumericType(x.GetType) Then
                ' is a single scalr value
                Return New Double() {CDbl(x)}
            ElseIf TypeOf x Is String Then
                ' parse string
                Return New Double() {Val(x)}
            ElseIf isVector(Of String)(x) Then
                ' parse string
                Return CLRVector.asCharacter(x).Select(AddressOf Val).ToArray
            ElseIf TypeOf x Is Date() Then
                Return DirectCast(x, Date()).Select(Function(d) d.UnixTimeStamp).ToArray
            ElseIf TypeOf x Is TimeSpan() Then
                Return DirectCast(x, TimeSpan()).Select(Function(d) d.TotalMilliseconds).ToArray
            ElseIf TypeOf x Is Object() Then
                Return DirectCast(x, Object()).Select(Function(d) CDbl(d)).ToArray
            Else
                Throw New InvalidCastException(x.GetType.FullName)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function asLogical(v As vector) As Boolean()
            Return asLogical(x:=CObj(v))
        End Function

        ''' <summary>
        ''' NULL -> false
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function asLogical(x As Object) As Boolean()
            If x Is Nothing Then
                Return {False}
            ElseIf TypeOf x Is Boolean Then
                Return New Boolean() {CBool(x)}
            ElseIf TypeOf x Is Boolean() Then
                Return x
            ElseIf TypeOf x Is String Then
                Return New Boolean() {CStr(x).ParseBoolean}
            End If

            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            End If

            Dim vector As Array
            Dim type As Type

            If x.GetType.IsArray Then
                vector = REnv.UnsafeTryCastGenericArray(x)
            Else
                vector = REnv.asVector(Of Object)(x)
                ' vector = REnv.UnsafeTryCastGenericArray(x)
            End If

            If vector.Length = 0 Then
                Return {}
            End If

            Dim test As Object = (From obj As Object
                                  In vector.AsQueryable
                                  Where Not obj Is Nothing).FirstOrDefault

            If Not test Is Nothing Then
                type = test.GetType
            Else
                ' all is nothing?
                Return (From obj As Object
                        In vector.AsQueryable
                        Select False).ToArray
            End If

            If type Is GetType(Boolean) Then
                Return castVector(vector)
            ElseIf DataFramework.IsNumericType(type) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(num) CDbl(num) <> 0) _
                    .ToArray
            ElseIf type Is GetType(String) Then
                Return parseString(vector)
            Else
                Return testNull(vector)
            End If
        End Function

        Private Function castVector(vector As Array) As Boolean()
            Return vector.AsObjectEnumerator _
                .Select(Function(b)
                            If b Is Nothing Then
                                Return False
                                'ElseIf TypeOf b Is String Then
                                '    Return CStr(b).ParseBoolean
                                'ElseIf TypeOf b Is Boolean Then
                            Else
                                Return DirectCast(b, Boolean)
                                'Else
                                '    Return CBool(b)
                            End If
                        End Function) _
                .ToArray
        End Function

        Private Function parseString(vector As Array) As Boolean()
            Return vector.AsObjectEnumerator _
                .Select(Function(o)
                            Return DirectCast(o, String).ParseBoolean
                        End Function) _
                .ToArray
        End Function

        Private Function testNull(vector As Array) As Boolean()
            Return vector.AsObjectEnumerator _
                .Select(Function(o)
                            If TypeOf o Is Boolean Then
                                Return CBool(o)
                            End If

                            Return Not o Is Nothing
                        End Function) _
                .ToArray
        End Function
    End Module
End Namespace

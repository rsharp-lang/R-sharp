#Region "Microsoft.VisualBasic::1ad8522d3e42e14097fdc08e9bedcbc8, R-sharp\R#\Runtime\Vectorization\CLRVector.vb"

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

    '   Total Lines: 127
    '    Code Lines: 104
    ' Comment Lines: 9
    '   Blank Lines: 14
    '     File Size: 4.86 KB


    '     Class CLRVector
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: asCharacter, asInteger, asLogical, asLong, asNumeric
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Vectorization

    ''' <summary>
    ''' Data cast type helper for the primitive array in CLR function code
    ''' </summary>
    Public NotInheritable Class CLRVector

        Private Sub New()
        End Sub

        Public Shared Function asLong(x As Object) As Long()
            If x Is Nothing Then
                Return Nothing
            End If

            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            End If

            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            ElseIf TypeOf x Is Long() Then
                Return x
            ElseIf DataFramework.IsNumericType(x.GetType) Then
                Return New Long() {CLng(x)}
            ElseIf x.GetType.ImplementInterface(GetType(IEnumerable(Of Long))) Then
                Return DirectCast(x, IEnumerable(Of Long)).ToArray
            ElseIf TypeOf x Is Integer() OrElse TypeOf x Is List(Of Integer) Then
                Return DirectCast(x, IEnumerable(Of Integer)) _
                    .Select(Function(i) CLng(i)) _
                    .ToArray
            End If

            Throw New NotImplementedException
        End Function

        Public Shared Function asCharacter(x As Object) As String()
            If x Is Nothing Then
                Return Nothing
            End If
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If
            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
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

        Public Shared Function asInteger(x As Object) As Integer()
            If x Is Nothing Then
                Return Nothing
            End If
            If TypeOf x Is vector Then
                x = DirectCast(x, vector).data
            End If
            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            End If
            If TypeOf x Is Integer() Then
                Return x
            ElseIf DataFramework.IsNumericType(x.GetType) Then
                Return New Integer() {CInt(x)}
            ElseIf x.GetType.ImplementInterface(Of IEnumerable(Of Integer)) Then
                Return DirectCast(x, IEnumerable(Of Integer)).ToArray
            End If

            Throw New NotImplementedException
        End Function

        Public Shared Function asNumeric(x As Object) As Double()
            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            End If

            If TypeOf x Is Double() Then
                Return x
            ElseIf TypeOf x Is Integer() OrElse TypeOf x Is Long() OrElse TypeOf x Is Single() OrElse TypeOf x Is Short() Then
                Return DirectCast(x, Array).AsObjectEnumerator.Select(Function(d) CDbl(d)).ToArray
            ElseIf DataFramework.IsNumericType(x.GetType) Then
                Return New Double() {CDbl(x)}
            ElseIf TypeOf x Is vector AndAlso DirectCast(x, vector).elementType Like RType.floats Then
                Return DirectCast(x, Array).AsObjectEnumerator.Select(Function(d) CDbl(d)).ToArray
            ElseIf TypeOf x Is Object() Then
                Return DirectCast(x, Object()).Select(Function(d) CDbl(d)).ToArray
            Else
                Throw New InvalidCastException(x.GetType.FullName)
            End If
        End Function

        ''' <summary>
        ''' NULL -> false
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Function asLogical(x As Object) As Boolean()
            If x Is Nothing Then
                Return {False}
            ElseIf TypeOf x Is Boolean Then
                Return New Boolean() {CBool(x)}
            ElseIf TypeOf x Is Boolean() Then
                Return x
            End If

            If TypeOf x Is list Then
                x = DirectCast(x, list).slots.Values.ToArray
            End If

            Dim vector As Array = REnv.asVector(Of Object)(x)
            Dim type As Type

            If vector.Length = 0 Then
                Return {}
            Else
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
            End If

            If type Is GetType(Boolean) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(b)
                                If b Is Nothing Then
                                    Return False
                                Else
                                    Return DirectCast(b, Boolean)
                                End If
                            End Function) _
                    .ToArray
            ElseIf DataFramework.IsNumericType(type) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(num) CDbl(num) <> 0) _
                    .ToArray
            ElseIf type Is GetType(String) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(o)
                                Return DirectCast(o, String).ParseBoolean
                            End Function) _
                    .ToArray
            Else
                Return vector.AsObjectEnumerator _
                    .Select(Function(o) Not o Is Nothing) _
                    .ToArray
            End If
        End Function
    End Class
End Namespace

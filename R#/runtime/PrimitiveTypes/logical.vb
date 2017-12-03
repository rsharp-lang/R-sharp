#Region "Microsoft.VisualBasic::b287aa337ae539471f1624adaef62704, ..\R-sharp\R#\runtime\PrimitiveTypes\logical.vb"

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
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text

Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.boolean"/>
    ''' </summary>
    Public Class logical : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.boolean, GetType(Boolean))
            Call MyBase.[New]()

            MyBase.UnaryOperators("not") = Core.BuildMethodInfo(Of Boolean, Boolean)(Function(b) Not b, "not")
            MyBase.BinaryOperator1("and") = New BinaryOperator("and", BuildIntegerMethodInfo1(Function(x, y) x AndAlso y, "and"))
            MyBase.BinaryOperator2("and") = New BinaryOperator("and", BuildIntegerMethodInfo1(Function(x, y) x AndAlso y, "and"))
            MyBase.BinaryOperator1("or") = New BinaryOperator("or", BuildIntegerMethodInfo1(Function(x, y) x OrElse y, "or"))
            MyBase.BinaryOperator2("or") = New BinaryOperator("or", BuildIntegerMethodInfo1(Function(x, y) x OrElse y, "or"))
        End Sub

        Private Shared Function AsLogical(n As Object) As Boolean
            If n = 0 Then
                Return False
            ElseIf n = 0R Then
                Return False
            Else
                Return True
            End If
        End Function

        Public Shared Function BuildIntegerMethodInfo1(op As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo()
            Dim ii = BuildMethodInfo(Of Boolean, Integer, Boolean)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Integer)).Select(Function(i) AsLogical(i)).ToArray)
            Dim id = BuildMethodInfo(Of Boolean, Double, Boolean)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Double)).Select(Function(i) AsLogical(i)).ToArray)
            Dim ib = BuildMethodInfo(Of Boolean, Boolean, Boolean)(op, Nothing, Nothing)
            Dim ic = BuildMethodInfo(Of Boolean, Char, Boolean)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Char)).CodeArray.Select(Function(i) AsLogical(i)).ToArray)
            Dim iu = BuildMethodInfo(Of Boolean, ULong, Boolean)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of ULong)).Select(Function(i) AsLogical(i)).ToArray)

            Return {ii, id, ib, ic, iu}
        End Function

        Public Shared Function BuildIntegerMethodInfo2(op As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo()
            Dim ii = BuildMethodInfo(Of Integer, Boolean, Boolean)(op, Function(x) DirectCast(x, IEnumerable(Of Integer)).Select(Function(i) AsLogical(i)).ToArray, Nothing)
            Dim id = BuildMethodInfo(Of Double, Boolean, Boolean)(op, Function(x) DirectCast(x, IEnumerable(Of Double)).Select(Function(i) AsLogical(i)).ToArray, Nothing)
            Dim ib = BuildMethodInfo(Of Boolean, Boolean, Boolean)(op, Nothing, Nothing)
            Dim ic = BuildMethodInfo(Of Char, Boolean, Boolean)(op, Function(x) DirectCast(x, IEnumerable(Of Char)).CodeArray.Select(Function(i) AsLogical(i)).ToArray, Nothing)
            Dim iu = BuildMethodInfo(Of ULong, Boolean, Boolean)(op, Function(x) DirectCast(x, IEnumerable(Of ULong)).Select(Function(i) AsLogical(i)).ToArray, Nothing)

            Return {ii, id, ib, ic, iu}
        End Function

        Public Overrides Function ToString() As String
            Return "R# logical"
        End Function
    End Class
End Namespace

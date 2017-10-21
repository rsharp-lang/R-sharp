#Region "Microsoft.VisualBasic::31ffaef4b226f2112ebe0e4b26d6e230, ..\R-sharp\R#\runtime\PrimitiveTypes\integer.vb"

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
    ''' <see cref="TypeCodes.integer"/>
    ''' </summary>
    Public Class [integer] : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.integer, GetType(Integer))
            Call MyBase.[New]()

            BinaryOperator1("+") = New BinaryOperator("+", [integer].BuildIntegerMethodInfo1(op_Add, "+"))
            BinaryOperator1("-") = New BinaryOperator("-", [integer].BuildIntegerMethodInfo1(op_Minus, "-"))
            BinaryOperator1("*") = New BinaryOperator("*", [integer].BuildIntegerMethodInfo1(op_Multiply, "*"))
            BinaryOperator1("/") = New BinaryOperator("/", [integer].BuildIntegerMethodInfo1(op_Divided, "/"))
            BinaryOperator1("%") = New BinaryOperator("%", [integer].BuildIntegerMethodInfo1(op_Mod, "%"))

            BinaryOperator2("+") = New BinaryOperator("+", [integer].BuildIntegerMethodInfo2(op_Add, "+"))
            BinaryOperator2("-") = New BinaryOperator("-", [integer].BuildIntegerMethodInfo2(op_Minus, "-"))
            BinaryOperator2("*") = New BinaryOperator("*", [integer].BuildIntegerMethodInfo2(op_Multiply, "*"))
            BinaryOperator2("/") = New BinaryOperator("/", [integer].BuildIntegerMethodInfo2(op_Divided, "/"))
            BinaryOperator2("%") = New BinaryOperator("%", [integer].BuildIntegerMethodInfo2(op_Mod, "%"))
        End Sub

        Public Overrides Function ToString() As String
            Return "R# integer"
        End Function

        Public Shared Function BuildIntegerMethodInfo2(op As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo()
            Dim ii = BuildMethodInfo(Of Integer, Integer, Integer)(op, Nothing, Nothing)
            Dim id = BuildMethodInfo(Of Double, Integer, Double)(op, Nothing, Nothing)
            Dim ib = BuildMethodInfo(Of Integer, Integer, Integer)(op, Function(x) DirectCast(x, IEnumerable(Of Boolean)).Select(Function(b) If(b, 1, 0)).ToArray, Nothing, fakeX:=GetType(Boolean))
            Dim ic = BuildMethodInfo(Of Integer, Integer, Integer)(op, Function(x) DirectCast(x, IEnumerable(Of Char)).CodeArray, Nothing, fakeX:=GetType(Char))
            Dim iu = BuildMethodInfo(Of ULong, Integer, Integer)(op, Nothing, Nothing)

            Return {ii, id, ib, ic, iu}
        End Function

        Public Shared Function BuildIntegerMethodInfo1(op As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo()
            Dim ii = BuildMethodInfo(Of Integer, Integer, Integer)(op, Nothing, Nothing)
            Dim id = BuildMethodInfo(Of Integer, Double, Double)(op, Nothing, Nothing)
            Dim ib = BuildMethodInfo(Of Integer, Integer, Integer)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Boolean)).Select(Function(b) If(b, 1, 0)).ToArray, fakeY:=GetType(Boolean))
            Dim ic = BuildMethodInfo(Of Integer, Integer, Integer)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Char)).CodeArray, fakeY:=GetType(Char))
            Dim iu = BuildMethodInfo(Of Integer, ULong, Integer)(op, Nothing, Nothing)

            Return {ii, id, ib, ic, iu}
        End Function
    End Class
End Namespace

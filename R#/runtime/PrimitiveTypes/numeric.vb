Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text

Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.double"/>
    ''' </summary>
    Public Class numeric : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.double, GetType(Double))
            Call MyBase.[New]()

            BinaryOperator1("+") = New BinaryOperator("+", numeric.BuildIntegerMethodInfo1(op_Add, "+"))
            BinaryOperator1("-") = New BinaryOperator("-", numeric.BuildIntegerMethodInfo1(op_Minus, "-"))
            BinaryOperator1("*") = New BinaryOperator("*", numeric.BuildIntegerMethodInfo1(op_Multiply, "*"))
            BinaryOperator1("/") = New BinaryOperator("/", numeric.BuildIntegerMethodInfo1(op_Divided, "/"))
            BinaryOperator1("%") = New BinaryOperator("%", numeric.BuildIntegerMethodInfo1(op_Mod, "%"))

            BinaryOperator2("+") = New BinaryOperator("+", numeric.BuildIntegerMethodInfo2(op_Add, "+"))
            BinaryOperator2("-") = New BinaryOperator("-", numeric.BuildIntegerMethodInfo2(op_Minus, "-"))
            BinaryOperator2("*") = New BinaryOperator("*", numeric.BuildIntegerMethodInfo2(op_Multiply, "*"))
            BinaryOperator2("/") = New BinaryOperator("/", numeric.BuildIntegerMethodInfo2(op_Divided, "/"))
            BinaryOperator2("%") = New BinaryOperator("%", numeric.BuildIntegerMethodInfo2(op_Mod, "%"))
        End Sub

        Public Overrides Function ToString() As String
            Return "R# numeric"
        End Function

        Public Shared Function BuildIntegerMethodInfo2(op As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo()
            Dim ii = BuildMethodInfo(Of Integer, Double, Double)(op, Nothing, Nothing)
            Dim id = BuildMethodInfo(Of Double, Double, Double)(op, Nothing, Nothing)
            Dim ib = BuildMethodInfo(Of Boolean, Double, Double)(op, Function(x) DirectCast(x, IEnumerable(Of Boolean)).Select(Function(b) If(b, 1, 0)).ToArray, Nothing)
            Dim ic = BuildMethodInfo(Of Char, Double, Double)(op, Function(x) DirectCast(x, IEnumerable(Of Char)).CodeArray, Nothing)
            Dim iu = BuildMethodInfo(Of ULong, Double, Double)(op, Nothing, Nothing)

            Return {ii, id, ib, ic, iu}
        End Function

        Public Shared Function BuildIntegerMethodInfo1(op As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo()
            Dim ii = BuildMethodInfo(Of Double, Integer, Double)(op, Nothing, Nothing)
            Dim id = BuildMethodInfo(Of Double, Double, Double)(op, Nothing, Nothing)
            Dim ib = BuildMethodInfo(Of Double, Boolean, Double)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Boolean)).Select(Function(b) If(b, 1, 0)).ToArray)
            Dim ic = BuildMethodInfo(Of Double, Char, Double)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Char)).CodeArray)
            Dim iu = BuildMethodInfo(Of Double, ULong, Double)(op, Nothing, Nothing)

            Return {ii, id, ib, ic, iu}
        End Function
    End Class
End Namespace
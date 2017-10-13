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

        Public Shared Function BuildIntegerMethodInfo2(op As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo()
            Dim ii = BuildMethodInfo(Of Integer, Integer, Integer)(op, Nothing, Nothing)
            Dim id = BuildMethodInfo(Of Double, Integer, Double)(op, Nothing, Nothing)
            Dim ib = BuildMethodInfo(Of Boolean, Integer, Integer)(op, Function(x) DirectCast(x, IEnumerable(Of Boolean)).Select(Function(b) If(b, 1, 0)).ToArray, Nothing)
            Dim ic = BuildMethodInfo(Of Char, Integer, Integer)(op, Function(x) DirectCast(x, IEnumerable(Of Char)).CodeArray, Nothing)
            Dim iu = BuildMethodInfo(Of ULong, Integer, Integer)(op, Nothing, Nothing)

            Return {ii, id, ib, ic, iu}
        End Function

        Public Shared Function BuildIntegerMethodInfo1(op As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing) As RMethodInfo()
            Dim ii = BuildMethodInfo(Of Integer, Integer, Integer)(op, Nothing, Nothing)
            Dim id = BuildMethodInfo(Of Integer, Double, Double)(op, Nothing, Nothing)
            Dim ib = BuildMethodInfo(Of Integer, Boolean, Integer)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Boolean)).Select(Function(b) If(b, 1, 0)).ToArray)
            Dim ic = BuildMethodInfo(Of Integer, Char, Integer)(op, Nothing, Function(y) DirectCast(y, IEnumerable(Of Char)).CodeArray)
            Dim iu = BuildMethodInfo(Of Integer, ULong, Integer)(op, Nothing, Nothing)

            Return {ii, id, ib, ic, iu}
        End Function

        Public Shared Function BuildMethodInfo(Of TX As IComparable(Of TX),
                                                  TY As IComparable(Of TY), TOut)(
                                                [do] As Func(Of Object, Object, Object),
                                                castX As Func(Of Object, Object),
                                                castY As Func(Of Object, Object),
                                                <CallerMemberName> Optional name$ = Nothing) As RMethodInfo
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
    End Class
End Namespace
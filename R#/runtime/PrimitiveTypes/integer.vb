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

            BinaryOperator1("+") = New BinaryOperator("+", [integer].Add1)
            BinaryOperator2("+") = New BinaryOperator("+", [integer].Add2)
            BinaryOperator1("*") = New BinaryOperator("*", [integer].Multiply1)
        End Sub

        Public Shared Function Add1() As RMethodInfo()
            Dim ii As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Integer).Argv("y", 1)}, Function(x, y) Core.Add(Of Integer, Integer, Integer)(x, y))
            Dim id As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Double).Argv("y", 1)}, Function(x, y) Core.Add(Of Integer, Double, Double)(x, y))
            Dim ib As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Boolean).Argv("y", 1)}, Function(x, y) Core.Add(Of Integer, Boolean, Integer)(x, y))
            Dim ic As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Char).Argv("y", 1)}, Function(x, y) Core.Add(Of Integer, Integer, Integer)(x, DirectCast(y, IEnumerable(Of Char)).CodeArray))
            Dim iu As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(ULong).Argv("y", 1)}, Function(x, y) Core.Add(Of Integer, ULong, Integer)(x, y))

            Return {ii, id, ib, ic, iu}
        End Function

        Public Shared Function Add2() As RMethodInfo()
            Dim ii As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Integer).Argv("y", 1)}, Function(x, y) Core.Add(Of Integer, Integer, Integer)(x, y))
            Dim di As New RMethodInfo({GetType(Double).Argv("x", 0), GetType(Integer).Argv("y", 1)}, Function(x, y) Core.Add(Of Double, Integer, Double)(x, y))
            Dim bi As New RMethodInfo({GetType(Boolean).Argv("x", 0), GetType(Integer).Argv("y", 1)}, Function(x, y) Core.Add(Of Boolean, Integer, Integer)(x, y))
            Dim ci As New RMethodInfo({GetType(Char).Argv("x", 0), GetType(Integer).Argv("y", 1)}, Function(x, y) Core.Add(Of Integer, Integer, Integer)(DirectCast(x, IEnumerable(Of Char)).CodeArray, y))
            Dim ui As New RMethodInfo({GetType(ULong).Argv("x", 0), GetType(Integer).Argv("y", 1)}, Function(x, y) Core.Add(Of ULong, Integer, Integer)(x, y))

            Return {ii, di, bi, ci, ui}
        End Function

        Public Shared Function Multiply1() As RMethodInfo()
            Dim ii As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Integer).Argv("y", 1)}, Function(x, y) Core.Multiply(Of Integer, Integer, Integer)(x, y))
            Dim id As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Double).Argv("y", 1)}, Function(x, y) Core.Multiply(Of Integer, Double, Double)(x, y))
            Dim ib As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Boolean).Argv("y", 1)}, Function(x, y) Core.Multiply(Of Integer, Boolean, Integer)(x, y))
            Dim ic As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(Char).Argv("y", 1)}, Function(x, y) Core.Multiply(Of Integer, Integer, Integer)(x, DirectCast(y, IEnumerable(Of Char)).CodeArray))
            Dim iu As New RMethodInfo({GetType(Integer).Argv("x", 0), GetType(ULong).Argv("y", 1)}, Function(x, y) Core.Multiply(Of Integer, ULong, Integer)(x, y))

            Return {ii, id, ib, ic, iu}
        End Function
    End Class
End Namespace
Imports System.Runtime.CompilerServices

Namespace Interpreter.ExecuteEngine.Linq

    Module LinqOutput

        <Extension>
        Public Function getOutputSort(linq As LinqExpression) As (indexBy As Expression, desc As Boolean)
            Dim sort As FunctionInvoke = linq.output.Where(Function(fun) fun.funcName.ToString = "sort").FirstOrDefault

            If sort Is Nothing Then
                Return Nothing
            Else
                Return (sort.parameters(Scan0), DirectCast(sort.parameters(1).Evaluate(Nothing), Boolean))
            End If
        End Function

        <Extension>
        Public Function populateOutput(linq As LinqExpression, result As List(Of LinqOutputUnit)) As Object
            Dim sort = linq.getOutputSort

            If sort.indexBy Is Nothing Then
                Return result.ToDictionary(Function(a) a.key, Function(a) a.value)
            ElseIf sort.desc Then
                Return result.OrderByDescending(Function(a) a.sortKey).ToDictionary(Function(a) a.key, Function(a) a.value)
            Else
                Return result.OrderBy(Function(a) a.sortKey).ToDictionary(Function(a) a.key, Function(a) a.value)
            End If
        End Function
    End Module

    Friend Class LinqOutputUnit

        Public key As String
        Public sortKey As Object
        Public value As Object

        Public Overrides Function ToString() As String
            Return $"[{key}] {Scripting.ToString(value, "null")}"
        End Function

    End Class
End Namespace
Namespace Interpreter.ExecuteEngine.LINQ

    Friend Class LinqOutputUnit

        Public key As String
        Public sortKey As Object
        Public value As Object

        Public Overrides Function ToString() As String
            Return $"[{key}] {Scripting.ToString(value, "null")}"
        End Function

    End Class
End Namespace
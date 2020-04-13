Namespace Runtime.Interop

    Public Delegate Function IBinaryOperator(left As Object, right As Object, env As Environment) As Object

    Public Class BinaryOperator

        Public Property operatorSymbol As String
        Public Property left As RType
        Public Property right As RType

        ReadOnly operation As IBinaryOperator

        Sub New(op As IBinaryOperator)
            operation = op
        End Sub

        Public Overrides Function ToString() As String
            Return $"({left} {operatorSymbol} {right})"
        End Function

    End Class
End Namespace
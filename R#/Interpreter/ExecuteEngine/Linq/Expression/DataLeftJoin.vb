Namespace Interpreter.ExecuteEngine.LINQ

    Public Class DataLeftJoin : Inherits LinqKeywordExpression

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "JOIN"
            End Get
        End Property

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
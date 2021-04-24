Namespace Interpreter.ExecuteEngine.LINQ

    Public Class VectorLiteral : Inherits Expression

        Public Overrides ReadOnly Property name As String
            Get
                Return "[...]"
            End Get
        End Property

        Public Property elements As Expression()

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
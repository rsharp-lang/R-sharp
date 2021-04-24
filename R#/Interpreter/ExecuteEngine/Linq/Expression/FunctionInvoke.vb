Namespace Interpreter.ExecuteEngine.LINQ

    Public Class FunctionInvoke : Inherits Expression

        Public Overrides ReadOnly Property name As String
            Get
                Return "func()"
            End Get
        End Property

        Public Property parameters As Expression()
        Public Property func As Expression

        Sub New(name As Expression, params As Expression())
            func = name
            parameters = params
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
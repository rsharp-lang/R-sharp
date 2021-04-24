Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class ValueAssign : Inherits Expression

        Public Overrides ReadOnly Property name As String
            Get
                Return "a <- x"
            End Get
        End Property

        Friend symbolName As String
        Friend value As RExpression

        Sub New(symbolName As String, value As RExpression)
            Me.value = value
            Me.symbolName = symbolName
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
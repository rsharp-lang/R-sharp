Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Components

    Public Class InvokeParameter

        Public Property value As Expression

        Public ReadOnly Property name As String
            Get
                If value Is Nothing Then
                    Return ""
                ElseIf TypeOf value Is SymbolReference Then
                    Return DirectCast(value, SymbolReference).symbol
                Else
                    Return value.ToString
                End If
            End Get
        End Property

        Public Function Evaluate(envir As Environment) As Object
            Return value.Evaluate(envir)
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class
End Namespace
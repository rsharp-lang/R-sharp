
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' data filter: ``WHERE &lt;condition>``
    ''' </summary>
    Public Class WhereFilter : Inherits LinqKeywordExpression

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "WHERE"
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
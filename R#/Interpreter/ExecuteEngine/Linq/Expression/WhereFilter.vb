
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' data filter: ``WHERE &lt;condition>``
    ''' </summary>
    Public Class WhereFilter : Inherits LinqKeywordExpression

        Dim filter As Expression

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "WHERE"
            End Get
        End Property

        Sub New(filter As Expression)
            Me.filter = filter
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
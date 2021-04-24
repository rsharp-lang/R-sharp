Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' aggregate ... into ...
    ''' </summary>
    Public Class AggregateExpression : Inherits QueryExpression

        Public Overrides ReadOnly Property name As String
            Get
                Return "aggregate ... into ..."
            End Get
        End Property

        Sub New(symbol As SymbolDeclare, sequence As Expression, exec As IEnumerable(Of Expression))
            Call MyBase.New(symbol, sequence, exec)
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
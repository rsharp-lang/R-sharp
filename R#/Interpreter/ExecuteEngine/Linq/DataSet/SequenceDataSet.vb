Namespace Interpreter.ExecuteEngine.LINQ

    Public Class SequenceDataSet : Inherits DataSet

        Public Property sequence As Object

        Friend Overrides Function PopulatesData() As IEnumerable(Of Object)
            Throw New NotImplementedException()
        End Function
    End Class

End Namespace
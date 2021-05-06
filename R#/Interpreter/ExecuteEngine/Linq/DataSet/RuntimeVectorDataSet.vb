Namespace Interpreter.ExecuteEngine.LINQ

    Public Class RuntimeVectorDataSet : Inherits DataSet

        ReadOnly vector As Array

        Sub New(any As Array)
            vector = any
        End Sub

        Friend Overrides Iterator Function PopulatesData() As IEnumerable(Of Object)
            For i As Integer = 0 To vector.Length - 1
                Yield vector.GetValue(i)
            Next
        End Function
    End Class
End Namespace
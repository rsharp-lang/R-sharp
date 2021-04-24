Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' populate by rows
    ''' </summary>
    Public Class DataFrameDataSet : Inherits DataSet

        Public Property dataframe As dataframe

        ''' <summary>
        ''' populate a list of javascript object
        ''' </summary>
        ''' <returns></returns>
        Friend Overrides Iterator Function PopulatesData() As IEnumerable(Of Object)
            Dim nrows As Integer = dataframe.nrows

            For i As Integer = 0 To nrows

            Next
        End Function
    End Class

End Namespace
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the linq dataset object, a helper module for 
    ''' iterates through the data sequence that produced
    ''' by the <see cref="QueryExpression"/>
    ''' </summary>
    Public MustInherit Class DataSet

        Friend MustOverride Function PopulatesData() As IEnumerable(Of Object)

        Friend Shared Function CreateDataSet(queryExpression As QueryExpression, context As ExecutableContext) As DataSet
            Dim result As Object = queryExpression.GetSeqValue(context)

            If result Is Nothing Then
                Return New ErrorDataSet With {.message = Internal.debug.stop("null query sequence data!", context)}
            ElseIf TypeOf result Is Message Then
                Return New ErrorDataSet With {.message = result}
            ElseIf TypeOf result Is dataframe Then
                Return New DataFrameDataSet With {.dataframe = result}
            Else
                Return New SequenceDataSet With {.sequence = result}
            End If
        End Function
    End Class
End Namespace
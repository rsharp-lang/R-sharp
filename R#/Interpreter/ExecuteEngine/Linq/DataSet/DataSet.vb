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

    Public Class SequenceDataSet : Inherits DataSet

        Public Property sequence As Object

        Friend Overrides Function PopulatesData() As IEnumerable(Of Object)
            Throw New NotImplementedException()
        End Function
    End Class

    Public Class ErrorDataSet : Inherits DataSet

        Public Property message As Message

        Friend Overrides Iterator Function PopulatesData() As IEnumerable(Of Object)
        End Function
    End Class

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
            Throw New NotImplementedException()
        End Function
    End Class

End Namespace
Namespace Interpreter.ExecuteEngine.LINQ

    Public MustInherit Class QueryExpression : Inherits Expression

        ReadOnly sequence As Expression

        Friend ReadOnly symbol As SymbolDeclare
        Friend ReadOnly executeQueue As Expression()

        Sub New(symbol As SymbolDeclare, sequence As Expression, execQueue As IEnumerable(Of Expression))
            Me.symbol = symbol
            Me.sequence = sequence
            Me.executeQueue = execQueue.ToArray
        End Sub

        ''' <summary>
        ''' get sequence value
        ''' 
        ''' evaluate expression for get ``IN ...`` data source
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Public Function GetSeqValue(context As ExecutableContext) As Object
            Return sequence.Exec(context)
        End Function

        ''' <summary>
        ''' get data source iterator for query
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Protected Function GetDataSet(context As ExecutableContext) As DataSet
            Return DataSet.CreateDataSet(Me, context)
        End Function
    End Class
End Namespace
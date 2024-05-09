Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.AprioriRules
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' apriori: Mining Associations with the Apriori Algorithm
''' </summary>
<Package("apriori")>
Module aprioriRules

    <ExportAPI("transactions")>
    <RApiReturn(GetType(Transaction))>
    Public Function load_transactions(<RListObjectArgument> <RLazyExpression> args As list, Optional env As Environment = Nothing) As Object
        Dim trans As New List(Of Transaction)

        For Each name As String In args.getNames
            Dim exp As Expression = args.getByName(name)

            If TypeOf exp Is VectorLiteral Then
                Dim vec = DirectCast(exp, VectorLiteral).ToArray
                Dim items As String() = vec.Select(Function(e) ValueAssignExpression.GetSymbol(e)).ToArray
                trans.Add(New Transaction(name, items))
            ElseIf TypeOf exp Is SymbolReference Then
                Dim item As String = ValueAssignExpression.GetSymbol(exp)
                trans.Add(New Transaction(name, {item}))
            Else
                Throw New NotImplementedException
            End If
        Next

        Return trans.ToArray
    End Function

    ''' <summary>
    ''' apriori: Mining Associations with the Apriori Algorithm
    ''' </summary>
    ''' <param name="data">
    ''' object of class transactions. Any data structure which can be coerced into 
    ''' transactions (e.g., a binary matrix, a data.frame or a tibble) can also 
    ''' be specified and will be internally coerced to transactions.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>Returns an object of class rules or itemsets.</returns>
    <ExportAPI("apriori")>
    Public Function apriori(<RRawVectorArgument> data As Object, Optional env As Environment = Nothing) As Object
        Dim trans As pipeline = pipeline.TryCreatePipeline(Of Transaction)(data, env)

        If trans.isError Then
            Return trans.getError
        End If

        Dim transList As Transaction() = trans.populates(Of Transaction)(env).ToArray
        Dim rules = transList.AnalysisTransactions


    End Function

End Module

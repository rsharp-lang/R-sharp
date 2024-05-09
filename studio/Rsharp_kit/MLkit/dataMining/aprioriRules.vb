Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.Scripting.MetaData
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
    Public Function load_transactions(<RListObjectArgument> args As list, Optional env As Environment = Nothing) As Object

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

    End Function

End Module

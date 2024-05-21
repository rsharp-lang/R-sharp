#Region "Microsoft.VisualBasic::76890bfcc15efcca8182ada571828ba5, studio\Rsharp_kit\MLkit\dataMining\aprioriRules.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 128
    '    Code Lines: 84 (65.62%)
    ' Comment Lines: 26 (20.31%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 18 (14.06%)
    '     File Size: 5.37 KB


    ' Module aprioriRules
    ' 
    '     Function: apriori, load_transactions, niceTable, parse_transactions
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.AprioriRules
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' apriori: Mining Associations with the Apriori Algorithm
''' </summary>
''' <remarks>
''' In data mining, Apriori is a classic algorithm for learning association rules. 
''' Apriori Is designed To operate On databases containing transactions 
''' (for example, collections of items bought by customers, Or details of a website frequentation).
''' Other algorithms are designed For finding association rules In data having no transactions 
''' (Winepi And Minepi), Or having no timestamps (DNA sequencing).
''' </remarks>
<Package("apriori")>
Module aprioriRules

    Sub Main()
        Internal.Object.Converts.makeDataframe.addHandler(GetType(Output), AddressOf niceTable)
    End Sub

    <RGenericOverloads("as.data.frame")>
    Public Function niceTable(out As Output, args As list, env As Environment) As dataframe
        Dim df As New dataframe With {.columns = New Dictionary(Of String, Array)}
        Dim rules = out.StrongRules.ToArray

        Call df.add("lhs", rules.Select(Function(r) r.X.ToString))
        Call df.add("rhs", rules.Select(Function(r) r.Y.ToString))
        Call df.add("support(XY)", rules.Select(Function(r) r.SupportXY))
        Call df.add("support(X)", rules.Select(Function(r) r.SupportX))
        Call df.add("support", rules.Select(Function(r) r.SupportXY / out.TransactionSize))
        Call df.add("confidence", rules.Select(Function(r) r.Confidence))

        Return df
    End Function

    <ExportAPI("parse_transactions")>
    <RApiReturn(GetType(Transaction))>
    Public Function parse_transactions(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim trans As New List(Of Transaction)
        Dim id As i32 = 1

        For Each line As String In CLRVector.asCharacter(x).SafeQuery
            Call trans.Add(New Transaction With {
                .Name = ++id,
                .Items = line _
                    .StringSplit("; ", True) _
                    .Where(Function(s) s.Length > 0) _
                    .ToArray
            })
        Next

        Return trans.ToArray
    End Function

    ''' <summary>
    ''' create the transaction data
    ''' </summary>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
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
    <RApiReturn(GetType(Output))>
    Public Function apriori(<RRawVectorArgument> data As Object,
                            Optional support As Double = 0.01,
                            Optional confidence As Double = 0.01,
                            Optional minlen As Integer = 2,
                            Optional env As Environment = Nothing) As Object

        Dim trans As pipeline = pipeline.TryCreatePipeline(Of Transaction)(data, env)

        If trans.isError Then
            Return trans.getError
        End If

        Dim transList As Transaction() = trans.populates(Of Transaction)(env).ToArray
        Dim rules = transList.AnalysisTransactions(
            minSupport:=support,
            minConfidence:=confidence,
            minlen:=minlen)

        Return rules
    End Function

End Module

#Region "Microsoft.VisualBasic::def13cd2f7d5e32521a3173267fdb522, R#\Runtime\Internal\internalInvokes\Linq\dataframe.vb"

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

    '   Total Lines: 206
    '    Code Lines: 131
    ' Comment Lines: 49
    '   Blank Lines: 26
    '     File Size: 8.60 KB


    '     Module dataframe_methods
    ' 
    '         Function: colMeans, colSums, eval, rank_unique, rename
    '                   rowMeans, rowSums
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Linq
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports anys = Microsoft.VisualBasic.Scripting
Imports expr = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

Namespace Runtime.Internal.Invokes.LinqPipeline

    ''' <summary>
    ''' some common dataframe operations
    ''' </summary>
    Module dataframe_methods

        <ExportAPI("aggregate")>
        Public Function eval(x As dataframe, <RLazyExpression> expr As expr, Optional env As Environment = Nothing) As Object
            Dim symbols = SymbolAnalysis.GetSymbolReferenceList(expr).ToArray

            For Each ref As NamedValue(Of PropertyAccess) In symbols
                If x.hasName(ref.Name) Then
                    Call env.AssignSymbol(ref.Name, x(ref.Name))
                End If
            Next

            Dim result As Object = expr.Evaluate(env)
            Return result
        End Function

        ''' <summary>
        ''' colSums: Form Row and Column Sums and Means
        ''' 
        ''' Form row and column sums and means for numeric arrays (or data frames).
        ''' </summary>
        ''' <param name="x">
        ''' an array of two or more dimensions, containing numeric, 
        ''' complex, integer or logical values, or a numeric data 
        ''' frame. For .colSums() etc, a numeric, integer or logical 
        ''' matrix (or vector of length m * n).
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("colSums")>
        Public Function colSums(x As dataframe, Optional env As Environment = Nothing) As vector
            Dim names As String() = x.colnames
            Dim vec As Double() = names _
                .Select(Function(v)
                            ' get the column vector, and then evaluate the sum of the column
                            Return CLRVector.asNumeric(x.columns(v)).Sum
                        End Function) _
                .ToArray

            Return New vector(names, vec, env)
        End Function

        <ExportAPI("colMeans")>
        Public Function colMeans(x As dataframe, Optional env As Environment = Nothing) As vector
            Dim names As String() = x.colnames
            Dim vec As Double() = names _
                .Select(Function(v)
                            ' get the column vector, and then evaluate the average of the column
                            Return CLRVector.asNumeric(x.columns(v)).Average
                        End Function) _
                .ToArray

            Return New vector(names, vec, env)
        End Function

        <ExportAPI("rowSums")>
        Public Function rowSums(x As dataframe, Optional env As Environment = Nothing) As vector
            Dim sums = x.forEachRow _
                .Select(Function(r) CLRVector.asNumeric(r.value)) _
                .Select(Function(r) r.Sum) _
                .ToArray
            Dim v As New vector(x.rownames, sums, env)

            Return v
        End Function

        <ExportAPI("rowMeans")>
        Public Function rowMeans(x As dataframe, Optional env As Environment = Nothing) As vector
            Dim avgs = x.forEachRow _
                .Select(Function(r) CLRVector.asNumeric(r.value)) _
                .Select(Function(r) r.Average) _
                .ToArray
            Dim v As New vector(x.rownames, avgs, env)

            Return v
        End Function

        ''' <summary>
        ''' renames the dataframe object its specific column fields
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="renames">
        ''' a collection of name mapping lambda, liked: ``a -> b``
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' this function will returns nothing if the given dataframe object is nothing
        ''' </returns>
        <ExportAPI("rename")>
        Public Function rename(x As dataframe,
                               <RListObjectArgument>
                               Optional renames As list = Nothing,
                               Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return Nothing
            End If

            Dim rownameCopy As String() = If(x.rownames.IsNullOrEmpty, Nothing, x.rownames.ToArray)
            Dim copy As New dataframe With {
                .rownames = rownameCopy,
                .columns = New Dictionary(Of String, Array)(x.columns)
            }
            Dim newName As String
            Dim oldName As String

            ' df |> rename(
            '   old1 -> new1,
            '   old2 -> new2
            ' )

            For Each nameMap As NamedValue(Of Object) In renames.namedValues
                If TypeOf nameMap.Value Is DeclareLambdaFunction Then
                    Dim lambda As DeclareLambdaFunction = nameMap.Value

                    oldName = lambda.parameterNames(Scan0)
                    newName = ValueAssignExpression.GetSymbol(lambda.closure)
                Else
                    ' new = old
                    newName = nameMap.Name
                    oldName = anys.ToString(nameMap.Value)
                End If

                copy.columns(newName) = copy.columns(oldName)
                copy.columns.Remove(oldName)
            Next

            Return copy
        End Function

        ''' <summary>
        ''' make rank unique of the element rows inside a dataframe
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="duplicates">
        ''' the column field name which contains the duplictaed keys for the element rows.
        ''' </param>
        ''' <param name="ranking">the column field name for the ranking score or 
        ''' a numeric vector of the ranking scores for each corresponding element
        ''' rows.</param>
        ''' <param name="env"></param>
        ''' <returns>a new dataframe object with duplicated rows removed</returns>
        <ExportAPI("rank_unique")>
        Public Function rank_unique(x As dataframe, duplicates As String, <RRawVectorArgument> ranking As Object, Optional env As Environment = Nothing) As Object
            Dim ranking_str As String() = CLRVector.asCharacter(ranking)
            Dim ranks As Double()

            If ranking_str.IsNullOrEmpty Then
                Return Internal.debug.stop("the required of the ranking score should not be nothing!", env)
            End If
            If ranking_str.Length = 1 Then
                ' is column field name
                ranks = CLRVector.asNumeric(x(ranking_str(0)))
            Else
                ranks = CLRVector.asNumeric(ranking_str)
            End If

            Dim cols As String() = x.colnames
            Dim ordinal As Integer = cols.IndexOf(duplicates)
            Dim groups = x.forEachRow() _
                .Select(Function(r, i) (row:=r, score:=ranks(i))) _
                .GroupBy(Function(r)
                             ' group by duplicated key
                             Return anys.ToString(r.Item1(ordinal))
                         End Function) _
                .Select(Function(r)
                            ' get the top score item for make unique row
                            Return r.OrderByDescending(Function(t) t.score).First.row
                        End Function) _
                .ToArray
            Dim df As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = groups.Keys(distinct:=False)
            }
            Dim v As Object()

            For i As Integer = 0 To cols.Length - 1
                ordinal = i
                v = groups _
                    .Select(Function(r) r(ordinal)) _
                    .ToArray
                df.add(cols(i), UnsafeTryCastGenericArray(v))
            Next

            Return df
        End Function
    End Module
End Namespace

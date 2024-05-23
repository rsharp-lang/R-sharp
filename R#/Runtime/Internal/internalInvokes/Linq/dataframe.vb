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
'    Code Lines: 131 (63.59%)
' Comment Lines: 49 (23.79%)
'    - Xml Docs: 77.55%
' 
'   Blank Lines: 26 (12.62%)
'     File Size: 8.60 KB


'     Module dataframe_methods
' 
'         Function: colMeans, colSums, eval, rank_unique, rename
'                   rowMeans, rowSums
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Linq
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports anys = Microsoft.VisualBasic.Scripting

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language.Syntax
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes.LinqPipeline

    ''' <summary>
    ''' some common dataframe operations
    ''' </summary>
    Module dataframe_methods

        ''' <summary>
        ''' ### Compute Summary Statistics of Data Subsets
        ''' 
        ''' Splits the data into subsets, computes summary statistics for each, 
        ''' and returns the result in a convenient form.
        ''' </summary>
        ''' <param name="x">an R object. For the formula method a formula, such as y ~ x or cbind(y1, y2) ~ x1 + x2,
        ''' where the y variables are numeric data to be split into groups according to the grouping 
        ''' x variables (usually factors).</param>
        ''' <param name="by">
        ''' a list of grouping elements, each as long as the variables in the data frame x. 
        ''' The elements are coerced to factors before use.
        ''' </param>
        ''' <param name="FUN">
        ''' a function to compute the summary statistics which can be applied to 
        ''' all data subsets.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <example>
        ''' # create data frame
        ''' df &lt;- data.frame(team     = c('A', 'A', 'A', 'B', 'B', 'B'),
        '''                     position = c('G', 'G', 'F', 'G', 'F', 'F'),
        '''                     points   = c(99, 90, 86, 88, 95, 99),
        '''                     assists  = c(33, 28, 31, 39, 34, 23),
        '''                     rebounds = c(30, 28, 24, 24, 28, 33));
        ''' 
        ''' # view data frame
        ''' print(df);
        ''' 
        ''' #            team position    points   assists  rebounds
        ''' # -------------------------------------------------------
        ''' # &lt;mode> &lt;string> &lt;string> &lt;integer> &lt;integer> &lt;integer>
        ''' # [1, ]       "A"      "G"        99        33        30
        ''' # [2, ]       "A"      "G"        90        28        28
        ''' # [3, ]       "A"      "F"        86        31        24
        ''' # [4, ]       "B"      "G"        88        39        24
        ''' # [5, ]       "B"      "F"        95        34        28
        ''' # [6, ]       "B"      "F"        99        23        33
        ''' 
        ''' # find mean points by team
        ''' aggregate(df$points, by=list(df$team), FUN=mean);
        ''' 
        ''' #           Group        x
        ''' # ------------------------
        ''' # &lt;mode> &lt;string> &lt;double>
        ''' # A           "A"  91.6667
        ''' # B           "B"       94
        ''' 
        ''' # or 
        ''' aggregate(df, by = points ~ team, FUN = mean);
        ''' 
        ''' #           Group        x
        ''' # ------------------------
        ''' # &lt;mode> &lt;string> &lt;double>
        ''' # A           "A"  91.6667
        ''' # B           "B"       94
        ''' 
        ''' # get aggregate function demo
        ''' #
        ''' let f = aggregate(FUN = "mean");
        ''' 
        ''' # is equalient as the expression mean
        ''' f([1,2,3,4,5]);
        ''' mean([1,2,3,4,5]);
        ''' </example>
        <ExportAPI("aggregate")>
        Public Function aggregate_eval(<RRawVectorArgument> Optional x As Object = Nothing,
                                       <RRawVectorArgument> Optional [by] As Object = Nothing,
                                       <RLazyExpression>
                                       Optional FUN As Object = Nothing,
                                       Optional env As Environment = Nothing) As Object

            If FUN Is Nothing Then
                Return Internal.debug.stop("all of the required aggregate element is nothing!", env)
            End If

            ' get a function object for do aggregate
            Dim fx As Func(Of IEnumerable(Of Double), Double) = aggregate_func(FUN)

            If x Is Nothing AndAlso by Is Nothing Then
                Return fx
            End If

            If TypeOf x Is dataframe Then
                If Not TypeOf by Is FormulaExpression Then
                    Throw New NotImplementedException
                End If

                Dim formula As FormulaExpression = by
                Dim vx_name As String = formula.var
                Dim symbols = SymbolAnalysis _
                    .GetSymbolReferenceList(formula.formula) _
                    .Select(Function(fi) fi.Name) _
                    .ToArray
                Dim factors As String() = combineFactors(symbols, x)
                Dim vx As Double() = CLRVector.asNumeric(DirectCast(x, dataframe)(vx_name))

                Return aggregate_run(vx, factors, fx)
            Else
                Dim vx As Double() = CLRVector.asNumeric(x)
                Dim factors As String() = combineFactors(by, Nothing)

                Return aggregate_run(vx, factors, fx)
            End If
        End Function

        Private Function aggregate_run(vx As Double(), factors As String(), fx As Func(Of IEnumerable(Of Double), Double)) As Object
            Dim groups = factors.Select(Function(fstr, i) (fstr, vx(i))).GroupBy(Function(f) f.fstr).ToArray
            Dim result As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = groups.Select(Function(g) g.Key).ToArray
            }

            Call result.add("Group", result.rownames)
            Call result.add("x", groups.Select(Function(g) fx(g.Select(Function(i) i.Item2))))

            Return result
        End Function

        Private Function aggregate_func(fun As Object) As Func(Of IEnumerable(Of Double), Double)
            Dim assign As ValueAssignExpression = fun
            Dim value As Expression = assign.value
            Dim name As String = ValueAssignExpression.GetSymbol(value)
            Dim flag As Aggregates = anys.Expressions.ParseFlag(name)

            If flag = Aggregates.Invalid Then
                Throw New NotImplementedException
            Else
                Return anys.Expressions.GetAggregateFunction(flag)
            End If
        End Function

        Private Function combineFactors(by As Object, source_df As dataframe) As String()
            If TypeOf by Is list Then
                ' combine with multiple factor
                Dim factors As New List(Of String())

                For Each col In DirectCast(by, list).data
                    factors.Add(CLRVector.asCharacter(col))
                Next

                Return factors.combineFactors(nsize:=factors(0).Length)
            Else
                Dim factors As String() = CLRVector.asCharacter(by)

                If source_df Is Nothing Then
                    Return factors
                Else
                    ' should be the col names
                    Dim factorList As New List(Of String())

                    For Each name As String In factors
                        factorList.Add(CLRVector.asCharacter(source_df(name)))
                    Next

                    Return factorList.combineFactors(nsize:=source_df.nrows)
                End If
            End If
        End Function

        <Extension>
        Private Function combineFactors(factors As List(Of String()), nsize As Integer) As String()
            Dim group_factors As String() = Enumerable.Range(0, nsize) _
                .Select(Function(i)
                            Return factors _
                                .Select(Function(f) f(i)) _
                                .JoinBy(" / ")
                        End Function) _
                .ToArray

            Return group_factors
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

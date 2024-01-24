#Region "Microsoft.VisualBasic::37747acb205c4eee2b027ec1b2590ab6, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/base/dataframeBase.vb"

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

    '   Total Lines: 95
    '    Code Lines: 64
    ' Comment Lines: 16
    '   Blank Lines: 15
    '     File Size: 3.70 KB


    '     Module dataframeBase
    ' 
    '         Function: colSums, eval, rename
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Internal.Object
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
                               Optional renames As List = Nothing,
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
    End Module
End Namespace

#Region "Microsoft.VisualBasic::aeb7b9dc21c8d2e53670d6f36da4df14, R#\Runtime\Internal\objects\base\dataframe\rbind.vb"

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

    '   Total Lines: 150
    '    Code Lines: 102
    ' Comment Lines: 26
    '   Blank Lines: 22
    '     File Size: 5.86 KB


    '     Module rbindOp
    ' 
    '         Function: rowBindDataFrame, safeRowBindDataFrame
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime


Namespace Runtime.Internal.Object.baseOp.dataframeOp

    ''' <summary>
    ''' rbind function helpers
    ''' </summary>
    Module rbindOp

        ''' <summary>
        ''' this function will deal with the missing column name problem
        ''' (missing column will be added into the result dataframe)
        ''' </summary>
        ''' <param name="d"></param>
        ''' <param name="row"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function safeRowBindDataFrame(d As dataframe, row As dataframe, env As Environment) As Object
            ' get all union column names
            Dim colNames As String() = d.colnames _
                .JoinIterates(row.colnames) _
                .Distinct _
                .ToArray
            Dim rbind As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = d.getRowNames _
                    .JoinIterates(row.getRowNames) _
                    .uniqueNames _
                    .ToArray
            }
            Dim totalSize As Integer = d.nrows + row.nrows

            For Each col As String In colNames
                Dim v1 As Array
                Dim v2 As Array

                If d.hasName(col) Then
                    v1 = d.getColumnVector(col)
                Else
                    v1 = Nothing
                End If
                If row.hasName(col) Then
                    v2 = row.getColumnVector(col)
                Else
                    v2 = Nothing
                End If

                If v1 Is Nothing Then
                    v1 = Array.CreateInstance(v2.GetType.GetElementType, d.nrows)
                End If
                If v2 Is Nothing Then
                    v2 = Array.CreateInstance(v1.GetType.GetElementType, row.nrows)
                End If

                Dim union As Array = Array.CreateInstance(v1.GetType.GetElementType, totalSize)

                Try
                    Call Array.ConstrainedCopy(v1, Scan0, union, Scan0, v1.Length)
                    Call Array.ConstrainedCopy(v2, Scan0, union, v1.Length, v2.Length)
                Catch ex As Exception
                    Return Internal.debug.stop({
                        $"data type mis-matched while union the data fields('{col}') of two dataframe!",
                        $"colname: {col}",
                        $"v1_type: {RType.GetRSharpType(v1.GetType.GetElementType).ToString}",
                        $"v2_type: {RType.GetRSharpType(v2.GetType.GetElementType).ToString}"
                    }, env)
                End Try

                Call rbind.columns.Add(col, REnv.UnsafeTryCastGenericArray(union))
            Next

            Return rbind
        End Function

        ''' <summary>
        ''' combine two dataframe by rows
        ''' </summary>
        ''' <param name="d"></param>
        ''' <param name="row"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function will check for the column name matches
        ''' </remarks>
        Public Function rowBindDataFrame(d As dataframe, row As dataframe, env As Environment) As Object
            ' 20240119
            ' one of them maybe empty
            If d.empty Then
                Return row
            ElseIf row.empty Then
                Return d
            End If

            If d.columns.Count <> row.columns.Count Then
                Return Internal.debug.stop({
                    $"mismatch column size between two dataframe!",
                    $"({d.ncols}) columns: {d.colnames.GetJson}",
                    $"({row.ncols}) columns: {row.colnames.GetJson}",
                    $"diffs: {DirectCast(Invokes.[set].setdiff(d.colnames, row.colnames, env), String()).GetJson}"
                }, env)
            End If

            For Each col In row.columns
                If Not d.hasName(col.Key) Then
                    Return Internal.debug.stop({$"names do not match previous names", $"missing: {col.Key}"}, env)
                End If
            Next

            Dim colNames As String() = d.columns.Keys.ToArray
            ' re-order columns, make the column orders keeps the same
            ' between two dataframe objects
            Dim copy As dataframe = d.projectByColumn(colNames, fullSize:=True, env:=env)
            Dim copy2 As dataframe = row.projectByColumn(colNames, fullSize:=True, env:=env)
            Dim r1 As Integer = copy.nrows
            Dim r2 As Integer = copy2.nrows
            Dim totalRows As Integer = r1 + r2
            Dim oldRownames As String() = copy.getRowNames

            For Each col As String In colNames
                Dim a As GetVectorElement = GetVectorElement.CreateAny(copy.columns(col))
                Dim b As GetVectorElement = GetVectorElement.CreateAny(copy2.columns(col))
                Dim vec As Object() = New Object(totalRows - 1) {}

                For i As Integer = 0 To r1 - 1
                    vec(i) = a(i)
                Next

                For i As Integer = 0 To r2 - 1
                    vec(i + r1) = b(i)
                Next

                copy.columns(col) = vec
            Next

            copy.rownames = oldRownames _
                .JoinIterates(copy2.getRowNames) _
                .uniqueNames _
                .ToArray

            Return copy
        End Function

    End Module
End Namespace

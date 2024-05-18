#Region "Microsoft.VisualBasic::0dad2d3969f2d5a0e2b3ba5b0c67fb6d, R#\Runtime\Internal\objects\Linq\DataframeGroup.vb"

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

    '   Total Lines: 86
    '    Code Lines: 64
    ' Comment Lines: 6
    '   Blank Lines: 16
    '     File Size: 3.39 KB


    '     Module DataframeGroup
    ' 
    '         Function: groupBy, head, orderBy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Object.Linq

    Public Module DataframeGroup

        <Extension>
        Public Function orderBy(data As dataframe, key As String, Optional desc As Boolean = False) As dataframe
            Static env As Environment = GlobalEnvironment.defaultEmpty

            Dim i As Double() = CLRVector.asNumeric(data.getColumnVector(key))
            Dim order As Integer() = ranking.order(i, desc, env)

            Return data.sliceByRow(order, env)
        End Function

        ''' <summary>
        ''' group by the string factor values in dataframe column as key
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="key"></param>
        ''' <param name="safe">
        ''' will cast null to empty string if this parameter value is set to TRUE
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function groupBy(data As dataframe, key As String, safe As Boolean) As Dictionary(Of String, dataframe)
            Dim values As vector = CLRVector.asCharacter(data.getColumnVector(columnName:=key)) _
                .DoCall(Function(v)
                            If v Is Nothing Then
                                Throw New MissingPrimaryKeyException($"missing the field name '{key}' inside your dataframe for may data table group by field, please check of the dataframe field names or file encoding if the dataframe is read from csv file which contains non-ascii characters(example as zh-CN characters).")
                            End If

                            Return vector.asVector(Of String)(v)
                        End Function)
            Dim factors As String() = DirectCast(values.data, String()) _
                .Select(Function(factor)
                            ' cast to safe key for null
                            If factor Is Nothing AndAlso safe Then
                                Return ""
                            Else
                                Return factor
                            End If
                        End Function) _
                .Distinct _
                .ToArray
            Dim groups As New Dictionary(Of String, dataframe)
            Dim env As New Environment

            For Each factor As String In factors
                Dim i As Boolean() = (values = factor).asLogical
                Dim partRows As dataframe = data.sliceByRow(i, env)

                ' null will case the error
                Call groups.Add(factor, partRows)
            Next

            Return groups
        End Function

        <Extension>
        Public Function head(df As dataframe, Optional n As Integer = 6) As dataframe
            If df.nrows <= n Then
                Return df
            End If

            Dim data As New Dictionary(Of String, Array)
            Dim colVal As Array
            Dim colSubset As Array
            Dim vtype As Type

            For Each col In df.columns
                If col.Value.Length = 1 Then
                    data.Add(col.Key, col.Value)
                Else
                    colVal = col.Value
                    vtype = colVal.GetType.GetElementType
                    colSubset = Array.CreateInstance(vtype, n)

                    For i As Integer = 0 To n - 1
                        colSubset.SetValue(colVal.GetValue(i), i)
                    Next

                    data.Add(col.Key, colSubset)
                End If
            Next

            Return New dataframe With {
                .columns = data,
                .rownames = df.rownames _
                    .SafeQuery _
                    .Take(n) _
                    .ToArray
            }
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::0bf3b28e6010bdf5a1736209afb59413, R#\Runtime\Internal\objects\dataframe.vb"

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

    '     Class dataframe
    ' 
    '         Properties: columns, nrows, rownames
    ' 
    '         Function: GetTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Internal

    Public Class dataframe

        ''' <summary>
        ''' 长度为1或者长度为n
        ''' </summary>
        ''' <returns></returns>
        Public Property columns As Dictionary(Of String, Array)
        Public Property rownames As String()
        Public ReadOnly Property nrows As Integer
            Get
                Return Aggregate col In columns.Values Let len = col.Length Into Max(len)
            End Get
        End Property

        ''' <summary>
        ''' Each element in a return result array is a row in table matrix
        ''' </summary>
        ''' <returns></returns>
        Public Function GetTable() As String()()
            Dim table As String()() = New String(nrows)() {}
            Dim row As String()
            Dim rIndex As Integer
            Dim colNames$() = columns.Keys.ToArray
            Dim col As Array

            row = {""}.Join(colNames)
            table(Scan0) = row.ToArray

            If rownames.IsNullOrEmpty Then
                rownames = table _
                    .Sequence(offSet:=1) _
                    .Select(Function(r) $"[{r}, ]") _
                    .ToArray
            End If

            For i As Integer = 1 To table.Length - 1
                rIndex = i - 1
                row(Scan0) = rownames(rIndex)

                For j As Integer = 0 To columns.Count - 1
                    col = columns(colNames(j))

                    If col.Length = 1 Then
                        row(j + 1) = Scripting.ToString(col.GetValue(Scan0), "NULL")
                    Else
                        row(j + 1) = Scripting.ToString(col.GetValue(rIndex), "NULL")
                    End If
                Next

                table(i) = row.ToArray
            Next

            Return table
        End Function

    End Class
End Namespace

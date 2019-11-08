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
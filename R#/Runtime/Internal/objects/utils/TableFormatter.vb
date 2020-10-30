Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Runtime.Internal.Object.Utils

    Public Class TableFormatter

        ''' <summary>
        ''' Each element in a return result array is a row in table matrix
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetTable(df As dataframe, env As GlobalEnvironment, Optional printContent As Boolean = True, Optional showRowNames As Boolean = True) As String()()
            Dim table As String()() = New String(df.nrows)() {}
            Dim rIndex As Integer
            Dim colNames$() = df.columns.Keys.ToArray
            Dim col As Array
            Dim row As String() = {""}.Join(colNames)
            Dim rownames = df.getRowNames()

            If showRowNames Then
                table(Scan0) = row.ToArray
            Else
                table(Scan0) = row.Skip(1).ToArray
            End If

            Dim elementTypes As Type() = colNames _
                .Select(Function(key)
                            Return df.columns(key).GetType.GetElementType
                        End Function) _
                .ToArray
            Dim formatters As IStringBuilder() = elementTypes _
                .Select(Function(type)
                            Return printer.ToString(type, env, printContent)
                        End Function) _
                .ToArray

            For i As Integer = 1 To table.Length - 1
                rIndex = i - 1
                row(Scan0) = rownames(rIndex)

                For j As Integer = 0 To df.columns.Count - 1
                    col = df.columns(colNames(j))

                    If col.Length = 1 Then
                        row(j + 1) = formatters(j)(col.GetValue(Scan0))
                    Else
                        row(j + 1) = formatters(j)(col.GetValue(rIndex))
                    End If
                Next

                If showRowNames Then
                    table(i) = row.ToArray
                Else
                    table(i) = row.Skip(1).ToArray
                End If
            Next

            Return table
        End Function
    End Class
End Namespace
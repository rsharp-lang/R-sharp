Imports System.Runtime.CompilerServices

Namespace Runtime.Internal.Object.Converts

    Module makeList

        <Extension>
        Public Function listByColumns(data As dataframe) As list
            Return New list With {
                .slots = data.columns _
                    .ToDictionary(Function(t) t.Key,
                                  Function(t)
                                      Return CObj(t.Value)
                                  End Function)
            }
        End Function

        <Extension>
        Public Function listByRows(data As dataframe) As list
            Dim rows As New Dictionary(Of String, Object)
            Dim columns As String() = data.columns.Keys.ToArray
            Dim getVals As New Dictionary(Of String, Func(Of Integer, Object))

            For Each key As String In columns
                Dim val As Array = data.columns(key)

                If val.Length = 1 Then
                    Dim first As Object = val.GetValue(Scan0)
                    getVals.Add(key, Function() first)
                Else
                    getVals.Add(key, Function(i) val.GetValue(i))
                End If
            Next

            Dim index As Integer
            Dim row As Dictionary(Of String, Object)

            For i As Integer = 0 To data.nrows - 1
                index = i
                row = columns.ToDictionary(Function(col) col, Function(col) getVals(col)(index))
                rows.Add($"[[{index + 1}]]", New list With {.slots = row})
            Next

            Return New list With {.slots = rows}
        End Function
    End Module
End Namespace
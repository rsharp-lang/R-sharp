Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Net.Http

Public Class TableExport

    ReadOnly fields As String()
    ReadOnly haveIDCol As Integer
    ReadOnly table As New List(Of EntityObject)

    Sub New(fields As String())
        Me.fields = fields
        Me.haveIDCol = fields.IndexOf("ID")
    End Sub

    Public Function GetTable() As EntityObject()
        Return table.ToArray
    End Function

    Public Function Fill(rId As Integer, row As Object()) As Boolean
        Dim cols As New Dictionary(Of String, String)
        Dim id$

        If haveIDCol > -1 Then
            id = row(haveIDCol)

            For i As Integer = 0 To fields.Length - 1
                If i = haveIDCol Then
                    Continue For
                Else
                    Call addColToString(cols, i, row(i))
                End If
            Next
        Else
            id = "#" & rId

            For i As Integer = 0 To fields.Length - 1
                Call addColToString(cols, i, row(i))
            Next
        End If

        Call table.Add(New EntityObject With {.id = id, .Properties = cols})

        Return False
    End Function

    Private Sub addColToString(ByRef cols As Dictionary(Of String, String), i As Integer, col As Object)
        If col.GetType Is GetType(Byte()) Then
            ' base64
            If IsDBNull(col) Then
                cols.Add(fields(i), "")
            Else
                cols.Add(fields(i), DirectCast(col, Byte()).ToBase64String)
            End If
        Else
            cols.Add(fields(i), Scripting.ToString(col))
        End If
    End Sub
End Class
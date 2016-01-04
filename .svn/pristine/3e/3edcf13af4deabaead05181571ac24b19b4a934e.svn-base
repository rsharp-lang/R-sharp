Public Class Preferences

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Using Open As New OpenFileDialog With {.Filter = "Program File(*.exe)|*.exe"}

            If Open.ShowDialog = DialogResult.OK Then
                TextBox1.Text = Open.FileName
            End If
        End Using
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Program.Settings.Debugger = TextBox1.Text
    End Sub
End Class

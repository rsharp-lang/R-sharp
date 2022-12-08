Imports System.Threading
Imports Flute.Http.Core

Public Module Utils

    ''' <summary>
    ''' if parent is exists then kill current web server 
    ''' </summary>
    ''' <param name="parentId"></param>
    ''' <param name="kill"></param>
    Public Sub BindToParent(parentId As String, kill As HttpServer)
        ' not specific the parent process id
        If parentId.StringEmpty Then
            Return
        Else
#If WINDOWS Then
            Dim task As New ThreadStart(
                Sub()
                    Utils.parentHeartbeat(Integer.Parse(parentId), kill)
                End Sub)

            Call New Thread(task).Start()
#End If
        End If
    End Sub

    Private Sub parentHeartbeat(parentId As Integer, kill As HttpServer)
        Dim parent As Process

        Try
            parent = Process.GetProcessById(parentId)
        Catch ex As Exception
            Call kill.Dispose()
            Return
        End Try

        Do While True
            Try
                If parent.HasExited Then
                    Call kill.Dispose()
                    Exit Do
                End If
            Catch ex As Exception
                Call kill.Dispose()
                Exit Do
            End Try

            Call Thread.Sleep(100)
        Loop
    End Sub

End Module
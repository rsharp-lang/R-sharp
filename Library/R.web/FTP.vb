Imports System.IO
Imports System.Net
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

<Package("ftp", Category:=APICategories.UtilityTools)>
Module FTP

    <ExportAPI("list.ftp_dirs")>
    Public Function list_ftpdirs(ftp As FtpContext, dir As String, Optional env As Environment = Nothing) As Object
        Dim request As FtpWebRequest = ftp.CreateRequest(dir)
        Dim list As New List(Of String)

        request.Method = WebRequestMethods.Ftp.ListDirectory

        Using response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)
            Dim responseStream As Stream = response.GetResponseStream

            Using reader As New StreamReader(responseStream)
                Do While reader.Peek <> -1
                    list.Add(reader.ReadLine)
                Loop
            End Using

            Return list.ToArray
        End Using
    End Function
End Module

Public Class FtpContext

    Public Property userName As String
    Public Property password As String
    Public Property server As String

    Public Function CreateRequest(dir As String) As FtpWebRequest
        Dim ftpContext As String = $"{server}/{dir}"
        Dim request As FtpWebRequest = DirectCast(WebRequest.Create(ftpContext), FtpWebRequest)

        If Not (userName.StringEmpty OrElse password.StringEmpty) Then
            request.Credentials = New NetworkCredential(userName, password)
        End If

        Return request
    End Function

End Class
Imports System.IO
Imports System.Net
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("ftp", Category:=APICategories.UtilityTools)>
<RTypeExport("ftp", GetType(FtpContext))>
Module FTP

    <ExportAPI("list.ftp_dirs")>
    Public Function list_ftpdirs(ftp As FtpContext, dir As String, Optional env As Environment = Nothing) As Object
        Dim request As FtpWebRequest = ftp.CreateRequest(dir)
        Dim list As New List(Of String)

        request.Method = WebRequestMethods.Ftp.ListDirectory

        Using response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)
            Dim responseStream As Stream = response.GetResponseStream
            Dim dirname As String = dir.Trim("/"c).Split("/"c).Last

            Using reader As New StreamReader(responseStream)
                Do While reader.Peek <> -1
                    list.Add(reader.ReadLine)
                Loop
            End Using

            Return list _
                .Select(Function(a) a.Replace($"{dirname}/", "")) _
                .ToArray
        End Using
    End Function

    <ExportAPI("ftp.get")>
    Public Function ftpget(ftp As FtpContext, file As String, Optional save As String = "./", Optional env As Environment = Nothing) As Object
        Dim request As FtpWebRequest = ftp.CreateRequest(file)

        request.Method = WebRequestMethods.Ftp.DownloadFile

        Using response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)
            Dim responseStream As Stream = response.GetResponseStream
            Dim filepath As String
            Dim buffer As Byte() = New Byte(1024 - 1) {}
            Dim size As i32 = Scan0

            If save.StringEmpty OrElse save.Last = "/" Then
                filepath = $"{save}/{file.FileName}"
            Else
                filepath = save
            End If

            Using write As New BinaryWriter(filepath.Open(, doClear:=True)), reader As New StreamReader(responseStream)
                Do While True
                    If (size = reader.BaseStream.Read(buffer, Scan0, buffer.Length)) > 0 Then
                        Call write.Write(buffer.Take(size).ToArray)
                    Else
                        Exit Do
                    End If
                Loop

                Call write.Flush()
            End Using
        End Using

        Return True
    End Function
End Module

Public Class FtpContext

    Public Property username As String
    Public Property password As String
    Public Property server As String

    Public Function CreateRequest(dir As String) As FtpWebRequest
        Dim ftpContext As String = $"ftp://{server}/{dir}"
        Dim request As FtpWebRequest = DirectCast(WebRequest.Create(ftpContext), FtpWebRequest)

        If Not (username.StringEmpty OrElse password.StringEmpty) Then
            request.Credentials = New NetworkCredential(username, password)
        End If

        Return request
    End Function

End Class
Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.System.Configuration
Imports SMRUCC.WebCloud.HTTPInternal.Core

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("--start")>
    <Usage("--start [--port <port number, default=7452> --Rweb <directory, default=./Rweb> --n_threads <max_threads, default=8>]")>
    Public Function start(args As CommandLine) As Integer
        Dim port As Integer = args("--port") Or 7452
        Dim Rweb As String = args("--Rweb") Or App.CurrentDirectory & "/Rweb"
        Dim n_threads As Integer = args("--n_threads") Or 8

        Using http As New Rweb(Rweb, port, threads:=n_threads)
            Return http.Run()
        End Using
    End Function

End Module

Public Class Rweb : Inherits HttpServer

    Dim Rweb As String

    Public Sub New(Rweb$, port As Integer, Optional threads As Integer = -1)
        MyBase.New(port, threads)

        Me.Rweb = Rweb
    End Sub

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Using response As New HttpResponse(p.outputStream, AddressOf p.writeFailure)
            ' /<scriptFileName>?...args
            Dim request As New HttpRequest(p)
            Dim Rscript As String = Rweb & "/" & Strings.Trim(request.URL.Split("?"c).FirstOrDefault).Trim("."c, "/"c) & ".R"

            If Not Rscript.FileExists Then
                Call p.writeFailure(404, "file not found!")
            Else
                Call runRweb(Rscript, response)
            End If
        End Using
    End Sub

    Private Sub runRweb(Rscript As String, response As HttpResponse)
        Using output As New MemoryStream(), Rstd_out As New StreamWriter(output)
            Dim result As Object
            Dim code As Integer
            Dim content_type As String

            ' run rscript
            Using R As RInterpreter = RInterpreter _
                .FromEnvironmentConfiguration(ConfigFile.localConfigs) _
                .RedirectOutput(Rstd_out)

                R.silent = True

                For Each pkgName As String In R.configFile.GetStartupLoadingPackages
                    Call R.LoadLibrary(packageName:=pkgName, silent:=True)
                Next

                result = R.Source(Rscript)
                code = Rserve.Rscript.handleResult(result, R.globalEnvir, Nothing)

                If R.globalEnvir.stdout.recommendType Is Nothing Then
                    content_type = "text/html"
                Else
                    content_type = R.globalEnvir.stdout.recommendType
                End If
            End Using

            Call Rstd_out.Flush()

            If code <> 0 Then
                Call response.WriteError(code, Encoding.UTF8.GetString(output.ToArray))
            Else
                Call response.WriteHeader(content_type, output.Length)
                Call response.Write(output.ToArray)
            End If
        End Using
    End Sub

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Call p.writeFailure(404, "not allowed!")
    End Sub

    Public Overrides Sub handlePUTMethod(p As HttpProcessor, inputData As String)
        Call p.writeFailure(404, "not allowed!")
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Call p.writeFailure(404, "not allowed!")
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Overrides Function __httpProcessor(client As TcpClient) As HttpProcessor
        Return New HttpProcessor(client, Me, MAX_POST_SIZE:=App.BufferSize) With {
            ._404Page = Function() "404 Not Found"
        }
    End Function
End Class
Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.System.Configuration

''' <summary>
''' Rweb is not design for general web programming, it is 
''' design for running a background data task.
''' </summary>
Public Class Rweb : Inherits HttpServer

    Dim Rweb As String
    Dim showError As Boolean

    Public Sub New(Rweb$, port As Integer, show_error As Boolean, Optional threads As Integer = -1)
        MyBase.New(port, threads)

        Me.Rweb = Rweb
        Me.showError = show_error
    End Sub

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Using response As New HttpResponse(p.outputStream, AddressOf p.writeFailure)
            ' /<scriptFileName>?...args
            Dim request As New HttpRequest(p)
            Dim Rscript As String = Rweb & "/" & request.URL.path & ".R"

            If Not Rscript.FileExists Then
                Call p.writeFailure(404, "file not found!")
            Else
                Call runRweb(Rscript, request.URL.query, response)
            End If
        End Using
    End Sub

    Private Sub runRweb(Rscript As String, args As Dictionary(Of String, String()), response As HttpResponse)
        Using output As New MemoryStream(), Rstd_out As New StreamWriter(output, Encodings.UTF8WithoutBOM.CodePage)
            Dim result As Object
            Dim code As Integer
            Dim content_type As String
            Dim http As NamedValue(Of Object)() = args _
                .Select(Function(t)
                            Return New NamedValue(Of Object) With {
                                .Name = t.Key,
                                .Value = t.Value
                            }
                        End Function) _
                .ToArray

            Call args.GetJson.__DEBUG_ECHO

            ' run rscript
            Using R As RInterpreter = RInterpreter _
                .FromEnvironmentConfiguration(ConfigFile.localConfigs) _
                .RedirectOutput(Rstd_out)

                R.silent = True
                R.redirectError2stdout = showError

                For Each pkgName As String In R.configFile.GetStartupLoadingPackages
                    Call R.LoadLibrary(packageName:=pkgName, silent:=True)
                Next

                result = R.Source(Rscript, http)
                code = Rserve.Rscript.handleResult(result, R.globalEnvir, Nothing)

                If R.globalEnvir.stdout.recommendType Is Nothing Then
                    content_type = "text/html"
                Else
                    content_type = R.globalEnvir.stdout.recommendType
                End If
            End Using

            Call Rstd_out.Flush()

            If code <> 0 Then
                Dim err As String = Encoding.UTF8.GetString(output.ToArray)

                If showError Then
                    Call response.WriteHTML(err)
                Else
                    Call response.WriteError(code, err)
                End If
            Else
                Call response.WriteHeader(content_type, output.Length)
                Call response.Write(output.ToArray)
            End If
        End Using
    End Sub

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Call p.writeFailure(404, "not allowed!")
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Call p.writeFailure(404, "not allowed!")
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize%) As HttpProcessor
        Return New HttpProcessor(client, Me, MAX_POST_SIZE:=bufferSize)
    End Function
End Class
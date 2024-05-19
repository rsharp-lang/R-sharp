#Region "Microsoft.VisualBasic::a61b92224f73ffddfbfb0fb48bdb63bf, studio\Rserver\Rweb\RProcessor.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 261
    '    Code Lines: 192
    ' Comment Lines: 32
    '   Blank Lines: 37
    '     File Size: 9.74 KB


    ' Class RProcessor
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: RscriptRouter, WithStartups
    ' 
    '     Sub: pushBackResult, RscriptHttpGet, RscriptHttpPost, runRweb, SaveResponse
    '     Class WebTask
    ' 
    '         Function: callInternal, ToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Specialized
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Flute.Http.FileSystem
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' 
''' </summary>
Public Class RProcessor

    ''' <summary>
    ''' APP_PATH, the directory path of the rscript folder for the slave rscript process.
    ''' </summary>
    Dim Rweb As String
    Dim showError As Boolean
    Dim requestPostback As New Dictionary(Of String, BufferObject)
    Dim localRServer As Rweb
    ''' <summary>
    ''' package list for load while on slave process startup
    ''' </summary>
    Dim startups As String
    Dim debug As Boolean

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Rserver"></param>
    ''' <param name="RwebDir">
    ''' the directory path of the rscript folder for the slave rscript process.
    ''' </param>
    ''' <param name="show_error"></param>
    ''' <param name="debug"></param>
    Sub New(Rserver As Rweb, RwebDir As String, show_error As Boolean, debug As Boolean)
        Me.Rweb = RwebDir.GetDirectoryFullPath
        Me.showError = show_error
        Me.localRServer = Rserver
        Me.debug = debug
    End Sub

    ''' <summary>
    ''' setting the startup package names for run the slave rscript process.
    ''' </summary>
    ''' <param name="packages"></param>
    ''' <returns></returns>
    Public Function WithStartups(ParamArray packages As String()) As RProcessor
        startups = packages.JoinBy(",")
        Return Me
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub SaveResponse(requestId As String, buffer As Buffer)
        SyncLock requestPostback
            requestPostback(requestId) = buffer.data
        End SyncLock
    End Sub

    ''' <summary>
    ''' put the task into background queue by adding a query parameter
    ''' 
    ''' ```
    ''' rweb_background=true
    ''' ```
    ''' </summary>
    ''' <param name="request"></param>
    ''' <param name="response"></param>
    Public Sub RscriptHttpPost(request As HttpPOSTRequest, response As HttpResponse)
        ' /<scriptFileName>?...args
        Dim Rscript As String = RscriptRouter(request)
        Dim args As New Dictionary(Of String, String())(request.URL.query)
        Dim is_background As Boolean = request.GetBoolean("rweb_background")

        If Not Rscript.FileExists Then
            Call response.WriteError(HTTP_RFC.RFC_NOT_FOUND, "rscript file not found!")
            Return
        End If

        Dim form As NameValueCollection = request.POSTData.Form
        Dim request_id As String = Rserver.Rweb.NextRequestId

        For Each formKey As String In form.AllKeys
            args(formKey) = form.GetValues(formKey)
        Next

        If Not request.POSTData.Objects.IsNullOrEmpty Then
            For Each obj In request.POSTData.Objects
                If TypeOf obj.Value Is JsonObject Then
                    args(obj.Key) = {DirectCast(obj.Value, JsonObject).BuildJsonString}
                Else
                    args(obj.Key) = CLRVector.asCharacter(obj.Value)
                End If
            Next
        End If

        If Not request.POSTData.files.IsNullOrEmpty Then
            For Each file In request.POSTData.files
                args(file.Key) = file.Value _
                    .Select(Function(f) f.GetJSON) _
                    .ToArray
            Next
        End If

        ' the request id is send back to the client 
        ' in plain text format
        If is_background Then
            Call RunTask(Sub() Call runRweb(Rscript, request_id, args, request, response, is_background))
            Call response.WriteHTML(request_id)
        Else
            Call runRweb(Rscript, request_id, args, request, response, is_background)
        End If
    End Sub

    Public Sub RscriptHttpGet(p As HttpProcessor)
        Using response As New HttpResponse(p) With {
            .AccessControlAllowOrigin = "*"
        }
            ' /<scriptFileName>?...args
            Dim request As New HttpRequest(p)
            Dim Rscript As String = RscriptRouter(request)
            Dim is_background As Boolean = request.GetBoolean("rweb_background")
            Dim request_id As String = Rserver.Rweb.NextRequestId

            If request.URL.path = "check_invoke" Then
                request_id = request.URL("request_id")

                SyncLock requestPostback
                    Call response.WriteHTML(requestPostback.ContainsKey(request_id))
                End SyncLock
            ElseIf request.URL.path = "get_invoke" Then
                Call pushBackResult(request.URL("request_id"), request, response)
            ElseIf Not Rscript.FileExists Then
                If localRServer.fs Is Nothing Then
                    Call p.writeFailure(HTTP_RFC.RFC_NOT_FOUND, "file not found!")
                Else
                    Call WebFileSystemListener.HostStaticFile(localRServer.fs, request, response)
                End If
            ElseIf is_background Then
                Dim task As New WebTask With {
                    .Rscript = Rscript,
                    .request_id = request_id,
                    .host = Me,
                    .query = request.URL.query,
                    .request = request,
                    .response = response,
                    .app_path = Rweb
                }

                Call $"task '{request_id}' will be running in background.".__DEBUG_ECHO
                Call task.CommitTask()
                Call response.WriteHTML(request_id)
            Else
                Call runRweb(Rscript, request_id, request.URL.query, request, response, is_background)
            End If
        End Using
    End Sub

    Private Class WebTask

        Public Rscript As String
        Public request_id As String
        Public query As Dictionary(Of String, String())
        Public request As HttpRequest
        Public response As HttpResponse
        Public host As RProcessor
        Public app_path As String

        Public Async Function CommitTask() As Task(Of Boolean)
            Return Await Task.Run(AddressOf callInternal)
        End Function

        Private Function callInternal() As Boolean
            Call host.runRweb(
                Rscript:=Rscript,
                request_id:=request_id,
                args:=request.URL.query,
                request:=request,
                response:=response,
                is_background:=True
            )

            Return True
        End Function

        Public Overrides Function ToString() As String
            Return request_id
        End Function
    End Class

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function RscriptRouter(request As HttpRequest) As String
        Dim url As URL = request.URL

        If url.path.StringEmpty OrElse url.path = "/" Then
            Return $"{Rweb}/index.R"
        ElseIf url.path.Last = "/"c Then
            Return $"{Rweb}/{request.URL.path}/index.R"
        Else
            Return $"{Rweb}/{request.URL.path}.R"
        End If
    End Function

    Private Sub pushBackResult(request_id$, request As HttpRequest, response As HttpResponse)
        Dim result As BufferObject = requestPostback.TryGetValue(request_id)

        If result Is Nothing Then
            Call $"unsure for empty result:: [{request_id}]...".Warning
        Else
            Call $"get callback from slave process [{request_id}] -> {result.code.Description}".__INFO_ECHO
        End If

        SyncLock requestPostback
            Call requestPostback.Remove(request_id)
        End SyncLock

        Call RCallbackMessage.SendHttpResponseMessage(result, request, response, debug, showErr:=showError)
    End Sub

    ''' <summary>
    ''' helper function for call rscript slave process
    ''' </summary>
    ''' <param name="Rscript">the file path of the target R# script file</param>
    ''' <param name="args">script arguments</param>
    ''' <param name="response"></param>
    Private Sub runRweb(Rscript As String,
                        request_id$,
                        args As Dictionary(Of String, String()),
                        request As HttpRequest,
                        response As HttpResponse,
                        is_background As Boolean)

        Dim rawjson As String = JsonContract.GetJson(args)
        Dim argsText As String = rawjson.Base64String(gzip:=True)
        Dim port As Integer = localRServer.TcpPort
        Dim master As String = "localhost"
        Dim entry As String = "run"
        Dim Rslave = RscriptCommandLine.Rscript.FromEnvironment(directory:=App.HOME)

        Call rawjson.__DEBUG_ECHO

        ' --slave /exec <script.R> /args <json_base64> /request-id <request_id> /PORT=<port_number> [/MASTER=<ip, default=localhost> /entry=<function_name, default=NULL>]
        Dim arguments As String = Rslave.GetslaveModeCommandLine(
            exec:=Rscript.GetFullPath,
            argvs:=argsText,
            request_id:=request_id,
            PORT:=port,
            master:=master,
            entry:=entry,
            internal_pipelineMode:=False,
            startups:=startups
        )

        Rslave.dotnetcoreApp = True
        Rslave.SetDotNetCoreDll()

        Dim task As Process = Process.Start(New ProcessStartInfo With {
            .Arguments = Rslave.GetDotnetCoreCommandLine(arguments),
            .WorkingDirectory = App.HOME,
            .CreateNoWindow = True,
            .FileName = "dotnet",
            .UseShellExecute = False,
            .RedirectStandardInput = True
        })
        Dim http_context As New MultipartForm
        Dim cookies As Cookies = request.GetCookies

        task.StartInfo.EnvironmentVariables("APP_PATH") = Rweb

        If Not cookies.CheckCookie("session_id") Then
            Call cookies.SetValue("session_id", (Now.ToString & arguments).MD5)
            Call response.SetCookies("session_id", cookies.GetCookie("session_id"))
        End If

        Call http_context.Add("cookies", cookies.ToJSON)
        Call http_context.Add("configs", request.HttpRequest.GetSettings.GetJson(maskReadonly:=True))
        Call task.Start()

        Using std_in As StreamWriter = task.StandardInput
            Call std_in.WriteLine(http_context.boundary)
            Call std_in.WriteLine(http_context.ToBase64)
            Call std_in.Flush()
        End Using

        ' view commandline
        Call VBDebugger.EchoLine(arguments.TrimNewLine.Trim.StringReplace("\s{2,}", " "))
        Call VBDebugger.EchoLine(http_context.boundary)
        Call VBDebugger.EchoLine(http_context.ToBase64)

        Call task.WaitForExit()

        If Not is_background Then
            Call pushBackResult(request_id, request, response)
        End If
    End Sub
End Class

#Region "Microsoft.VisualBasic::b09c6457798724ba66e8f763be75c802, D:/GCModeller/src/R-sharp/studio/Rserver//Rweb/RProcessor.vb"

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

    '   Total Lines: 228
    '    Code Lines: 166
    ' Comment Lines: 27
    '   Blank Lines: 35
    '     File Size: 8.34 KB


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
Imports System.Runtime.CompilerServices
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' 
''' </summary>
Public Class RProcessor

    Dim Rweb As String
    Dim showError As Boolean
    Dim requestPostback As New Dictionary(Of String, BufferObject)
    Dim localRServer As Rweb
    ''' <summary>
    ''' package list for load while on slave process startup
    ''' </summary>
    Dim startups As String
    Dim debug As Boolean

    Sub New(Rserver As Rweb, RwebDir As String, show_error As Boolean, debug As Boolean)
        Me.Rweb = RwebDir
        Me.showError = show_error
        Me.localRServer = Rserver
        Me.debug = debug
    End Sub

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
            Call response.WriteError(404, "not allowed!")
            Return
        End If

        Dim form As NameValueCollection = request.POSTData.Form
        Dim request_id As String = Rserver.Rweb.NextRequestId

        For Each formKey In form.AllKeys
            args(formKey) = form.GetValues(formKey)
        Next

        If Not request.POSTData.Objects.IsNullOrEmpty Then
            For Each obj In request.POSTData.Objects
                args(obj.Key) = REnv.asVector(Of String)(obj.Value)
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
        Using response As New HttpResponse(p.outputStream, AddressOf p.writeFailure) With {
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
                Call p.writeFailure(404, "file not found!")
            ElseIf is_background Then
                Dim task As New WebTask With {
                    .Rscript = Rscript,
                    .request_id = request_id,
                    .host = Me,
                    .query = request.URL.query,
                    .request = request,
                    .response = response
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
        Return $"{Rweb}/{request.URL.path}.R"
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
    ''' 
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

        Dim argsText As String = args.GetJson.Base64String(gzip:=True)
        Dim port As Integer = localRServer.TcpPort
        Dim master As String = "localhost"
        Dim entry As String = "run"
        Dim Rslave = RscriptCommandLine.Rscript.FromEnvironment(directory:=App.HOME)

        Call args.GetJson.__DEBUG_ECHO

        ' --slave /exec <script.R> /args <json_base64> /request-id <request_id> /PORT=<port_number> [/MASTER=<ip, default=localhost> /entry=<function_name, default=NULL>]
        Dim arguments As String = Rslave.GetslaveModeCommandLine(
            exec:=Rscript.GetFullPath,
            args:=argsText,
            request_id:=request_id,
            PORT:=port,
            master:=master,
            entry:=entry,
            internal_pipelineMode:=False,
            startups:=startups
        )

        Rslave.dotnetcoreApp = True
        Rslave.SetDotNetCoreDll()

        Dim task As RunSlavePipeline = Rslave.CreateSlave(arguments, workdir:=App.HOME)

        ' view commandline
        Call Console.WriteLine(task.ToString)

        task.Shell = True
        task.Run()

        If Not is_background Then
            Call pushBackResult(request_id, request, response)
        End If
    End Sub
End Class

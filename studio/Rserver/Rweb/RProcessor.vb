#Region "Microsoft.VisualBasic::492f6f417f1840e8655b767b1ffdaf5c, R-sharp\studio\Rserver\Rweb\RProcessor.vb"

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

'   Total Lines: 203
'    Code Lines: 161
' Comment Lines: 12
'   Blank Lines: 30
'     File Size: 7.92 KB


' Class RProcessor
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: RscriptRouter, WithStartups
' 
'     Sub: pushBackResult, RscriptHttpGet, RscriptHttpPost, runRweb, SaveResponse
' 
' /********************************************************************************/

#End Region

Imports System.Collections.Specialized
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.[Object].dataframe
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports REnv = SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports Microsoft.VisualBasic.Data.csv.IO

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

        If is_background Then
            Call RunTask(Sub() Call runRweb(Rscript, request_id, args, response, is_background))
            Call response.WriteHTML(request_id)
        Else
            Call runRweb(Rscript, request_id, args, response, is_background)
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
                    Call p.WriteLine(requestPostback.ContainsKey(request_id))
                End SyncLock
            ElseIf request.URL.path = "get_invoke" Then
                Call pushBackResult(request.URL("request_id"), response)
            ElseIf Not Rscript.FileExists Then
                Call p.writeFailure(404, "file not found!")
            ElseIf is_background Then
                Call $"task '{request_id}' will be running in background.".__DEBUG_ECHO
                Call New Action(Sub() Call runRweb(Rscript, request_id, request.URL.query, response, is_background)).BeginInvoke(Nothing, Nothing)
                Call response.WriteHTML(request_id)
            Else
                Call runRweb(Rscript, request_id, request.URL.query, response, is_background)
            End If
        End Using
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Function RscriptRouter(request As HttpRequest) As String
        Return $"{Rweb}/{request.URL.path}.R"
    End Function

    Private Sub pushBackResult(request_id$, response As HttpResponse)
        Dim result As BufferObject = requestPostback.TryGetValue(request_id)

        If result Is Nothing Then
            Call $"unsure for empty result:: [{request_id}]...".Warning
        Else
            Call $"get callback from slave process [{request_id}] -> {result.code.Description}".__INFO_ECHO
        End If

        SyncLock requestPostback
            Call requestPostback.Remove(request_id)
        End SyncLock

        If TypeOf result Is messageBuffer Then
            Call sendRStudioErrDebugMessage(DirectCast(result, messageBuffer).GetErrorMessage, response)
        ElseIf TypeOf result Is bitmapBuffer Then
            Dim bytes As Byte() = result.Serialize

            Using buffer As New MemoryStream(bytes)
                bytes = buffer.UnGzipStream.ToArray
            End Using

            Call response.WriteHttp("image/png", bytes.Length)
            Call response.Write(bytes)
        ElseIf TypeOf result Is textBuffer Then
            Dim bytes As Byte() = result.Serialize

            If debug Then
#Disable Warning
                Call Console.WriteLine(vbNewLine)
                Call Console.WriteLine(DirectCast(result, textBuffer).text)
                Call Console.WriteLine(vbNewLine)
#Enable Warning
            End If

            Call response.WriteHttp("html/text", bytes.Length)
            Call response.Write(bytes)
        ElseIf TypeOf result Is dataframeBuffer Then
            Dim env As Environment = New RInterpreter().globalEnvir
            Dim dataTable As Rdataframe = DirectCast(result, dataframeBuffer).getFrame
            Dim check_x = dataTable.CheckDimension(env)
            Dim row_names As String() = dataTable.getRowNames
            Dim formatNumber As String = "G8"

            If TypeOf check_x Is Message Then
                Call sendRStudioErrDebugMessage(check_x, response)
            Else
                Dim document = DirectCast(check_x, Rdataframe).DataFrameRows(row_names, formatNumber, env)
                Dim ms As New MemoryStream

                Call StreamIO.SaveDataFrame(document, ms, Encoding.UTF8, tsv:=False, silent:=False)
                Call ms.Flush()
                Call response.WriteHttp("text/csv", ms.Length)
                Call response.Write(ms.ToArray)
                Call ms.Dispose()
                Call response.Flush()
            End If
        Else
            Call response.WriteHttp("html/text", 0)
            Call response.Write(New Byte() {})
        End If
    End Sub

    Private Sub sendRStudioErrDebugMessage(message As Message, response As HttpResponse)
        Dim err As String

        Using buffer As New MemoryStream, output As New StreamWriter(buffer)
            Call Internal.debug.writeErrMessage(message, stdout:=output, redirectError2stdout:=True)
            Call buffer.Flush()

            err = Encoding.UTF8.GetString(buffer.ToArray)
        End Using

        If showError Then
            Call response.WriteHTML(err)
        Else
            Call response.WriteError(500, err)
        End If
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Rscript">the file path of the target R# script file</param>
    ''' <param name="args">script arguments</param>
    ''' <param name="response"></param>
    Private Sub runRweb(Rscript As String, request_id$, args As Dictionary(Of String, String()), response As HttpResponse, is_background As Boolean)
        Dim argsText As String = args.GetJson.Base64String
        Dim port As Integer = localRServer.TcpPort
        Dim master As String = "localhost"
        Dim entry As String = "run"
        Dim Rslave = RscriptCommandLine.Rscript.FromEnvironment(directory:=App.HOME)

        Call args.GetJson.__DEBUG_ECHO

        ' --slave /exec <script.R> /args <json_base64> /request-id <request_id> /PORT=<port_number> [/MASTER=<ip, default=localhost> /entry=<function_name, default=NULL>]
        Dim arguments As String = Rslave.GetslaveModeCommandLine(
            exec:=Rscript,
            args:=argsText,
            request_id:=request_id,
            PORT:=port,
            master:=master,
            entry:=entry,
            internal_pipelineMode:=False,
            startups:=startups
        )

        '        If App.IsMicrosoftPlatform Then
        '            Call App.Shell(Rslave.Path, arguments, CLR:=True, debug:=True).Run()
        '        Else
        '#If netcore5 = 1 Then
        '            Call UNIX.Shell("dotnet", $"{Rslave.Path.ChangeSuffix("dll").CLIPath} {arguments}", verbose:=True)
        '#Else
        '            Call UNIX.Shell("mono", $"{Rslave.Path.CLIPath} {arguments}", verbose:=True)
        '#End If
        '        End If
        Dim task As RunSlavePipeline = Rslave.CreateSlave(arguments, workdir:=App.HOME)
        task.Shell = True
        task.Run()

        Call Console.WriteLine(task.ToString)

        If Not is_background Then
            Call pushBackResult(request_id, response)
        End If
    End Sub
End Class

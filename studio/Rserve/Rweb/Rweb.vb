#Region "Microsoft.VisualBasic::81296557efcbda6b327d3aa0c2329436, studio\Rserve\Rweb\Rweb.vb"

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

' Class Rweb
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: callback, getHttpProcessor, Run
' 
'     Sub: handleGETRequest, handleOtherMethod, handlePOSTRequest, runRweb
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Flute.Http.Core
Imports Flute.Http.Core.HttpStream
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports SMRUCC.Rsharp.System

''' <summary>
''' Rweb is not design for general web programming, it is 
''' design for running a background data task.
''' </summary>
Public Class Rweb : Inherits HttpServer

    Dim Rweb As String
    Dim showError As Boolean
    Dim requestPostback As New Dictionary(Of String, BufferObject)
    Dim socket As TcpServicesSocket

    Public Sub New(Rweb$, port As Integer, tcp As Integer, show_error As Boolean, Optional threads As Integer = -1)
        MyBase.New(port, threads)

        Me.Rweb = Rweb
        Me.showError = show_error
        Me.socket = New TcpServicesSocket(tcp) With {
            .ResponseHandler = AddressOf callback
        }
    End Sub

    Public Overrides Function Run() As Integer
        Call RunTask(AddressOf socket.Run)
        Return MyBase.Run()
    End Function

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Using response As New HttpResponse(p.outputStream, AddressOf p.writeFailure)
            ' /<scriptFileName>?...args
            Dim request As New HttpRequest(p)
            Dim Rscript As String = Rweb & "/" & request.URL.path & ".R"
            Dim is_background As Boolean = request.GetBoolean("rweb_background")
            Dim request_id As String = NextRequestId

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
                Call RunTask(Sub() Call runRweb(Rscript, request_id, request.URL.query, response, is_background))
                Call response.WriteHTML(request_id)
            Else
                Call runRweb(Rscript, request_id, request.URL.query, response, is_background)
            End If
        End Using
    End Sub

    Private Sub pushBackResult(request_id$, response As HttpResponse)
        Dim result As BufferObject = requestPostback.TryGetValue(request_id)

        Call $"get callback from slave process [{request_id}] -> {result.code.Description}".__INFO_ECHO

        SyncLock requestPostback
            Call requestPostback.Remove(request_id)
        End SyncLock

        If TypeOf result Is messageBuffer Then
            Dim err As String
            Dim message = DirectCast(result, messageBuffer).GetErrorMessage

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
        ElseIf TypeOf result Is bitmapBuffer Then
            Dim bytes As Byte() = result.Serialize

            Using buffer As New MemoryStream(bytes)
                bytes = buffer.UnGzipStream.ToArray
            End Using

            Call response.WriteHeader("image/png", bytes.Length)
            Call response.Write(bytes)
        ElseIf TypeOf result Is textBuffer Then
            Dim bytes As Byte() = result.Serialize

            Call response.WriteHeader("html/text", bytes.Length)
            Call response.Write(bytes)
        Else
            Call response.WriteHeader("html/text", 0)
            Call response.Write(New Byte() {})
        End If
    End Sub

    Public ReadOnly Property NextRequestId As String
        Get
            Return App.GetNextUniqueName("web_request__")
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Rscript">the file path of the target R# script file</param>
    ''' <param name="args">script arguments</param>
    ''' <param name="response"></param>
    Private Sub runRweb(Rscript As String, request_id$, args As Dictionary(Of String, String()), response As HttpResponse, is_background As Boolean)
        Dim argsText As String = args.GetJson.Base64String
        Dim port As Integer = socket.LocalPort
        Dim master As String = "localhost"
        Dim entry As String = "run"

        Call args.GetJson.__DEBUG_ECHO

        ' --slave /exec <script.R> /args <json_base64> /request-id <request_id> /PORT=<port_number> [/MASTER=<ip, default=localhost> /entry=<function_name, default=NULL>]
        Dim arguments As String = $"--slave /exec {Rscript.CLIPath} /request-id {request_id} /PORT={port} /MASTER={master} /entry={entry} /args ""{argsText}"""
        Dim Rslave As String = $"{App.HOME}/R#.exe"

        If App.IsMicrosoftPlatform Then
            Call App.Shell(Rslave, arguments, CLR:=True, debug:=True).Run()
        Else
            Call UNIX.Shell("mono", $"{Rslave.CLIPath} {arguments}", verbose:=True)
        End If

        If Not is_background Then
            Call pushBackResult(request_id, response)
        End If
    End Sub

    Private Function callback(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
        Using bytes As New MemoryStream(request.ChunkBuffer)
            Dim data As IPCBuffer = IPCBuffer.ParseBuffer(bytes)

            SyncLock requestPostback
                requestPostback(data.requestId) = data.buffer.data
            End SyncLock

            Call $"accept callback data: {data.ToString}".__DEBUG_ECHO

            Return New DataPipe(NetResponse.RFC_OK)
        End Using
    End Function

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Dim request As New HttpPOSTRequest(p, inputData)

        Select Case request.URL.path
            Case "callback"
                Dim requestId As String = request.URL("request")
                Dim data As HttpPostedFile = request.POSTData.files _
                    .TryGetValue("data") _
                   ?.FirstOrDefault

                Using file As FileStream = data.TempPath.Open
                    Dim buffer As Buffer = Buffer.ParseBuffer(raw:=file)

                    SyncLock requestPostback
                        requestPostback(requestId) = buffer.data
                    End SyncLock
                End Using

                Call p.writeSuccess(0)
            Case Else
                Using response As New HttpResponse(p.outputStream, AddressOf p.writeFailure)
                    ' /<scriptFileName>?...args
                    Dim Rscript As String = Rweb & "/" & request.URL.path & ".R"
                    Dim args As New Dictionary(Of String, String())(request.URL.query)
                    Dim is_background As Boolean = request.GetBoolean("rweb_background")

                    If Not Rscript.FileExists Then
                        Call p.writeFailure(404, "not allowed!")
                    Else
                        Dim form = request.POSTData.Form
                        Dim request_id As String = NextRequestId

                        For Each formKey In form.AllKeys
                            args(formKey) = form.GetValues(formKey)
                        Next

                        If is_background Then
                            Call RunTask(Sub() Call runRweb(Rscript, request_id, request.URL.query, response, is_background))
                            Call response.WriteHTML(request_id)
                        Else
                            Call runRweb(Rscript, request_id, request.URL.query, response, is_background)
                        End If
                    End If
                End Using
        End Select
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Call p.writeFailure(404, "not allowed!")
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize%) As HttpProcessor
        Return New HttpProcessor(client, Me, MAX_POST_SIZE:=bufferSize)
    End Function
End Class

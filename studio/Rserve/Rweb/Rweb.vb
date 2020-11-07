#Region "Microsoft.VisualBasic::51b6c5dba57d9e06cd78b782b93acb05, studio\Rserve\Rweb\Rweb.vb"

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
'     Function: getHttpProcessor
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
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Serialize

''' <summary>
''' Rweb is not design for general web programming, it is 
''' design for running a background data task.
''' </summary>
Public Class Rweb : Inherits HttpServer

    Dim Rweb As String
    Dim showError As Boolean
    Dim requestPostback As New Dictionary(Of String, BufferObject)

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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Rscript">the file path of the target R# script file</param>
    ''' <param name="args">script arguments</param>
    ''' <param name="response"></param>
    Private Sub runRweb(Rscript As String, args As Dictionary(Of String, String()), response As HttpResponse)
        Dim argsText As String = args.GetJson.Base64String
        Dim request_id As String = App.GetNextUniqueName("web_request__")
        Dim port As Integer = Me.localPort
        Dim master As String = "localhost"
        Dim entry As String = "run"

        Call args.GetJson.__DEBUG_ECHO

        ' --slave /exec <script.R> /args <json_base64> /request-id <request_id> /PORT=<port_number> [/MASTER=<ip, default=localhost> /entry=<function_name, default=NULL>]
        Dim arguments As String = $"--slave /exec {Rscript.CLIPath} /args ""{argsText}"" /request-id {request_id} /PORT={port} /MASTER={master} /entry={entry}"
        Dim Rslave As String = $"{App.HOME}/R#.exe"

        If App.IsMicrosoftPlatform Then
            Call App.Shell(Rslave, arguments, CLR:=True).Run()
        Else
            Call UNIX.Shell("mono", $"{Rslave.CLIPath} {arguments}", verbose:=True)
        End If

        Dim result As BufferObject = requestPostback.TryGetValue(request_id)

        SyncLock requestPostback
            Call requestPostback.Remove(request_id)
        End SyncLock

        If TypeOf result Is messageBuffer Then
            Dim err As String
            Dim message = DirectCast(result, messageBuffer).GetErrorMessage

            Using buffer As New MemoryStream, output As New StreamWriter(buffer)
                Call Internal.debug.writeErrMessage(message, stdout:=output, redirectError2stdout:=False)
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
                Call p.writeFailure(404, "not allowed!")
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

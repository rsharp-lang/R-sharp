#Region "Microsoft.VisualBasic::90da4289367998ca63ae3275de5d16d8, studio\Rserver\Rweb\Rweb.vb"

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

    '   Total Lines: 171
    '    Code Lines: 128
    ' Comment Lines: 15
    '   Blank Lines: 28
    '     File Size: 6.03 KB


    ' Class Rweb
    ' 
    '     Properties: access, config, fs, NextRequestId, Processor
    '                 TcpPort
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: callback, getHttpProcessor, ParseJSON, Run
    ' 
    '     Sub: handleGETRequest, handleOtherMethod, handlePOSTRequest, SetAccessControl, SetFileSystem
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports Flute.Http
Imports Flute.Http.Configurations
Imports Flute.Http.Core
Imports Flute.Http.Core.HttpStream
Imports Flute.Http.Core.Message
Imports Flute.Http.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Runtime.Serialize

''' <summary>
''' Rweb is not design for general web programming, it is 
''' design for running a background data task.
''' </summary>
Public Class Rweb : Inherits HttpServer

    Dim socket As TcpServicesSocket

    ''' <summary>
    ''' a wrapper of the local filesystem object for hosting the static files, example as html file, image files, etc
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property fs As FileSystem
    Public ReadOnly Property access As AccessController

    Public ReadOnly Property Processor As RProcessor

    Public ReadOnly Property config As Configuration
        Get
            Return _settings
        End Get
    End Property

    ''' <summary>
    ''' next unique request id
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property NextRequestId As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return App.GetNextUniqueName("web_request__") & Now.ToString.MD5.Substring(2, 4)
        End Get
    End Property

    Public ReadOnly Property TcpPort As Integer
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return socket.LocalPort
        End Get
    End Property

    Public Sub New(Rweb$,
                   port As Integer,
                   tcp As Integer,
                   show_error As Boolean,
                   Optional threads As Integer = -1,
                   Optional debug As Boolean = False,
                   Optional configs As Configuration = Nothing)

        MyBase.New(port, threads, configs)

        Me.Processor = New RProcessor(Me, Rweb, show_error, debug)
        Me.socket = New TcpServicesSocket(tcp) With {
            .ResponseHandler = AddressOf callback
        }
    End Sub

    Public Sub SetFileSystem(fs As FileSystem)
        _fs = fs
    End Sub

    Public Sub SetAccessControl(access As AccessController)
        _access = access
    End Sub

    Public Overrides Function Run() As Integer
        Call RunTask(AddressOf socket.Run)
        Return MyBase.Run()
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Call Processor.RscriptHttpGet(p)
    End Sub

    Private Function callback(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
        Using bytes As New MemoryStream(request.ChunkBuffer)
            Dim data As IPCBuffer = IPCBuffer.ParseBuffer(bytes)

            Call $"accept callback data: {data.ToString}".__DEBUG_ECHO
            Call Processor.SaveResponse(data.requestId, data.buffer)

            Return New DataPipe(NetResponse.RFC_OK)
        End Using
    End Function

    Private Shared Function ParseJSON(json As String) As Dictionary(Of String, Object)
        Dim raw As JsonElement = RJSON.ParseJSONinternal(json, raw:=True, strict_vector_syntax:=False, Nothing)

        If raw Is Nothing Then
            Return New Dictionary(Of String, Object)
        End If

        If TypeOf raw Is JsonObject Then
            Dim obj As New Dictionary(Of String, Object)
            Dim list As JsonObject = DirectCast(raw, JsonObject)

            For Each name As String In list.ObjectKeys
                Call obj.Add(name, list(name))
            Next

            Return obj
        Else
            Return New Dictionary(Of String, Object) From {
                {"data", raw}
            }
        End If
    End Function

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Dim request As New HttpPOSTRequest(p, inputData, parseJSON:=AddressOf ParseJSON)

        Select Case request.URL.path
            Case "callback"
                ' IPC slave node post back the result data for
                ' a specific background task
                ' the task is referenced via a unique request id
                Dim requestId As String = request.URL("request")
                Dim data As HttpPostedFile = request.POSTData.files _
                    .TryGetValue("data") _
                   ?.FirstOrDefault

                Using file As FileStream = data.TempPath.Open
                    Call Buffer _
                        .ParseBuffer(raw:=file) _
                        .DoCall(Sub(buf)
                                    Call Processor.SaveResponse(requestId, buf)
                                End Sub)
                End Using

                Call p.writeSuccess(0)
            Case Else
                Using response As New HttpResponse(p) With {
                    .AccessControlAllowOrigin = "*"
                }
                    Call Processor.RscriptHttpPost(request, response)
                End Using
        End Select
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Using response As New HttpResponse(p) With {
            .AccessControlAllowOrigin = "*"
        }
            Call response.WriteHTML("passed")
            Call p.outputStream.Flush()
        End Using
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize%) As HttpProcessor
        Return New HttpProcessor(client, Me, MAX_POST_SIZE:=bufferSize, _settings)
    End Function
End Class

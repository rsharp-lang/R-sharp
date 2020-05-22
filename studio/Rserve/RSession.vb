#Region "Microsoft.VisualBasic::ef6970ef19e1555d085e5b8d94c3fb54, studio\Rserve\RSession.vb"

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

' Class RSession
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: getHttpProcessor
' 
'     Sub: handleGETRequest, handleOtherMethod, handlePOSTRequest
' 
' Class RSessionBackend
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: handleRScript, requiredDataURI
' 
'     Sub: RunCode
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Sockets
Imports System.Text
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.System.Configuration
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Public Class RSession : Inherits HttpServer

    ReadOnly R As RSessionBackend

    Public Sub New(port As Integer,
                   Optional workspace$ = "./",
                   Optional showError As Boolean = True)

        Call MyBase.New(port, App.CPUCoreNumbers)

        R = New RSessionBackend(workspace, showError)
    End Sub

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Dim request As New HttpRequest(p)

        Select Case request.URL.path
            Case "exec"
                Dim script As New StringBuilder

                For Each line As String In request.URL.GetValues("script")
                    script.AppendLine(line & ";")
                Next

                Call R.RunCode(script.ToString, p.openResponseStream)
            Case "inspect"
                ' 获取指定uid对象的json数据用于前端查看
                Call R.InspectObject(request.URL.getArgumentVal("guid"), p.openResponseStream)

        End Select
    End Sub

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Dim post As New HttpPOSTRequest(p, inputData)

        Select Case post.URL.path
            Case "exec"
                Dim script As String = post.POSTData.Objects("script")
                Dim output As HttpResponse = p.openResponseStream

                Call R.RunCode(script, output)
        End Select
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Call p.writeFailure(404, "method not found")
    End Sub

    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize As Integer) As HttpProcessor
        Return New HttpProcessor(client, Me, bufferSize)
    End Function
End Class

Public Class RSessionBackend

    ReadOnly R As RInterpreter
    ReadOnly inspector As New Dictionary(Of String, Byte())

    Public Sub New(Optional workspace$ = "./", Optional showError As Boolean = True)
        Me.R = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Me.R.silent = True
        Me.R.redirectError2stdout = showError

        For Each pkgName As String In R.configFile.GetStartupLoadingPackages
            Call R.LoadLibrary(packageName:=pkgName, silent:=True)
        Next

        With $"{workspace}/.RData"
            If .FileExists(True) Then

            End If
        End With
    End Sub

    Public Sub InspectObject(guid$, output As HttpResponse)
        SyncLock inspector
            If inspector.ContainsKey(guid) Then
                Dim buffer As Byte() = inspector(guid)

                output.WriteHeader("text/json", buffer.Length)
                output.Write(buffer)
                output.Flush()

                inspector.Remove(guid)
            Else
                output.WriteError(404, "object not found!")
            End If
        End SyncLock
    End Sub

    Private Function handleRScript(scriptText$, Rstd_out As StreamWriter) As RInvoke
        Dim invokeRtvl As New RInvoke
        Dim result As Object

        result = R.RedirectOutput(Rstd_out, OutputEnvironments.Html).Evaluate(scriptText)

        If RProgram.isException(result) Then
            invokeRtvl.code = 500
            invokeRtvl.err = result
        Else
            invokeRtvl.code = 0

            If Not result Is Nothing Then
                ' 在终端显示最后的结果值
                R.Evaluate($"print({RInterpreter.lastVariableName});")
            End If
        End If

        If R.globalEnvir.stdout.recommendType Is Nothing Then
            invokeRtvl.content_type = "text/html"
        Else
            invokeRtvl.content_type = R.globalEnvir.stdout.recommendType
        End If

        Call Rstd_out.Flush()

        Return invokeRtvl
    End Function

    Private Shared Function requiredDataURI(result As RInvoke) As Boolean
        Static exclude_types As String() = {"text/html", "text/json", "text/xml", "text/csv", "application/json"}

        SyncLock exclude_types
            Return Not exclude_types _
                .All(Function(name)
                         Return result.content_type.StartsWith(name)
                     End Function)
        End SyncLock
    End Function

    Public Sub RunCode(scriptText As String, response As HttpResponse)
        Dim result As RInvoke

        Using output As New MemoryStream(), Rstd_out As New StreamWriter(output, Encodings.UTF8WithoutBOM.CodePage)
            result = handleRScript(scriptText, Rstd_out)

            If result.content_type = "inspector/json" OrElse result.content_type = "inspector/csv" Then
                Dim guid As String = App.NextTempName

                inspector.Add(guid, output.ToArray)
                result.info = guid
            Else
                ' 后端的输出应该包含有两部分的内容
                ' 终端输出的文本
                ' 以及最后的值
                If requiredDataURI(result) Then
                    result.info = New DataURI(base64:=output.ToArray.ToBase64String, mime:=result.content_type).ToString
                Else
                    result.info = output.ToArray.ToBase64String
                End If
            End If

            result.warnings = R.globalEnvir.messages.PopAll
        End Using

        Dim json As String = JSONSerializer.GetJson(result, enumToStr:=True)
        Dim buffer As Byte() = Encodings.UTF8WithoutBOM.CodePage.GetBytes(json)

        response.AccessControlAllowOrigin = "*"

        Call response.WriteHeader("application/json", buffer.Length)
        Call response.Write(buffer)
    End Sub
End Class

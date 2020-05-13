#Region "Microsoft.VisualBasic::44747715f0b0aeb1ac1dd1a772767cf2, studio\Rserve\RSession.vb"

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
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Sockets
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Configuration
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Public Class RSession : Inherits HttpServer

    ReadOnly R As RInterpreter

    Public Sub New(port As Integer,
                   Optional workspace$ = "./",
                   Optional showError As Boolean = True)

        Call MyBase.New(port, App.CPUCoreNumbers)

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

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Dim request As New HttpRequest(p)

        Select Case request.URL.path
            Case "exec"
                Call runCode(request.URL("script"), New HttpResponse(p.outputStream, AddressOf p.writeFailure))
        End Select
    End Sub

    Private Sub runCode(scriptText As String, response As HttpResponse)
        Dim invokeRtvl As New RInvoke

        Using output As New MemoryStream(), Rstd_out As New StreamWriter(output, Encodings.UTF8WithoutBOM.CodePage)
            Dim result As Object
            Dim content_type As String

            result = R.RedirectOutput(Rstd_out).Evaluate(scriptText)

            If RProgram.isException(result) Then
                invokeRtvl.code = 500
                invokeRtvl.err = result
            Else
                invokeRtvl.code = 0
            End If

            If R.globalEnvir.stdout.recommendType Is Nothing Then
                content_type = "text/html"
            Else
                content_type = R.globalEnvir.stdout.recommendType
            End If

            Call Rstd_out.Flush()

            invokeRtvl.info = output.ToArray.ToBase64String
            invokeRtvl.warnings = R.globalEnvir.messages.PopAll
            invokeRtvl.content_type = content_type
        End Using

        ' Dim json As String = invokeRtvl.GetJson(knownTypes:={GetType(Message), GetType(MSG_TYPES), GetType(StackFrame)})
        Dim json As String = JSONSerializer.GetJson(invokeRtvl, enumToStr:=True)
        Dim buffer As Byte() = Encodings.UTF8WithoutBOM.CodePage.GetBytes(json)

        Call response.WriteHeader("application/json", buffer.Length)
        Call response.Write(buffer)
    End Sub

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Dim post As New HttpPOSTRequest(p, inputData)

        Select Case post.URL.path
            Case "exec"

        End Select
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Call p.writeFailure(404, "method not found")
    End Sub

    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize As Integer) As HttpProcessor
        Return New HttpProcessor(client, Me, bufferSize)
    End Function
End Class


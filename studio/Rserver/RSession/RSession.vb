#Region "Microsoft.VisualBasic::b4ade6cd231744caccd7640c64877533, E:/GCModeller/src/R-sharp/studio/Rserver//RSession/RSession.vb"

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

    '   Total Lines: 57
    '    Code Lines: 41
    ' Comment Lines: 1
    '   Blank Lines: 15
    '     File Size: 1.89 KB


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

Imports System.Net.Sockets
Imports System.Text
Imports Flute.Http.Core
Imports Flute.Http.Core.Message

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

#Region "Microsoft.VisualBasic::9a81c9dc88f0e5c3300bb3c84ecf81e8, F:/GCModeller/src/R-sharp/studio/Rstudio//HttpServer.vb"

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

    '   Total Lines: 185
    '    Code Lines: 116
    ' Comment Lines: 48
    '   Blank Lines: 21
    '     File Size: 6.43 KB


    ' Module HttpServer
    ' 
    '     Function: createDriver, customResponseHeader, getHeaders, getHttpRaw, getUrl
    '               httpMethod, parseUrl, serve, urlList
    ' 
    '     Sub: httpError, pushDownload
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Flute.Http
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("http", Category:=APICategories.UtilityTools)>
Public Module HttpServer

    ''' <summary>
    ''' create a http driver module
    ''' </summary>
    ''' <param name="silent"></param>
    ''' <returns></returns>
    <ExportAPI("http_socket")>
    Public Function createDriver(Optional silent As Boolean = True) As HttpDriver
        Return New HttpDriver(silent)
    End Function

    ''' <summary>
    ''' get http headers data from the browser request
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    <ExportAPI("getHeaders")>
    Public Function getHeaders(req As HttpRequest) As list
        Return New list(RType.GetRSharpType(GetType(String))) With {
            .slots = req.HttpHeaders _
                .ToDictionary(Function(h) h.Key,
                              Function(h)
                                  Return CObj(h.Value)
                              End Function)
        }
    End Function

    ''' <summary>
    ''' get url data from the browser request 
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    <ExportAPI("getUrl")>
    Public Function getUrl(req As HttpRequest) As list
        Return urlList(req.URL)
    End Function

    Private Function urlList(url As URL) As list
        Dim queryData As New list(RType.GetRSharpType(GetType(String))) With {
            .slots = url.query _
                .ToDictionary(Function(q) q.Key,
                              Function(q)
                                  Return CObj(q.Value)
                              End Function)
        }

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"url", url.ToString},
                {"path", url.path},
                {"hostName", url.hostName},
                {"hash", url.hashcode},
                {"query", queryData},
                {"port", url.port},
                {"protocol", url.protocol}
            }
        }
    End Function

    <ExportAPI("parseUrl")>
    Public Function parseUrl(url As String) As list
        Return urlList(New URL(url))
    End Function

    ''' <summary>
    ''' get the raw http request header from the browser request
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    <ExportAPI("getHttpRaw")>
    Public Function getHttpRaw(req As HttpRequest) As String
        Return req.HttpRequest.raw
    End Function

    ''' <summary>
    ''' add custom http response headers
    ''' </summary>
    ''' <param name="driver"></param>
    ''' <param name="headers"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("headers")>
    Public Function customResponseHeader(driver As HttpDriver,
                                         <RListObjectArgument>
                                         headers As list,
                                         Optional env As Environment = Nothing) As HttpDriver

        For Each header As KeyValuePair(Of String, String) In headers.AsGeneric(Of String)(env)
            Call driver.AddResponseHeader(header.Key, header.Value)
        Next

        Return driver
    End Function

    ''' <summary>
    ''' list to the specific tcp port and run the R# http web server
    ''' </summary>
    ''' <param name="driver"></param>
    ''' <param name="port%"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("listen")>
    Public Function serve(driver As HttpDriver,
                          Optional port% = -1,
                          Optional env As Environment = Nothing) As Integer

        Dim httpPort As Integer = If(port <= 0, Rnd() * 30000, port)
        Dim socket As HttpSocket = driver.GetSocket(httpPort)
        Dim localUrl$ = $"http://localhost:{httpPort}/"

        If env.globalEnvironment.debugMode Then
            Call New Thread(
                Sub()
                    Call Thread.Sleep(3000)
                    Call Process.Start(localUrl)
                End Sub) _
                         _
            .Start()
        End If

        Return socket.Run()
    End Function

    ''' <summary>
    ''' write http error code and send error response to browser
    ''' </summary>
    ''' <param name="write"></param>
    ''' <param name="code"></param>
    ''' <param name="message"></param>
    <ExportAPI("httpError")>
    Public Sub httpError(write As HttpResponse, code As Integer, message As String)
        Call write.WriteError(code, message)
    End Sub

    ''' <summary>
    ''' set http method handler to the R# web server
    ''' </summary>
    ''' <param name="driver"></param>
    ''' <param name="method"></param>
    ''' <param name="process"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("httpMethod")>
    Public Function httpMethod(driver As HttpDriver,
                               method As String,
                               process As RFunction,
                               Optional accessAny As Boolean = False,
                               Optional env As Environment = Nothing) As HttpDriver

        Call driver.HttpMethod(
            method:=method.ToUpper,
            handler:=Sub(req, response)
                         If accessAny Then
                             response.AccessControlAllowOrigin = "*"
                         End If

                         Call process.Invoke({req, response}, env)
                     End Sub)

        Return driver
    End Function

    <ExportAPI("pushDownload")>
    Public Sub pushDownload(response As HttpResponse, filename As String)
        If Not filename.FileExists Then
            Call response.WriteError(404, "not found!")
        Else
            Call response.SendFile(filename)
        End If
    End Sub
End Module

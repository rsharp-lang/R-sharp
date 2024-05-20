#Region "Microsoft.VisualBasic::103aa5c5be1d41b0b4f04ad5c093a0a6, studio\Rserver\Rweb\RCallbackMessage.vb"

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

    '   Total Lines: 168
    '    Code Lines: 141
    ' Comment Lines: 0
    '   Blank Lines: 27
    '     File Size: 7.09 KB


    ' Module RCallbackMessage
    ' 
    '     Sub: sendDataframe, SendHttpResponseMessage, sendImage, sendListObject, sendRawdata
    '          sendRStudioErrDebugMessage, sendText, sendVector
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.[Object].dataframe
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

Module RCallbackMessage

    Private Sub sendRStudioErrDebugMessage(message As Message, response As HttpResponse, showError As Boolean)
        Dim err As String

        Using buffer As New MemoryStream, output As New StreamWriter(buffer)
            Call Internal.debug.writeErrMessage(message, stdout:=output, redirectError2stdout:=True)
            Call buffer.Flush()

            err = Encoding.UTF8.GetString(buffer.ToArray)
        End Using

        If showError Then
            Call response.WriteHTML(err)
        Else
            Call response.WriteError(HTTP_RFC.RFC_INTERNAL_SERVER_ERROR, err)
        End If
    End Sub

    <Extension>
    Private Sub sendImage(result As bitmapBuffer, request As HttpRequest, response As HttpResponse, debug As Boolean, showErr As Boolean)
        Dim bytes As Byte() = result.Serialize

        Using buffer As New MemoryStream(bytes)
            bytes = buffer.UnGzipStream.ToArray

            If request.URL(query:="base64").ParseBoolean Then
                Dim base64 As New DataURI(New MemoryStream(bytes), mime:="image/png")
                Dim str As String = base64.ToString

                bytes = Encoding.ASCII.GetBytes(str)

                Call response.WriteHttp("image/png;charset=utf-8;base64", bytes.Length)
                Call response.Write(bytes)
            Else
                Call response.WriteHttp("image/png", bytes.Length)
                Call response.Write(bytes)
            End If
        End Using
    End Sub

    <Extension>
    Private Sub sendText(text As textBuffer, request As HttpRequest, response As HttpResponse, debug As Boolean, showErr As Boolean)
        Dim bytes As Byte() = Encoding.UTF8.GetBytes(text.text)

        If debug Then
#Disable Warning
            Call Console.WriteLine(vbNewLine)
            Call Console.WriteLine(text.text)
            Call Console.WriteLine(vbNewLine)
#Enable Warning
        End If

        Call response.WriteHttp(text.mime, bytes.Length)
        Call response.Write(bytes)
    End Sub

    <Extension>
    Private Sub sendDataframe(df As dataframeBuffer, request As HttpRequest, response As HttpResponse, debug As Boolean, showErr As Boolean)
        Dim env As Environment = New RInterpreter().globalEnvir
        Dim dataTable As Rdataframe = df.getFrame
        Dim check_x = dataTable.CheckDimension(env)
        Dim row_names As String() = dataTable.getRowNames
        Dim formatNumber As String = "G8"

        If TypeOf check_x Is Message Then
            Call sendRStudioErrDebugMessage(check_x, response, showErr)
        Else
            Dim document = DirectCast(check_x, Rdataframe).DataFrameRows(row_names, formatNumber, env)
            Dim ms As New MemoryStream

            If document Like GetType(Message) Then
                Call sendRStudioErrDebugMessage(document.TryCast(Of Message), response, showErr)
                Return
            End If

            Call StreamIO.SaveDataFrame(
                csv:=document.TryCast(Of csv).ToArray,
                file:=ms,
                encoding:=Encoding.UTF8,
                tsv:=df.tsv,
                silent:=False,
                autoCloseFile:=False
            )

            Call ms.Flush()
            Call response.WriteHttp("text/csv", ms.Length)
            Call response.Write(ms.ToArray)
            Call ms.Dispose()
            Call response.Flush()
        End If
    End Sub

    <Extension>
    Private Sub sendVector(vec As vectorBuffer, request As HttpRequest, response As HttpResponse, debug As Boolean, showErr As Boolean)
        Dim vector As vector = vec.getVector
        Dim json As Object = jsonlite.toJSON(vector, New RInterpreter().globalEnvir)

        If TypeOf json Is Message Then
            Call sendRStudioErrDebugMessage(json, response, showErr)
        Else
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(CStr(json))

            Call response.WriteHttp(MIME.JSONText, bytes.Length)
            Call response.Write(bytes)
        End If
    End Sub

    <Extension>
    Private Sub sendListObject(list As listBuffer, request As HttpRequest, response As HttpResponse, debug As Boolean, showErr As Boolean)
        Dim tuples As list = list.listData
        Dim json As Object = jsonlite.toJSON(tuples, New RInterpreter().globalEnvir)

        If TypeOf json Is Message Then
            Call sendRStudioErrDebugMessage(json, response, showErr)
        Else
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(CStr(json))

            Call response.WriteHttp(MIME.JSONText, bytes.Length)
            Call response.Write(bytes)
        End If
    End Sub

    <Extension>
    Private Sub sendRawdata(raw As rawBuffer, request As HttpRequest, response As HttpResponse, debug As Boolean, showErr As Boolean)
        Dim bytes As Byte() = raw.buffer.ToArray

        Call response.WriteHttp(MIME.MsDownload, bytes.Length)
        Call response.Write(bytes)
    End Sub

    Public Sub SendHttpResponseMessage(result As BufferObject, request As HttpRequest, response As HttpResponse, debug As Boolean, showErr As Boolean)
        If TypeOf result Is messageBuffer Then
            Call sendRStudioErrDebugMessage(DirectCast(result, messageBuffer).GetErrorMessage, response, showErr)
        ElseIf TypeOf result Is bitmapBuffer Then
            Call DirectCast(result, bitmapBuffer).sendImage(request, response, debug, showErr)
        ElseIf TypeOf result Is textBuffer Then
            Call DirectCast(result, textBuffer).sendText(request, response, debug, showErr)
        ElseIf TypeOf result Is dataframeBuffer Then
            Call DirectCast(result, dataframeBuffer).sendDataframe(request, response, debug, showErr)
        ElseIf TypeOf result Is vectorBuffer Then
            Call DirectCast(result, vectorBuffer).sendVector(request, response, debug, showErr)
        ElseIf TypeOf result Is listBuffer Then
            Call DirectCast(result, listBuffer).sendListObject(request, response, debug, showErr)
        ElseIf TypeOf result Is rawBuffer Then
            Call DirectCast(result, rawBuffer).sendRawdata(request, response, debug, showErr)
        Else
            Call response.WriteError(HTTP_RFC.RFC_NOT_IMPLEMENTED, $"send http response for '{result.GetType.FullName}' not implemented yet!")
        End If
    End Sub
End Module

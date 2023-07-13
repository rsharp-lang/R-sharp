#Region "Microsoft.VisualBasic::e78d9243dc3fd1ef6030d561e6b1cb9f, G:/GCModeller/src/R-sharp/studio/Rserver//Rweb/RCallbackMessage.vb"

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

    '   Total Lines: 106
    '    Code Lines: 90
    ' Comment Lines: 0
    '   Blank Lines: 16
    '     File Size: 4.30 KB


    ' Module RCallbackMessage
    ' 
    '     Sub: SendHttpResponseMessage, sendRStudioErrDebugMessage
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Flute.Http.Core
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Net.HTTP
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.[Object].dataframe

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
            Call response.WriteError(500, err)
        End If
    End Sub

    Public Sub SendHttpResponseMessage(result As BufferObject, request As HttpRequest, response As HttpResponse, debug As Boolean, showErr As Boolean)
        If TypeOf result Is messageBuffer Then
            Call sendRStudioErrDebugMessage(DirectCast(result, messageBuffer).GetErrorMessage, response, showErr)
        ElseIf TypeOf result Is bitmapBuffer Then
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
                Call sendRStudioErrDebugMessage(check_x, response, showErr)
            Else
                Dim document = DirectCast(check_x, Rdataframe).DataFrameRows(row_names, formatNumber, env)
                Dim ms As New MemoryStream

                If document Like GetType(Message) Then
                    Call sendRStudioErrDebugMessage(document.TryCast(Of Message), response, showErr)
                    Return
                End If

                Call StreamIO.SaveDataFrame(
                    csv:=document,
                    file:=ms,
                    encoding:=Encoding.UTF8,
                    tsv:=DirectCast(result, dataframeBuffer).tsv,
                    silent:=False,
                    autoCloseFile:=False
                )

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
End Module

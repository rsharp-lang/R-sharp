#Region "Microsoft.VisualBasic::357c19845733ca306d6a0d60e7b7532f, studio\R-terminal\CLI\IPC.vb"

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

' Module CLI
' 
'     Function: postResult, slaveMode
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports SMRUCC.Rsharp.System.Configuration

Partial Module CLI

    <ExportAPI("--slave")>
    <Description("Run the specific R# script file, and then post back the result data json to the specific master listener.")>
    <Usage("--slave /exec <script.R> /args <json_base64> /request-id <request_id> /PORT=<port_number> [/MASTER=<ip, default=localhost> /entry=<function_name, default=NULL>]")>
    <Argument("/exec", False, CLITypes.File, AcceptTypes:={GetType(String)}, Extensions:="*.R", Description:="a specific R# script for run")>
    <Argument("/args", False, CLITypes.Base64, PipelineTypes.std_in,
              AcceptTypes:={GetType(Dictionary(Of String, String))},
              Extensions:="*.json",
              Description:="The base64 text of the input arguments for running current R# script file, this is a json encoded text of the arguments.")>
    <Argument("/entry", True, CLITypes.String, AcceptTypes:={GetType(String)},
              Description:="the entry function name, by default is running the script from the begining to ends.")>
    Public Function slaveMode(args As CommandLine) As Integer
        Dim script As String = args <= "/exec"
        Dim arguments As Dictionary(Of String, String) = args("/args") _
            .Base64Decode _
            .LoadJSON(Of Dictionary(Of String, String))
        Dim port As Integer = args <= "/PORT"
        Dim master As String = args <= "/MASTER" Or "localhost"
        Dim entry As String = args <= "/entry"
        Dim request_id As String = args <= "/request-id"
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim parameters As NamedValue(Of Object)() = arguments _
            .Select(Function(a)
                        Return New NamedValue(Of Object) With {
                            .Name = a.Key,
                            .Value = a.Value
                        }
                    End Function) _
            .ToArray

        For Each pkgName As String In R.configFile.GetStartupLoadingPackages
            Call R.LoadLibrary(packageName:=pkgName)
        Next

        Dim result As Object = R.Source(script, parameters)

        If TypeOf result Is Message Then
            Return R.globalEnvir.postResult(result, master, port, request_id)
        ElseIf Not entry.StringEmpty Then
            result = R.Invoke(entry, parameters)
        End If

        ' post result data back to the master node
        Return R.globalEnvir.postResult(result, master, port, request_id)
    End Function

    <Extension>
    Private Function postResult(env As Environment, result As Object, master As String, port As Integer, request_id As String) As Integer
        Dim buffer As New Buffer
        Dim url As String = $"http://{master}:{port}/callback?request=${request_id}"

        If result Is Nothing Then
            buffer.data = rawBuffer.getEmptyBuffer
        ElseIf TypeOf result Is dataframe Then
            Throw New NotImplementedException(result.GetType.FullName)
        ElseIf TypeOf result Is vector Then
            buffer.data = vectorBuffer.CreateBuffer(DirectCast(result, vector), env)
        ElseIf TypeOf result Is list Then
            Throw New NotImplementedException(result.GetType.FullName)
        ElseIf TypeOf result Is Message Then
            buffer.data = New messageBuffer(DirectCast(result, Message))
        ElseIf TypeOf result Is BufferObject Then
            buffer.data = DirectCast(result, BufferObject)
        Else
            Throw New NotImplementedException(result.GetType.FullName)
        End If

        Using form As New MultipartForm
            Call form.Add("data", buffer.Serialize, "data.bin")
            Call form.POST(url)
        End Using

        If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
            Return DirectCast(result, Message).level
        Else
            Return 0
        End If
    End Function
End Module

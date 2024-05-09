#Region "Microsoft.VisualBasic::c02a4fbc01658152c432513d9f779687, E:/GCModeller/src/R-sharp/Rscript//CLI/IPC.vb"

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

    '   Total Lines: 300
    '    Code Lines: 238
    ' Comment Lines: 23
    '   Blank Lines: 39
    '     File Size: 12.44 KB


    ' Module CLI
    ' 
    '     Function: postResult, slaveMode, tryHandleJSON
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Net
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports IPEndPoint = Microsoft.VisualBasic.Net.IPEndPoint

Partial Module CLI

    <Extension>
    Private Function tryHandleJSON(json As String) As Dictionary(Of String, String())
        If json.StringEmpty Then
            Return New Dictionary(Of String, String())
        End If

        Try
            Return json.LoadJSON(Of Dictionary(Of String, String()))
        Catch ex As Exception
            Dim stringVals As Dictionary(Of String, String) = json.LoadJSON(Of Dictionary(Of String, String))
            Dim array As New Dictionary(Of String, String())

            For Each item In stringVals
                array.Add(item.Key, {item.Value})
            Next

            Return array
        End Try
    End Function

    ''' <summary>
    ''' R web server run a slave task
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--slave")>
    <Usage("--slave /exec <script.R> 
                    /argvs <json_base64> 
                    /request-id <request_id> 
                    /PORT=<port_number> [
                    
                    /timeout=<timeout in ms, default=1000> 
                    /retry=<retry_times, default=5> 
                    /MASTER=<ip, default=localhost> 
                    /entry=<function_name, default=run>

                    --debug 
                    --startups <packageNames, default="""">  
                    --attach <debug_pkg_dir>
                    --std_in <input_type: opt/arg>
    ]")>
    <Description("Create a R# cluster node for run background or parallel task. 
                  This IPC command will run a R# script file that specified by the ``/exec`` argument,
                  and then post back the result data json to the specific master listener.")>
    <Argument("/exec", False, CLITypes.File, AcceptTypes:={GetType(String)}, Extensions:="*.R",
              Description:="a specific R# script for run as background task.")>
    <Argument("/argvs", False, CLITypes.Base64, PipelineTypes.std_in,
              AcceptTypes:={GetType(Dictionary(Of String, String))},
              Extensions:="*.json",
              Description:="The base64 text of the input arguments for running current R# script file, this is a json encoded text of the arguments. the json object should be a collection of [key => value[]] pairs.")>
    <Argument("/entry", True, CLITypes.String, AcceptTypes:={GetType(String)},
              Description:="the entry function name, by default is running the script from the begining to ends.")>
    <Argument("/request-id", False, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="the unique id for identify current slave progress in the master node when invoke post data callback.")>
    <Argument("/MASTER", True, CLITypes.String,
              AcceptTypes:={GetType(IPAddress)},
              Description:="the ip address of the master node, by default this parameter value is ``localhost``.")>
    <Argument("/PORT", False, CLITypes.Integer,
              AcceptTypes:={GetType(Integer)},
              Description:="the port number for master node listen to this callback post data.")>
    <Argument("/retry", False, CLITypes.Integer,
              AcceptTypes:={GetType(Integer)},
              Description:="How many times that this cluster node should retry to send callback data if the TCP request timeout.")>
    <Argument("--startups", False, CLITypes.String,
              AcceptTypes:={GetType(String())},
              Description:="A list of package names for load during the current slave process startup.")>
    <Argument("--std_in", True, CLITypes.String,
              AcceptTypes:={GetType(String)},
              Description:="Should this IPC slave node open the standard input for read the input data? 
                            There are two kind of data operation: 

                    opt - for use the std input data for ``options`` function set global options data; 
                    arg - for use the std input data as the function parameter value.
              
              the standard input data is encoded in multipart form data.
              ")>
    Public Function slaveMode(args As CommandLine) As Integer
        Dim script As String = args <= "/exec"
        Dim arguments As Dictionary(Of String, String()) = args("/argvs") _
            .Base64Decode(ungzip:=True) _
            .tryHandleJSON
        Dim port As Integer = CInt(Val(args <= "/PORT"))
        Dim master As String = args("/MASTER") Or "localhost"
        Dim entry As String = args("/entry") Or "run"
        ' the unique web request id
        Dim request_id As String = args <= "/request-id"
        Dim retryTimes As Integer = args("/retry") Or 5
        Dim timeout As Double = args("/timeout") Or 1000
        Dim isDebugMode As Boolean = args("--debug")
        Dim startups As String() = args("--startups") _
            .Split("[,;]\s*", regexp:=True) _
            .SafeQuery _
            .Where(Function(str) Not str.StringEmpty) _
            .ToArray
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)
        Dim result As Object = Nothing
        Dim upstream As New IPEndPoint(master, port)
        Dim pkg_attach As String = args("--attach")

        If Not script.FileExists Then
            Dim msgErr As String = $"R# script file '{script.GetFullPath}' is not found on your filesystem!"

            Call msgErr.PrintException

            Return R.globalEnvir.postResult(
                result:=Internal.debug.stop({
                    msgErr,
                    $"file: {script}",
                    $"workdir: {App.CurrentDirectory}",
                    $"fullName: {script.GetFullPath}"
                }, envir:=R.globalEnvir,
                   suppress:=True
                ),
                master:=upstream,
                request_id:=request_id,
                retryTimes:=retryTimes,
                timeoutMS:=timeout,
                isDebugMode:=isDebugMode
            )
        Else
            If isDebugMode Then
                Call VBDebugger.EchoLine($"found script at location: {script.GetFullPath}")
            End If

            ' add variable values into environment for debug used
            For Each arg In arguments
                If isDebugMode Then
                    Call VBDebugger.EchoLine($"[init] {arg.Key} => {arg.Value.GetJson}")
                End If

                Call App.JoinVariable(arg.Key, arg.Value.JoinBy("; "))
            Next

            ' debug via commandline
            ' set commandline arguments
            For Each arg As NamedValue(Of String) In args
                Dim name As String = CLITools.TrimParamPrefix(arg.Name)

                If Not arguments.ContainsKey(name) Then
                    Call arguments.Add(name, {arg.Value})
                End If
            Next

            ' set request id to function parameters
            If Not arguments.ContainsKey("web_request_id") Then
                Call arguments.Add("web_request_id", {request_id})
            End If

            ' and also set to the runtime environments
            R.globalEnvir.options.setOption(
                opt:="web_request_id",
                value:=request_id,
                env:=R.globalEnvir
            )
        End If

        Dim parameters As List(Of NamedValue(Of Object)) = arguments _
            .Select(Function(a)
                        Return New NamedValue(Of Object) With {
                            .Name = a.Key,
                            .Value = a.Value
                        }
                    End Function) _
            .AsList

        R.debug = isDebugMode

        If isDebugMode Then
            Call VBDebugger.EchoLine("[init] load startup packages")
        End If

        For Each pkgName As String In R.configFile _
            .GetStartupLoadingPackages _
            .JoinIterates(startups)

            Call R.LoadLibrary(packageName:=pkgName)
        Next

        If Not pkg_attach.StringEmpty AndAlso pkg_attach.DirectoryExists Then
            Call VBDebugger.EchoLine($"load required packages from alternative repository: '{pkg_attach.GetDirectoryFullPath}'...")
            Call PackageLoader2.Hotload(pkg_attach, R.globalEnvir)
        End If
        If isDebugMode Then
            Call VBDebugger.EchoLine("[load] source target script...")
        End If

        result = R.Source(script, parameters.ToArray)

        If TypeOf result Is Message Then
            If isDebugMode Then
                Call VBDebugger.EchoLine("[end] script text contains invalid syntax?")
            End If

            Return R.globalEnvir.postResult(
                result:=result,
                master:=upstream,
                request_id:=request_id,
                retryTimes:=retryTimes,
                timeoutMS:=timeout,
                isDebugMode:=isDebugMode
            )
        ElseIf Not entry.StringEmpty Then
            If isDebugMode Then
                Call VBDebugger.EchoLine($"[call] {entry}")
            End If

            result = R.Invoke(entry, parameters.ToArray)
        Else
        End If

        If isDebugMode Then
            Call VBDebugger.EchoLine("[end] finalized")
        End If

        ' post result data back to the master node
        Return R.globalEnvir.postResult(
            result:=result,
            master:=upstream,
            request_id:=request_id,
            retryTimes:=retryTimes,
            timeoutMS:=timeout,
            isDebugMode:=isDebugMode
        )
    End Function

    ''' <summary>
    ''' post back the result data to the fastRweb host
    ''' </summary>
    ''' <param name="env"></param>
    ''' <param name="result"></param>
    ''' <param name="master"></param>
    ''' <param name="request_id"></param>
    ''' <param name="retryTimes"></param>
    ''' <param name="timeoutMS"></param>
    ''' <param name="isDebugMode"></param>
    ''' <returns></returns>
    <Extension>
    Private Function postResult(env As Environment,
                                result As Object,
                                master As IPEndPoint,
                                request_id As String,
                                retryTimes As Integer,
                                timeoutMS As Double,
                                isDebugMode As Boolean) As Integer

        Dim buffer As Buffer = BufferHandler.getBuffer(result, env)

        If isDebugMode AndAlso TypeOf buffer.data Is textBuffer Then
#Disable Warning
            Call VBDebugger.EchoLine(vbNewLine)
            Call VBDebugger.EchoLine(DirectCast(buffer.data, textBuffer).text)
            Call VBDebugger.EchoLine(vbNewLine)
#Enable Warning
        End If

        Dim packageData As Byte() = New IPCBuffer(request_id, buffer).Serialize
        Dim request As New RequestStream(0, 0, packageData)
        Dim timeout As Boolean = False
        Dim data As RequestStream

        For i As Integer = 0 To retryTimes
            Call $"push callback data '{buffer.code.Description}' to [{master}] [{packageData.Length} bytes]".__INFO_ECHO

            data = New Tcp.TcpRequest(master).SetTimeOut(TimeSpan.FromMilliseconds(timeoutMS)).SendMessage(request)

            If data.ProtocolCategory < 0 AndAlso data.Protocol = 500 Then
                Call data.GetUTF8String.Warning
                timeout = True
            Else
                timeout = False
            End If

            If Not timeout Then
                Exit For
            Else
                Call "operation timeout, retry...".__DEBUG_ECHO
            End If
        Next

        If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
            Call App.LogException(DirectCast(result, Message).ToCLRException)
            Call Internal.debug.PrintMessageInternal(result, env.globalEnvironment)

            Return DirectCast(result, Message).level
        Else
            Return 0
        End If
    End Function
End Module

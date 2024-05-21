#Region "Microsoft.VisualBasic::b4fe28d1bdb8f990cfc4becfe5013e86, studio\Rserve\Program.vb"

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

    '   Total Lines: 186
    '    Code Lines: 152 (81.72%)
    ' Comment Lines: 11 (5.91%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 23 (12.37%)
    '     File Size: 8.39 KB


    ' Module Program
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: listen, listenCurrentFolder, Main, makeconfig, runSession
    '               start
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports Flute.Http
Imports Flute.Http.Configurations
Imports Flute.Http.Core
Imports Flute.Http.FileSystem
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Parallel
Imports Rserver

Module Program

    Sub New()

    End Sub

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine, executeEmpty:=AddressOf listenCurrentFolder)
    End Function

    ''' <summary>
    ''' run ``--listen`` command for current folder by default
    ''' </summary>
    ''' <returns></returns>
    Private Function listenCurrentFolder() As Integer
        Return listen("--listen")
    End Function

    <ExportAPI("--listen")>
    <Description("Start a local static web server for hosting statics web page files")>
    <Usage("--listen [/wwwroot <directory_path> --attach <other_directory_path/streampack> --parent <parent_process_id> /port <http_port, default=80>]")>
    Public Function listen(args As CommandLine) As Integer
        Dim wwwroot As String = args("/wwwroot") Or App.CurrentDirectory
        Dim port As Integer = args("/port") Or 80
        Dim attach As String = args("--attach")
        Dim parent As String = args("--parent")
        Dim localfs As New WebFileSystemListener() With {
            .fs = New FileSystem(wwwroot)
        }
        Dim localhost As New HttpSocket(
            app:=AddressOf localfs.WebHandler,
            port:=port
        )

        If Not attach.StringEmpty Then
            If attach.DirectoryExists Then
                Call localfs.fs _
                    .AttachFolder(attach) _
                    .ToArray
            Else
                Call localfs.fs _
                    .AttachFolder(New StreamPack(
                        buffer:=attach.Open(FileMode.Open, doClear:=False, [readOnly]:=True),
                        [readonly]:=True
                    )) _
                    .ToArray
            End If
        End If

        Call BackgroundTaskUtils.BindToMaster(parentId:=parent, kill:=localhost)

        If Not Tcp.PortIsAvailable(port) Then
            Call Console.WriteLine($"local tcp port(={port}) is in used!")
            Return 500
        Else
            Return localhost.Run
        End If
    End Function

    <ExportAPI("/make.config")>
    <Description("export the default configuration file for the http server core.")>
    <Usage("/make.config --export <dir_path_to_save settings.ini>")>
    Public Function makeconfig(args As CommandLine) As Integer
        Dim export_dir As String = args("--export")
        Dim defaultSettings As Configuration = Configuration.Default
        Dim configfile As String = $"{export_dir}/settings.ini"
        Dim access As New AccessController With {
            .ignores = {},
            .redirect = "/",
            .status_key = "user"
        }

        Call Configuration.Save(defaultSettings, configfile)
        Call access.GetJson.SaveTo($"{export_dir}/access.json")

        Return 0
    End Function

    ''' <summary>
    ''' Run rscript
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--start")>
    <Description("Start R# web services, host R# script with http get request.")>
    <Usage("--start [--port <port number, default=7452> 
                     --tcp <port_number, default=3838> 
                     --Rweb <directory, default=./Rweb> 
                     --wwwroot <directory, default=null>
                     --startups <packageNames, default=""""> 
                     --show_error 
                     --n_threads <max_threads, default=8> 
                     --access_rule <access_control.json>
                     --config <config_file, default=null>
                     --parent <parent_pid, default="""">]")>
    <Argument("--Rweb", True, CLITypes.File, AcceptTypes:={GetType(String)},
              Description:="A directory path that contains the r-sharp script file for handling the http request from the client.")>
    <Argument("--port", True, CLITypes.Integer, AcceptTypes:={GetType(Integer)},
              Description:="the local tcp port for listening the in-comming http request.")>
    <Argument("--tcp", True, CLITypes.Integer, AcceptTypes:={GetType(Integer)},
              Description:="the local tcp port for listening the rscript slave processor result for handling the http request.")>
    <Argument("--startups", True, CLITypes.String, AcceptTypes:={GetType(String())},
              Description:="setting up the startup R# package names when running a rscript worker for the http request. 
              this parameter value should be a array of the package name with comma symbol as delimiter.")>
    <Argument("--parent", True, CLITypes.Integer, AcceptTypes:={GetType(Integer)},
              Description:="setting up the parent process its PID for binding current R# web server background service. 
              if this parameter has been configured, then this http web server will be killed after the specific parent process exit from running status.")>
    <Argument("---wwwroot", True, CLITypes.File, AcceptTypes:={GetType(String)},
              Description:="the root directory for the static file, example as html files, js files or image files. 
              leaves this argument blank means http file server function will be disabled, the http web server just handing the rscript request.")>
    <Argument("--config", True, CLITypes.File, AcceptTypes:={GetType(String)},
              Description:="the file path to the *.ini configuration file for the http web server.")>
    Public Function start(args As CommandLine) As Integer
        Dim port As Integer = args("--port") Or 7452
        Dim Rweb As String = args("--Rweb") Or App.CurrentDirectory & "/Rweb"
        Dim n_threads As Integer = args("--n_threads") Or 8
        Dim show_error As Boolean = args("--show_error")
        Dim tcp As Integer = args("--tcp") Or 3838
        Dim startups As String() = args("--startups").Split("[,;]\s*", regexp:=True)
        Dim parent As String = args("--parent")
        Dim wwwroot As String = args("--wwwroot")
        Dim fs As FileSystem = Nothing
        Dim inifile As String = args("--config")
        Dim config As Configuration = Configuration.Load(inifile)
        Dim rule_json As String = args("--access_rule")
        Dim access_rule As AccessController = Nothing

        If Not wwwroot.StringEmpty Then
            fs = New FileSystem(wwwroot)
        End If
        If rule_json.FileExists Then
            access_rule = rule_json.ReadAllText.LoadJSON(Of AccessController)
        End If

        If fs Is Nothing Then
            Call VBDebugger.EchoLine("filesystem is not mount, static file will not be served.")
        End If

        ' 20221007 fix of the relative path error
        ' by translate to absolute path at first!
        Rweb = Rweb.GetDirectoryFullPath

        Using http As New Rweb(Rweb, port, tcp, show_error,
                               threads:=n_threads,
                               configs:=config)

            Call http.Processor.WithStartups(startups)
            Call http.SetFileSystem(fs)
            Call http.SetAccessControl(access_rule)

            Call BackgroundTaskUtils.BindToMaster(parent, http)

            Return http.Run()
        End Using
    End Function

    <ExportAPI("--session")>
    <Description("Run GCModeller workbench R# backend session.")>
    <Usage("--session [--port <port number, default=8848> --workspace <directory, default=./>]")>
    Public Function runSession(args As CommandLine) As Integer
        Dim port As Integer = args("--port") Or 8848
        Dim workspace As String = args("--workspace") Or "./"
        Dim parent As String = args("--parent")

        Using http As New RSession(port, workspace)
            Call BackgroundTaskUtils.BindToMaster(parent, http)
            Return http.Run
        End Using
    End Function

End Module

#Region "Microsoft.VisualBasic::d8aec9cc0e9d442693bfdf5fee801390, E:/GCModeller/src/R-sharp/studio/Rserver//Rserver.vb"

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

    '   Total Lines: 242
    '    Code Lines: 131
    ' Comment Lines: 88
    '   Blank Lines: 23
    '     File Size: 10.50 KB


    ' Class Rserve
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: FromEnvironment
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService
Imports Microsoft.VisualBasic.ApplicationServices

' Microsoft VisualBasic CommandLine Code AutoGenerator
' assembly: ..\net6.0\Rserve.dll

' 
'  // 
'  // 
'  // 
'  // VERSION:   1.0.0.0
'  // ASSEMBLY:  Rserve, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
'  // COPYRIGHT: 
'  // GUID:      
'  // BUILT:     1/1/2000 12:00:00 AM
'  // 
' 
' 
'  < Rserve.Program >
' 
' 
' SYNOPSIS
' Rserve command [/argument argument-value...] [/@set environment-variable=value...]
' 
' All of the command that available in this program has been list below:
' 
'  --listen:         Start a local static web server for hosting statics web page files
'  --session:        Run GCModeller workbench R# backend session
'  --start:          Start R# web services, host R# script with http get request
'  /make.config:     export the default configuration file for the http server core
' 
' 
' ----------------------------------------------------------------------------------------------------
' 
'    1. You can using "Rserve ??<commandName>" for getting more details command help.
'    2. Using command "Rserve /CLI.dev [---echo]" for CLI pipeline development.
'    3. Using command "Rserve /i" for enter interactive console mode.
'    4. Using command "Rserve /STACK:xxMB" for adjust the application stack size, example as '/STACK:64MB'.

Namespace RscriptCommandLine


''' <summary>
''' Rserve.Program
''' </summary>
'''
Public Class Rserve : Inherits InteropService

    Public Const App$ = "Rserve.exe"

    Sub New(App$)
        Call MyBase.New(app:=App$)
    End Sub
        
''' <summary>
''' Create an internal CLI pipeline invoker from a given environment path. 
''' </summary>
''' <param name="directory">A directory path that contains the target application</param>
''' <returns></returns>
     <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function FromEnvironment(directory As String) As Rserve
          Return New Rserve(App:=directory & "/" & Rserve.App)
     End Function

''' <summary>
''' ```bash
''' --listen [/wwwroot &lt;directory_path&gt; --attach &lt;other_directory_path/streampack&gt; --parent &lt;parent_process_id&gt; /port &lt;http_port, default=80&gt;]
''' ```
''' Start a local static web server for hosting statics web page files
''' </summary>
'''

Public Function listen(Optional wwwroot As String = "", Optional attach As String = "", Optional parent As String = "", Optional port As String = "80") As Integer
Dim cli = GetlistenCommandLine(wwwroot:=wwwroot, attach:=attach, parent:=parent, port:=port, internal_pipelineMode:=True)
    Dim proc As IIORedirectAbstract = RunDotNetApp(cli)
    Return proc.Run()
End Function
Public Function GetlistenCommandLine(Optional wwwroot As String = "", Optional attach As String = "", Optional parent As String = "", Optional port As String = "80", Optional internal_pipelineMode As Boolean = True) As String
    Dim CLI As New StringBuilder("--listen")
    Call CLI.Append(" ")
    If Not wwwroot.StringEmpty Then
            Call CLI.Append("/wwwroot " & """" & wwwroot & """ ")
    End If
    If Not attach.StringEmpty Then
            Call CLI.Append("--attach " & """" & attach & """ ")
    End If
    If Not parent.StringEmpty Then
            Call CLI.Append("--parent " & """" & parent & """ ")
    End If
    If Not port.StringEmpty Then
            Call CLI.Append("/port " & """" & port & """ ")
    End If
     Call CLI.Append($"/@set --internal_pipeline={internal_pipelineMode.ToString.ToUpper()} ")


Return CLI.ToString()
End Function

''' <summary>
''' ```bash
''' --session [--port &lt;port number, default=8848&gt; --workspace &lt;directory, default=./&gt;]
''' ```
''' Run GCModeller workbench R# backend session.
''' </summary>
'''

Public Function runSession(Optional port As String = "8848", Optional workspace As String = "./") As Integer
Dim cli = GetrunSessionCommandLine(port:=port, workspace:=workspace, internal_pipelineMode:=True)
    Dim proc As IIORedirectAbstract = RunDotNetApp(cli)
    Return proc.Run()
End Function
Public Function GetrunSessionCommandLine(Optional port As String = "8848", Optional workspace As String = "./", Optional internal_pipelineMode As Boolean = True) As String
    Dim CLI As New StringBuilder("--session")
    Call CLI.Append(" ")
    If Not port.StringEmpty Then
            Call CLI.Append("--port " & """" & port & """ ")
    End If
    If Not workspace.StringEmpty Then
            Call CLI.Append("--workspace " & """" & workspace & """ ")
    End If
     Call CLI.Append($"/@set --internal_pipeline={internal_pipelineMode.ToString.ToUpper()} ")


Return CLI.ToString()
End Function

''' <summary>
''' ```bash
''' --start [--port &lt;port number, default=7452&gt; --tcp &lt;port_number, default=3838&gt; --Rweb &lt;directory, default=./Rweb&gt; --wwwroot &lt;directory, default=null&gt; --startups &lt;packageNames, default=&quot;&quot;&gt; --show_error --n_threads &lt;max_threads, default=8&gt; --config &lt;config_file, default=null&gt; --parent &lt;parent_pid, default=&quot;&quot;&gt;]
''' ```
''' Start R# web services, host R# script with http get request.
''' </summary>
'''
''' <param name="Rweb"> A directory path that contains the r-sharp script file for handling the http request from the client.
''' </param>
''' <param name="port"> the local tcp port for listening the in-comming http request.
''' </param>
''' <param name="tcp"> the local tcp port for listening the rscript slave processor result for handling the http request.
''' </param>
''' <param name="startups"> setting up the startup R# package names when running a rscript worker for the http request. 
'''               this parameter value should be a array of the package name with comma symbol as delimiter.
''' </param>
''' <param name="parent"> setting up the parent process its PID for binding current R# web server background service. 
'''               if this parameter has been configured, then this http web server will be killed after the specific parent process exit from running status.
''' </param>
''' <param name="wwwroot"> the root directory for the static file, example as html files, js files or image files. 
'''               leaves this argument blank means http file server function will be disabled, the http web server just handing the rscript request.
''' </param>
''' <param name="config"> the file path to the *.ini configuration file for the http web server.
''' </param>
Public Function start(Optional port As String = "7452", 
                         Optional tcp As String = "3838", 
                         Optional rweb As String = "./Rweb", 
                         Optional wwwroot As String = "null", 
                         Optional startups As String = "", 
                         Optional n_threads As String = "8", 
                         Optional config As String = "null", 
                         Optional parent As String = "", 
                         Optional show_error As Boolean = False) As Integer
Dim cli = GetstartCommandLine(port:=port, 
                         tcp:=tcp, 
                         rweb:=rweb, 
                         wwwroot:=wwwroot, 
                         startups:=startups, 
                         n_threads:=n_threads, 
                         config:=config, 
                         parent:=parent, 
                         show_error:=show_error, internal_pipelineMode:=True)
    Dim proc As IIORedirectAbstract = RunDotNetApp(cli)
    Return proc.Run()
End Function
Public Function GetstartCommandLine(Optional port As String = "7452", 
                         Optional tcp As String = "3838", 
                         Optional rweb As String = "./Rweb", 
                         Optional wwwroot As String = "null", 
                         Optional startups As String = "", 
                         Optional n_threads As String = "8", 
                         Optional config As String = "null", 
                         Optional parent As String = "", 
                         Optional show_error As Boolean = False, Optional internal_pipelineMode As Boolean = True) As String
    Dim CLI As New StringBuilder("--start")
    Call CLI.Append(" ")
    If Not port.StringEmpty Then
            Call CLI.Append("--port " & """" & port & """ ")
    End If
    If Not tcp.StringEmpty Then
            Call CLI.Append("--tcp " & """" & tcp & """ ")
    End If
    If Not rweb.StringEmpty Then
            Call CLI.Append("--rweb " & """" & rweb & """ ")
    End If
    If Not wwwroot.StringEmpty Then
            Call CLI.Append("--wwwroot " & """" & wwwroot & """ ")
    End If
    If Not startups.StringEmpty Then
            Call CLI.Append("--startups " & """" & startups & """ ")
    End If
    If Not n_threads.StringEmpty Then
            Call CLI.Append("--n_threads " & """" & n_threads & """ ")
    End If
    If Not config.StringEmpty Then
            Call CLI.Append("--config " & """" & config & """ ")
    End If
    If Not parent.StringEmpty Then
            Call CLI.Append("--parent " & """" & parent & """ ")
    End If
    If show_error Then
        Call CLI.Append("--show_error ")
    End If
     Call CLI.Append($"/@set --internal_pipeline={internal_pipelineMode.ToString.ToUpper()} ")


Return CLI.ToString()
End Function

''' <summary>
''' ```bash
''' /make.config --export &lt;file_path_to_save.ini&gt;
''' ```
''' export the default configuration file for the http server core.
''' </summary>
'''

Public Function makeconfig(export As String) As Integer
Dim cli = GetmakeconfigCommandLine(export:=export, internal_pipelineMode:=True)
    Dim proc As IIORedirectAbstract = RunDotNetApp(cli)
    Return proc.Run()
End Function
Public Function GetmakeconfigCommandLine(export As String, Optional internal_pipelineMode As Boolean = True) As String
    Dim CLI As New StringBuilder("/make.config")
    Call CLI.Append(" ")
    Call CLI.Append("--export " & """" & export & """ ")
     Call CLI.Append($"/@set --internal_pipeline={internal_pipelineMode.ToString.ToUpper()} ")


Return CLI.ToString()
End Function
End Class
End Namespace

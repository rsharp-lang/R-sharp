#Region "Microsoft.VisualBasic::94646487253fec71ba078416f1010afd, G:/GCModeller/src/R-sharp/studio/Rserve//Program.vb"

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

    '   Total Lines: 92
    '    Code Lines: 72
    ' Comment Lines: 7
    '   Blank Lines: 13
    '     File Size: 3.67 KB


    ' Module Program
    ' 
    '     Function: listen, Main, runSession, start
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Flute.Http.Core
Imports Flute.Http.FileSystem
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Net
Imports Parallel
Imports Rserver

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("--listen")>
    <Description("Start a local static web server for hosting statics web page files")>
    <Usage("--listen /wwwroot <directory_path> [--attach <other_directory_path> --parent <parent_process_id> /port <http_port, default=80>]")>
    Public Function listen(args As CommandLine) As Integer
        Dim wwwroot As String = args <= "/wwwroot"
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
            Call localfs.fs _
                .AttachFolder(attach) _
                .ToArray
        End If

        Call BackgroundTaskUtils.BindToMaster(parentId:=parent, kill:=localhost)

        If Not Tcp.PortIsAvailable(port) Then
            Call Console.WriteLine($"local tcp port(={port}) is in used!")
            Return 500
        Else
            Return localhost.Run
        End If
    End Function

    ''' <summary>
    ''' Run rscript
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--start")>
    <Description("Start R# web services, host R# script with http get request.")>
    <Usage("--start [--port <port number, default=7452> --tcp <port_number, default=3838> --Rweb <directory, default=./Rweb> --startups <packageNames, default=""""> --show_error --n_threads <max_threads, default=8>]")>
    Public Function start(args As CommandLine) As Integer
        Dim port As Integer = args("--port") Or 7452
        Dim Rweb As String = args("--Rweb") Or App.CurrentDirectory & "/Rweb"
        Dim n_threads As Integer = args("--n_threads") Or 8
        Dim show_error As Boolean = args("--show_error")
        Dim tcp As Integer = args("--tcp") Or 3838
        Dim startups As String() = args("--startups").Split("[,;]\s*", regexp:=True)
        Dim parent As String = args("--parent")

        ' 20221007 fix of the relative path error
        ' by translate to absolute path at first!
        Rweb = Rweb.GetDirectoryFullPath

        Using http As New Rweb(Rweb, port, tcp, show_error, threads:=n_threads)
            Call http.Processor.WithStartups(startups)
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

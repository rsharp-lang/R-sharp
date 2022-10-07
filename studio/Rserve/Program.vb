#Region "Microsoft.VisualBasic::6b82bce2af8969f1655cd5c191d6c19e, R-sharp\studio\Rserve\Program.vb"

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

    '   Total Lines: 47
    '    Code Lines: 35
    ' Comment Lines: 5
    '   Blank Lines: 7
    '     File Size: 1.95 KB


    ' Module Program
    ' 
    '     Function: Main, runSession, start
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.Default
Imports Rserver

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
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

        ' 20221007 fix of the relative path error
        ' by translate to absolute path at first!
        Rweb = Rweb.GetDirectoryFullPath

        Using http As New Rweb(Rweb, port, tcp, show_error, threads:=n_threads)
            Call http.Processor.WithStartups(startups)
            Return http.Run()
        End Using
    End Function

    <ExportAPI("--session")>
    <Description("Run GCModeller workbench R# backend session.")>
    <Usage("--session [--port <port number, default=8848> --workspace <directory, default=./>]")>
    Public Function runSession(args As CommandLine) As Integer
        Dim port As Integer = args("--port") Or 8848
        Dim workspace As String = args("--workspace") Or "./"

        Using http As New RSession(port, workspace)
            Return http.Run
        End Using
    End Function

End Module

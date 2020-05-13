Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

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
    <Description("Run R# script with http get request.")>
    <Usage("--start [--port <port number, default=7452> --Rweb <directory, default=./Rweb> --show_error --n_threads <max_threads, default=8>]")>
    Public Function start(args As CommandLine) As Integer
        Dim port As Integer = args("--port") Or 7452
        Dim Rweb As String = args("--Rweb") Or App.CurrentDirectory & "/Rweb"
        Dim n_threads As Integer = args("--n_threads") Or 8
        Dim show_error As Boolean = args("--show_error")

        Using http As New Rweb(Rweb, port, show_error, threads:=n_threads)
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

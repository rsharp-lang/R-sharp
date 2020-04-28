Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.WebCloud.HTTPInternal.Platform

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("--start")>
    <Usage("--start --port 7452 --Rweb <directory> [--n_threads <max_threads, default=8>]")>
    Public Function start(args As CommandLine) As Integer
        Dim port As Integer = args <= "--port"
        Dim Rweb As String = args <= "--Rweb"
        Dim n_threads As Integer = args("--n_threads") Or 8
        Dim http As New PlatformEngine(
            HOME, port,
            nullExists:=True,
            threads:=n_threads,
            cache:=False
        )

        Return http.Run()
    End Function

End Module

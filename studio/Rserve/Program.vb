Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

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

    End Function

End Module

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Interpreter

Partial Module CLI

    ' Rscript --lambda biodeepMSMS::doMSMSalignment --SetDllDirectory /usr/local/bin

    <ExportAPI("--lambda")>
    <Description("Execute R# function with parameters")>
    <Usage("--lambda <delegate_name> [--SetDllDirectory <dll_directory>]")>
    Public Function execLambda(args As CommandLine) As Integer
        Dim SetDllDirectory As String = args("--SetDllDirectory")
        Dim REngine As New RInterpreter

        If Not SetDllDirectory.StringEmpty Then
            Call REngine.globalEnvir.options.setOption("SetDllDirectory", SetDllDirectory)
        End If


    End Function
End Module

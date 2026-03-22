Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter

Partial Module CLI

    <ExportAPI("--head")>
    <Usage("--head <data.csv> [-n <n rows, default=15>]")>
    Public Function head(args As CommandLine) As Integer
        Dim file As String = args("--file") Or args.Tokens.ElementAtOrDefault(1)
        Dim n As Integer = args("-n") Or 15
        Dim rows As New List(Of String)

        If Not file.FileExists Then
            Call "invalid file argument, missing file or no file path value provided!".error
            Return 404
        End If

        Using s As Stream = File.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
            Dim text As New StreamReader(s)
            Dim line As Value(Of String) = text.ReadLine

            Call rows.Add(CStr(line))

            Do While (line = text.ReadLine) IsNot Nothing
                Call rows.Add(CStr(line))

                If rows.Count > n Then
                    Exit Do
                End If
            Loop
        End Using

        Dim tsv As Boolean = Not File.ExtensionSuffix("csv")
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(
            configs:=ConfigFile.localConfigs
        )

        Call R.Set("x", rows.JoinBy(vbCrLf))
        Call R.LoadLibrary(
            packageName:="utils",
            silent:=True,
            ignoreMissingStartupPackages:=True
        )

        Dim expr As String
        Dim result As Object

        If tsv Then
            expr = $"read.table(header = TRUE, row.names = 1, check.names = FALSE, text = x);"
        Else
            expr = $"read.csv(row.names = 1, check.names = FALSE, text = x)"
        End If

        expr = $"print({expr});"
        result = R.Evaluate(expr)

        Return Rscript.handleResult(result, R.globalEnvir, Nothing)
    End Function

End Module
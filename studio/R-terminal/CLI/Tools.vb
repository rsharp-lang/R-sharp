Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter

Partial Module CLI

    <ExportAPI("--head")>
    <Usage("--head <data.csv> [-n <n rows, default=15>]")>
    Public Function head(file As String, args As CommandLine) As Integer
        Dim n As Integer = args("-n") Or 15
        Dim rows As New List(Of String)

        Using s As Stream = file.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
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

        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(
            configs:=ConfigFile.localConfigs
        )
    End Function

End Module
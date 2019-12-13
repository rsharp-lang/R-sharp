Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Configuration

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine, executeFile:=AddressOf Run)
    End Function

    Private Function Run(filepath$, args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)

        If args("--debug") Then
            R.debug = True
        End If

        Call Console.WriteLine(args.ToString)
        Call Console.WriteLine()

        Call R.LoadLibrary("base")
        Call R.LoadLibrary("utils")
        Call R.LoadLibrary("grDevices")

        Call Console.WriteLine()

        'For Each arg As NamedValue(Of String) In args.ToArgumentVector
        '    Call R.Add(CommandLine.TrimNamePrefix(arg.Name), arg.Value, TypeCodes.generic)
        'Next

        Dim result As Object = R.Source(filepath)

        If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
            Return DirectCast(result, Message).level
        Else
            Return 0
        End If
    End Function

    <ExportAPI("/compile")>
    Public Function Compile(args As CommandLine) As Integer

    End Function
End Module

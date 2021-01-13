Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter

Partial Module CLI

    <ExportAPI("--parallel")>
    <Usage("--parallel --master <master_port> [--delegate <delegate_name>]")>
    Public Function parallelMode(args As CommandLine) As Integer
        Dim masterPort As Integer = args <= "--master"
        Dim REngine As New RInterpreter
        Dim plugin As String = LibDLL.GetDllFile($"snowFall.dll", REngine.globalEnvir)

        If plugin.FileExists Then
            Call PackageLoader.ParsePackages(plugin) _
                .Where(Function(pkg) pkg.namespace = "Parallel") _
                .FirstOrDefault _
                .DoCall(Sub(pkg)
                            Call REngine.globalEnvir.ImportsStatic(pkg.package)
                        End Sub)
            Call REngine.Invoke("Parallel::snowFall", {masterPort})
        Else
            Return 500
        End If

        Return 0
    End Function
End Module
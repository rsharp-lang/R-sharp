Imports System.ComponentModel
Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Configuration
Imports SMRUCC.Rsharp.System.Package

Partial Module CLI

    <ExportAPI("--install.packages")>
    <Description("Install new packages.")>
    <Usage("--install.packages /module <*.dll> [--verbose]")>
    <ArgumentAttribute("/module", False, CLITypes.File,
              Extensions:="*.dll",
              Description:=".NET Framework 4.8 assembly module file.")>
    <Group(SystemConfig)>
    Public Function Install(args As CommandLine) As Integer
        Dim module$ = args <= "/module"
        Dim config As New Options(ConfigFile.localConfigs)

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {ConfigFile.localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        If [module].StringEmpty Then
            Return "Missing '/module' argument!".PrintException
        End If

        Using pkgMgr As New PackageManager(config)
            If Not [module].ToLower.StartsWith("scan=") Then
                Call pkgMgr.InstallLocals(dllFile:=[module])
            Else
                For Each file As String In ls - l - "*.dll" <= [module].GetTagValue("=", trim:=True).Value
                    Try
                        Dim assm As Assembly = Assembly.LoadFrom(file.GetFullPath)

                        If Not assm.GetCustomAttribute(Of RPackageModuleAttribute) Is Nothing Then
                            Call pkgMgr.InstallLocals(dllFile:=file)
                        End If
                    Catch ex As Exception

                    End Try
                Next
            End If
        End Using

        Return 0
    End Function

    <ExportAPI("--startups")>
    <Usage("--startups [--add <namespaceList> --remove <namespaceList>]")>
    <Group(SystemConfig)>
    Public Function ConfigStartups(args As CommandLine) As Integer
        Dim adds As String = args("--add")
        Dim remove As String = args("--remove")
        Dim config As ConfigFile = ConfigFile.Load(ConfigFile.localConfigs)

        If config.startups Is Nothing Then
            config.startups = New StartupConfigs
        End If

        If Not adds.StringEmpty Then
            config.startups.loadingPackages = config.startups _
                .loadingPackages _
                .JoinIterates(adds.StringSplit("([;,]|\s)+")) _
                .ToArray
        End If
        If Not remove.StringEmpty Then
            Dim removePending As Index(Of String) = remove.StringSplit("([;,]|\s)+")

            config.startups.loadingPackages = config.startups _
                .loadingPackages _
                .SafeQuery _
                .Where(Function(name) Not name Like removePending) _
                .ToArray
        End If

        If adds.StringEmpty AndAlso remove.StringEmpty Then
            For Each name As String In config.startups.loadingPackages
                Call Console.WriteLine(name)
            Next
        End If

        Return config _
            .GetXml _
            .SaveTo(ConfigFile.localConfigs) _
            .CLICode
    End Function

    <ExportAPI("--setup")>
    <Description("Initialize the R# runtime environment.")>
    <Group(SystemConfig)>
    Public Function InitializeEnvironment(args As CommandLine) As Integer
        Dim config As New Options(ConfigFile.localConfigs)

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {ConfigFile.localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        App.CurrentDirectory = App.HOME

        Using pkgMgr As New PackageManager(config)
            For Each file As String In {"R.base.dll", "R.graph.dll", "R.graphics.dll", "R.math.dll", "R.plot.dll"}
                If Not file.FileExists Then
                    file = "Library/" & file
                End If

                If file.FileExists Then
                    Call pkgMgr.InstallLocals(dllFile:=file)
                    Call pkgMgr.Flush()
                Else
                    Call $"missing module dll: {file}".PrintException
                End If
            Next
        End Using

        Return 0
    End Function

End Module
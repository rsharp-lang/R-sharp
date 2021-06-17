#Region "Microsoft.VisualBasic::53db221c1e8cdb938cea6f75b2233e8e, studio\R-terminal\CLI\Utils.vb"

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

    ' Module CLI
    ' 
    '     Function: configREnv, ConfigStartups, InitializeEnvironment, Install, reset
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Partial Module CLI

    <ExportAPI("--install.packages")>
    <Description("Install new packages.")>
    <Usage("--install.packages /module <*.dll/*.zip> [--verbose]")>
    <Argument("/module", False, CLITypes.File,
              Extensions:="*.dll,*.zip",
              Description:=".NET Framework 4.8 assembly module file or compiled R# zip package file.")>
    <Group(SystemConfig)>
    Public Function Install(args As CommandLine) As Integer
        Dim module$ = args <= "/module"
        Dim config As New Options(ConfigFile.localConfigs, saveConfig:=False)

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {ConfigFile.localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        If [module].StringEmpty Then
            Return "Missing '/module' argument!".PrintException
        End If

        Using pkgMgr As New PackageManager(config)
            If Not [module].ToLower.StartsWith("scan=") Then
                Call pkgMgr.InstallLocals(pkgFile:=[module])
            Else
                For Each file As String In ls - l - "*.dll" <= [module].GetTagValue("=", trim:=True).Value
                    Try
                        Dim assm As Assembly = Assembly.LoadFrom(file.GetFullPath)

                        If Not assm.GetCustomAttribute(Of RPackageModuleAttribute) Is Nothing Then
                            Call pkgMgr.InstallLocals(pkgFile:=file)
                        End If
                    Catch ex As Exception

                    End Try
                Next
            End If
        End Using

        Return 0
    End Function

    <ExportAPI("--startups")>
    <Description("Config of the startup packages.")>
    <Usage("--startups [--add <namespaceList> --remove <namespaceList>]")>
    <Group(SystemConfig)>
    <Argument("--add", True, CLITypes.String,
              AcceptTypes:={GetType(String())},
              Description:="A list of package namespace that will be added to startup list when start running R# scripting host.")>
    <Argument("--remove", True, CLITypes.String,
              AcceptTypes:={GetType(String())},
              Description:="A list of package namespace that will be removed from startup list.")>
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
        Dim config As New Options(ConfigFile.localConfigs, saveConfig:=False)

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {ConfigFile.localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        App.CurrentDirectory = App.HOME

        Using pkgMgr As New PackageManager(config)
            For Each file As String In {"base.dll", "igraph.dll", "graphics.dll", "Rlapack.dll"}
                If Not file.FileExists Then
                    file = "Library/" & file
                End If

                If file.FileExists Then
                    Call pkgMgr.InstallLocals(pkgFile:=file)
                    Call pkgMgr.Flush()
                Else
                    Call $"missing module dll: {file}".PrintException
                End If
            Next
        End Using

        Return 0
    End Function

    <ExportAPI("--reset")>
    <Description("Reset the R# envronment, configuration and package list to default empty.")>
    <Group(SystemConfig)>
    Public Function reset(args As CommandLine) As Integer
        Using config As New Options(ConfigFile.EmptyConfigs, saveConfig:=True), pkgMgr As PackageManager = PackageManager.getEmpty(config)
            Return 0
        End Using
    End Function

    <ExportAPI("--config")>
    <Description("Run config of the R# environment its default options.")>
    <Group(SystemConfig)>
    <Usage("--config name1=value1 [name2=value2 ...]")>
    Public Function configREnv(args As CommandLine) As Integer
        Using config As New Options(ConfigFile.EmptyConfigs, saveConfig:=True)
            For Each optVal As NamedValue(Of String) In args.AsEnumerable
                Call config.setOption(optVal.Name, optVal.Value)
            Next
        End Using

        Return 0
    End Function

End Module

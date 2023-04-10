#Region "Microsoft.VisualBasic::2915cff546e3b2e954ebd616aeb94c83, D:/GCModeller/src/R-sharp/studio/R-terminal//CLI/Utils.vb"

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


    ' Code Statistics:

    '   Total Lines: 269
    '    Code Lines: 222
    ' Comment Lines: 5
    '   Blank Lines: 42
    '     File Size: 10.55 KB


    ' Module CLI
    ' 
    '     Function: configJSON, configREnv, ConfigStartups, getConfig, InitializeEnvironment
    '               Install, reset, View
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
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Partial Module CLI

    <ExportAPI("--install.packages")>
    <Description("Install new packages.")>
    <Usage("--install.packages /module <*.dll/*.zip> [--verbose]")>
    <Argument("/module", False, CLITypes.File,
              Extensions:="*.dll,*.zip",
              Description:=".NET6.0 assembly module file or compiled R# zip package file.")>
    <Group(SystemConfig)>
    Public Function Install(args As CommandLine) As Integer
        Dim module$ = args <= "/module"
        Dim localConfigs As String = getConfig(args)
        Dim config As New Options(localConfigs, saveConfig:=False)
        Dim err As Exception = Nothing

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        If [module].StringEmpty Then
            [module] = args.Tokens(1)

            If Not [module].FileExists Then
                Call $"missing module file '{[module].GetFullPath}'!".Warning
                Return -1
            End If
        End If

        Using pkgMgr As New PackageManager(config)
            If Not [module].ToLower.StartsWith("scan=") Then
                Call pkgMgr.InstallLocals(pkgFile:=[module], err:=err)

                If Not err Is Nothing Then
                    Call App.LogException(err)
                    Return 500
                End If
            Else
                For Each file As String In ls - l - "*.dll" <= [module].GetTagValue("=", trim:=True).Value
                    Try
                        Dim assm As Assembly = Assembly.LoadFrom(file.GetFullPath)

                        If Not assm.GetCustomAttribute(Of RPackageModuleAttribute) Is Nothing Then
                            Call pkgMgr.InstallLocals(pkgFile:=file, err:=err)

                            If Not err Is Nothing Then
                                Call App.LogException(err)
                                Return 500
                            End If
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
        Dim localConfigs As String = getConfig(args)
        Dim config As ConfigFile = ConfigFile.Load(localConfigs)

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
            .SaveTo(localConfigs) _
            .CLICode
    End Function

    Private Function getConfig(args As CommandLine) As String
        Dim engineConfig As String = (args("--R_LIBS_USER") Or System.Environment.GetEnvironmentVariable("R_LIBS_USER"))
        Return If(engineConfig.StringEmpty, ConfigFile.localConfigs, engineConfig)
    End Function

    <ExportAPI("--setup")>
    <Description("Initialize the R# runtime environment.")>
    <Group(SystemConfig)>
    <Usage("--setup [--verbose]")>
    Public Function InitializeEnvironment(args As CommandLine) As Integer
        Dim localConfigs As String = getConfig(args)
        Dim config As New Options(localConfigs, saveConfig:=False)
        Dim file As String = Nothing

        Internal.debug.verbose = args("--verbose")
        Internal.debug.write($"load config file: {localConfigs}")
        Internal.debug.write($"load package registry: {config.lib}")

        App.CurrentDirectory = App.HOME

        Using pkgMgr As New PackageManager(config)
            For Each fileName As String In {"base.dll", "igraph.dll", "graphics.dll", "Rlapack.dll"}
                For Each dir As String In {"./", "./Library", "./library", "../Library", "../library"}
                    file = $"{dir}/{fileName}"

                    If file.FileExists Then
                        Exit For
                    End If
                Next

                Dim err As Exception = Nothing

                If file.FileExists Then
                    Call pkgMgr.InstallLocals(pkgFile:=file, err:=err)
                    Call pkgMgr.Flush()

                    If Not err Is Nothing Then
                        Call App.LogException(err)
                        Return 500
                    End If
                Else
                    Call $"missing module dll: {fileName}".PrintException
                End If
            Next
        End Using

        Return 0
    End Function

    <ExportAPI("--reset")>
    <Description("Reset the R# envronment, configuration and package list to default empty.")>
    <Group(SystemConfig)>
    Public Function reset(args As CommandLine) As Integer
        Dim localConfigs As String = getConfig(args)

        Using config As New Options(ConfigFile.EmptyConfigs, saveConfig:=True) With {
            .localConfig = localConfigs
        },
            pkgMgr As PackageManager = PackageManager.getEmpty(config)

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

    ''' <summary>
    ''' generate config.json for given Rscript
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("/config.json")>
    <Description("generate config.json for given Rscript.")>
    <Usage("/config.json /script <Rscript.R> [/save <config.json, default=stdout>]")>
    Public Function configJSON(args As CommandLine) As Integer
        Dim scriptfile As String = args <= "/script"
        Dim save As String = args("/save") Or "stdout"
        Dim R As New RInterpreter

        Call R.Imports({"package_utils", "automation"}, "devkit.dll")
        Call R.LoadLibrary("JSON", silent:=True)
        Call R.Set("_", R.Evaluate($"package_utils::parse('{scriptfile}');"))
        Call R.Set("_", R.Evaluate($"automation::config.json(_);"))

        Dim jsonTemplate As String = R.Evaluate($"JSON::json_encode(_, indent = TRUE);")

        If save.TextEquals("stdout") Then
            Call Console.WriteLine(jsonTemplate)
        Else
            Return jsonTemplate _
                .SaveTo(save) _
                .CLICode
        End If

        Return 0
    End Function

    <ExportAPI("/view")>
    <Description("View any text data file.")>
    <Usage("/view <filepath/to/data>")>
    Public Function View(args As CommandLine) As Integer
        Dim file As String = args.Tokens.ElementAtOrNull(1)
        Dim R As New RInterpreter
        Dim result As Object
        Dim inspectObject As Boolean = False

        If file.StringEmpty Then
            result = Internal.debug.stop("missing file path!", R.globalEnvir)
        ElseIf Not file.FileExists Then
            result = Internal.debug.stop({$"target file '{file}' is not exists on your filesystem!", $"fullpath: {file.GetFullPath}"}, R.globalEnvir)
        End If

        Select Case file.ExtensionSuffix
            Case "csv"
                R.Imports({"utils"}, "base.dll")
                result = R.Evaluate($"read.csv('{file}', row.names = 1);")
            Case "tsv"
                R.Imports({"utils"}, "base.dll")
                result = R.Evaluate($"read.csv('{file}', row.names = 1, tsv = TRUE);")
            Case "json"
                R.Imports({"JSON"}, "base.dll")
                result = R.Evaluate($"json_decode(readText('{file}'));")
                inspectObject = True
            Case Else
                result = Internal.debug.stop(New NotImplementedException(file.ExtensionSuffix), R.globalEnvir)
        End Select

        Return Rscript.handleResult(
            result:=result,
            globalEnv:=R.globalEnvir,
            autoPrint:=True,
            inspect:=inspectObject
        )
    End Function

End Module

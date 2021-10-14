#Region "Microsoft.VisualBasic::d3ed5e4ef7060c730877de855a421723, studio\R-terminal\CLI\CLI.vb"

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
'     Function: BashRun, Info, SyntaxText, unixman, Version
' 
'     Sub: unixMan
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RlangScript = SMRUCC.Rsharp.Runtime.Components.Rscript
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

<GroupingDefine(CLI.SystemConfig, Description:="R# language system and environment configuration util tools.")>
<CLI()>
Module CLI

    Friend Const SystemConfig As String = "R# System Utils"

    <ExportAPI("--version")>
    <Description("Print R# interpreter version")>
    Public Function Version(args As CommandLine) As Integer
        Console.Write(GetType(RInterpreter).Assembly.FromAssembly.AssemblyVersion)
        Return 0
    End Function

    <ExportAPI("/bash")>
    <Usage("/bash --script <run.R>")>
    Public Function BashRun(args As CommandLine) As Integer
        Dim script$ = args <= "--script"
        Dim bash$ = script.ParentPath & "/" & script.BaseName
        Dim utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage
        Dim dirHelper As String = UNIX.GetLocationHelper

        script = dirHelper & vbLf & "
app=""$DIR/{script}""
cli=""$@""

R# ""$app"" $cli".Replace("{script}", script.FileName)
        script = script.LineTokens.JoinBy(vbLf)

        Return script.SaveTo(bash, utf8).CLICode
    End Function

    <ExportAPI("--man")>
    <Description("Export help document for Rscript commandline")>
    <Usage("--man /Rscript <script.R> [/save <help.txt>]")>
    Public Function man(args As CommandLine) As Integer
        Dim file As String = args <= "/Rscript"
        Dim savefile As String = args("/save") Or $"{file.TrimSuffix}.help.txt"

        If file.StringEmpty Then
            Call Console.WriteLine("missing Rscript file path!")
            Return -1
        ElseIf Not file.FileExists Then
            Call Console.WriteLine($"the required Rscript file '{file}' is not exists on filesystem!")
            Return -2
        End If

        Using output As StreamWriter = savefile.OpenWriter
            Return Program.QueryCommandLineArgvs(file, output)
        End Using
    End Function

    <ExportAPI("--man.1")>
    <Description("Exports unix man page data for current installed packages.")>
    <Usage("--man.1 [--module <module.dll> --debug --out <directory, default=./>]")>
    Public Function unixman(args As CommandLine) As Integer
        Dim out$ = args("--out") Or "./"
        Dim module$ = args("--module")
        Dim env As New RInterpreter
        Dim xmldocs As AnnotationDocs = env.globalEnvir.packages.packageDocs
        Dim utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage

        If [module].FileExists Then
            For Each pkg As Package In PackageLoader.ParsePackages(dll:=[module])
                Call pkg.unixMan(xmldocs, out)
                Call $"load: {pkg.info.Namespace}".__INFO_ECHO
            Next
        Else
            ' run build for all installed package modules
            For Each pkg As Package In env.globalEnvir.packages.AsEnumerable
                If pkg.isMissing Then
                    Call $"missing package: {pkg.namespace}...".PrintException
                Else
                    Call pkg.unixMan(xmldocs, out)
                End If
            Next
        End If

        Return 0
    End Function

    <Extension>
    Private Sub unixMan(pkg As Package, xmldocs As AnnotationDocs, out$)
        Dim annoDocs As ProjectType = xmldocs.GetAnnotations(pkg.package)
        Dim links As New List(Of NamedValue(Of String))

        For Each ref As String In pkg.ls
            Dim symbol As RMethodInfo = pkg.GetFunction(apiName:=ref)
            Dim docs As ProjectMember = xmldocs.GetAnnotations(symbol.GetRawDeclares)
            Dim help As UnixManPage = UnixManPagePrinter.CreateManPage(symbol, docs)

            links += New NamedValue(Of String) With {
                .Name = ref,
                .Value = $"{pkg.namespace}/{ref}.1",
                .Description = docs _
                    ?.Summary _
                    ?.LineTokens _
                     .FirstOrDefault
            }

            Call UnixManPage _
                .ToString(help, "man page create by R# package system.") _
                .SaveTo($"{out}/{pkg.namespace}/{ref}.1", UTF8)
        Next

        If annoDocs Is Nothing Then
            annoDocs = New ProjectType(New ProjectNamespace(New Project("n/a")))
        End If

        Using markdown As StreamWriter = $"{out}/{pkg.namespace}.md".OpenWriter
            Call markdown.WriteLine("# " & pkg.namespace)
            Call markdown.WriteLine()
            Call markdown.WriteLine(annoDocs.Summary)

            If Not annoDocs.Remarks.StringEmpty Then
                For Each line As String In annoDocs.Remarks.LineTokens
                    Call markdown.WriteLine("> " & line)
                Next
            End If

            Call markdown.WriteLine()

            For Each link As NamedValue(Of String) In links
                Call markdown.WriteLine($"+ [{link.Name}]({link.Value}) {link.Description}")
            Next
        End Using
    End Sub

    <ExportAPI("--info")>
    <Description("Print R# interpreter version information and R# terminal version information.")>
    Public Function Info(args As CommandLine) As Integer
        Dim Rterminal As AssemblyInfo = GetType(Program).Assembly.FromAssembly
        Dim RsharpCore As AssemblyInfo = GetType(RInterpreter).Assembly.FromAssembly

        Call Rterminal.AppSummary(Nothing, Nothing, App.StdOut)
        Call RsharpCore.AppSummary(Nothing, Nothing, App.StdOut)

        Call App.StdOut.value.Flush()
        Call New RInterpreter().Print("sessionInfo();")

        Return 0
    End Function

    <ExportAPI("--syntax")>
    <Description("Show syntax parser result of the input script.")>
    <Usage("--syntax /script <script.R>")>
    Public Function SyntaxText(args As CommandLine) As Integer
        Dim script$ = args <= "/script"
        Dim Rscript As RlangScript = RlangScript.FromFile(script)
        Dim error$ = Nothing
        Dim debugMode As Boolean = args.GetBoolean("--debug")
        Dim program As RProgram = RProgram.CreateProgram(
            Rscript:=Rscript,
            [error]:=[error],
            debug:=debugMode
        )

        If Not [error].StringEmpty Then
            Call Log4VB.Println([error], ConsoleColor.Red)
            Call VBDebugger.WaitOutput()
        Else
            Call Console.WriteLine(program.ToString)
        End If

        Return 0
    End Function
End Module

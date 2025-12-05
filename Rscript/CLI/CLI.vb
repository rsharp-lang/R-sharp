#Region "Microsoft.VisualBasic::5a914b545ada5b1d29e73a523bbe6c74, Rscript\CLI\CLI.vb"

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

    '   Total Lines: 88
    '    Code Lines: 74 (84.09%)
    ' Comment Lines: 2 (2.27%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (13.64%)
    '     File Size: 3.98 KB


    ' Module CLI
    ' 
    '     Function: Check, Compile
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

<CLI> Module CLI

    <ExportAPI("--build")>
    <Description("build a R# package source dir as package assembly file.")>
    <Usage("--build [/src <folder, default=./> --skip-src-build /save <Rpackage.zip> --github-page <syntax highlight, default=""../../_assets/R_syntax.js""> --debug-symbols]")>
    <Argument("/src", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(String)},
              Description:="A folder path that contains the R source files and meta data files of the target R package, 
              a folder that exists in this folder path which is named 'R' is required!")>
    Public Function Compile(args As CommandLine) As Integer
        Dim src$ = RPackage.sourceHelper(args("/src") Or App.CurrentDirectory)
        Dim meta As DESCRIPTION = DESCRIPTION.Parse($"{src}/DESCRIPTION")
        Dim save$ = args("/save") Or $"{src}/../{meta.Package}_{meta.Version}.zip"
        Dim skipSourceBuild As Boolean = args("--skip-src-build")
        Dim r_syntax As String = args("--github-page") Or "../../_assets/R_syntax.js"
        Dim enableDebugSymbols As Boolean = args("--debug-symbols")

        Return meta.Compile(src, save, skipSourceBuild, r_syntax, enableDebugSymbols:=enableDebugSymbols)
    End Function

    <ExportAPI("--check")>
    <Usage("--check --target <package.zip> [--debug]")>
    <Description("Verify a packed R# package is damaged or not or check the R# script problem in a R package source folder.")>
    Public Function Check(args As CommandLine) As Integer
        Dim target As String = args <= "--target"
        Dim tmpCheck As String = TempFileSystem.GetAppSysTempFile("___check/", App.PID, "package_")
        Dim is_debug As Boolean = args("--debug")

        If target.ExtensionSuffix("zip") Then
            ' check zip package
            Return UnZip.ImprovedExtractToDirectory(
                sourceArchiveFileName:=target,
                destinationDirectoryName:=tmpCheck,
                overwriteMethod:=Overwrite.Always,
                extractToFlat:=False
            ).DoCall(AddressOf PackageLoader2.CheckPackage) _
             .CLICode
        Else
            Dim hasErr As Boolean = False
            Dim color As ConsoleColor
            Dim error$ = Nothing
            Dim exec As RProgram

            ' check R source folder
            For Each script As String In ls - l - r - "*.R" <= $"{target}/R"
                Call Console.Write($" --> check {script}...")

                error$ = Nothing
                exec = RProgram.CreateProgram(
                    Rscript:=Rscript.FromFile(script),
                    debug:=is_debug,
                    [error]:=[error]
                )

                If Not [error].StringEmpty Then
                    hasErr = True
                    color = Console.ForegroundColor

                    Console.ForegroundColor = ConsoleColor.Red

                    Call Console.WriteLine("  [syntax error]!")
                    Call Console.WriteLine("")
                    Call Console.WriteLine($"Error in R script({script}):")
                    Call Console.WriteLine([error])
                    Call Console.WriteLine()
                    Call Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
                    Call Console.WriteLine()

                    Console.ForegroundColor = color
                Else
                    Console.WriteLine("  passed.")
                End If
            Next

            Return If(hasErr, 500, 0)
        End If
    End Function
End Module

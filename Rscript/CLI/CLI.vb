#Region "Microsoft.VisualBasic::2b78731c779401d2da0283397965dbf1, Rscript\CLI\CLI.vb"

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
    '     Function: Check, Compile, runMSBuild, sourceHelper
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

<CLI> Module CLI

    <ExportAPI("--build")>
    <Description("build R# package")>
    <Usage("--build [/src <folder, default=./> --skip-src-build /save <Rpackage.zip>]")>
    <Argument("/src", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(String)},
              Description:="A folder path that contains the R source files and meta data files of the target R package, 
              a folder that exists in this folder path which is named 'R' is required!")>
    Public Function Compile(args As CommandLine) As Integer
        Dim src$ = sourceHelper(args("/src") Or App.CurrentDirectory)
        Dim meta As DESCRIPTION = DESCRIPTION.Parse($"{src}/DESCRIPTION")
        Dim save$ = args("/save") Or $"{src}/../{meta.Package}_{meta.Version}.zip"
        Dim skipSourceBuild As Boolean = args("--skip-src-build")

        ' build .net5 assembly via dotnet msbuild command?
#If netcore5 = 1 Then
        If Not skipSourceBuild Then
            Call runMSBuild(src)
        Else
            Call Console.WriteLine($"Skip MSBuild for .NET 5 runtime...")
        End If
#End If

        Using outputfile As FileStream = save.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Dim assemblyFilters As Index(Of String) = {
                "Rscript.exe", "R#.exe", "Rscript.dll", "R#.dll", "REnv.dll", "Microsoft.VisualBasic.Runtime.dll"
            }
            Dim err As Message = meta.Build(src, outputfile, assemblyFilters)

            If RProgram.isException(err) Then
                Return CInt(debug.PrintMessageInternal(err, Nothing))
            Else
                Call Console.WriteLine()
                Call Console.WriteLine($"  Source package written to {save.ParentPath.GetDirectoryFullPath}")
                Call Console.WriteLine()
            End If

            Return 0
        End Using
    End Function

#If netcore5 = 1 Then
    Private Function runMSBuild(src As String) As Boolean
        If MSBuild.version Is Nothing Then
            Return False
        End If

        For Each sln As String In ls - l - "*.sln" <= src
            Call MSBuild.BuildVsSolution(sln, rebuild:=True)
        Next

        Return True
    End Function
#End If

    Private Function sourceHelper(src As String) As String
        src = Strings.Trim(src)

        If src.StringEmpty Then
            Return Nothing
        End If

        If src.DirectoryExists Then
            Return src
        ElseIf src.ExtensionSuffix("Rproj") Then
            Return src.ParentPath
        Else
            Throw New InvalidProgramException($"invalid project target ""{src}""!")
        End If
    End Function

    <ExportAPI("--check")>
    <Usage("--check --target <package.zip>")>
    <Description("Verify a packed R# package is damaged or not?")>
    Public Function Check(args As CommandLine) As Integer
        Dim target As String = args <= "--target"
        Dim tmpCheck As String = TempFileSystem.GetAppSysTempFile("___check/", App.PID, "package_")

        Call UnZip.ImprovedExtractToDirectory(target, tmpCheck, Overwrite.Always, False)

        Return PackageLoader2.CheckPackage(libDir:=tmpCheck).CLICode
    End Function
End Module

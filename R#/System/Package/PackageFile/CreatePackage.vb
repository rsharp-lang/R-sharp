#Region "Microsoft.VisualBasic::af244ff304e4901a6f4b5ba6cd794de1, R#\System\Package\PackageFile\CreatePackage.vb"

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

'     Module CreatePackage
' 
'         Function: Build, buildRscript, buildUnixMan, checkIndex, createAssetList
'                   filter, getAssemblyList, getDataSymbols, getFileReader, loadingDependency
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development.Package.File

    Public Module CreatePackage

        <Extension>
        Private Function checkIndex(desc As DESCRIPTION) As Message

            Return Nothing
        End Function

        <Extension>
        Private Function filter(dlls As IEnumerable(Of String), assemblyFilters As Index(Of String)) As String()
            If assemblyFilters Is Nothing Then
                Return {}
            Else
                Return (From file As String
                        In dlls
                        Let fileName As String = file.FileName
                        Where Not fileName Like assemblyFilters
                        Select file).ToArray
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <param name="assemblyFilters">
        ''' all of the assembly file that appears in this index list will be 
        ''' excludes from the package build 
        ''' </param>
        ''' <returns></returns>
        Private Function getAssemblyList(dir As String, assemblyFilters As Index(Of String)) As AssemblyPack
            Dim framework As Value(Of String) = ""

            If Not dir.DirectoryExists Then
                Call Console.WriteLine("There is no .NET assembly was found in source pack.")

                Return New AssemblyPack With {
                    .assembly = {},
                    .framework = "n/a",
                    .directory = dir
                }
            End If

#If netcore5 = 0 Then
            Dim dlls As String() = dir.EnumerateFiles("*.dll").ToArray
            
            If Not dlls.IsNullOrEmpty Then
                Return New AssemblyPack With {
                    .assembly = dlls.filter(assemblyFilters),
                    .directory = dir,
                    .framework = ".NET Framework 4.8"
                }
            Else
                Return New AssemblyPack With {
                    .assembly = {},
                    .directory = dir,
                    .framework = ".NET Framework 4.8"
                }
            End If
#Else
            If (framework = $"{dir}/net5.0").DirectoryExists Then
                Return New AssemblyPack With {
                    .assembly = framework _
                        .Value _
                        .EnumerateFiles("*.dll") _
                        .filter(assemblyFilters) _
                        .ToArray,
                    .directory = framework,
                    .framework = ".NET Core 5"
                }
            Else
                Call Console.WriteLine("There is no .NET Core 5 assembly was found in source pack.")

                Return New AssemblyPack With {
                    .assembly = {},
                    .framework = ".NET Core 5",
                    .directory = dir
                }
            End If
#End If
        End Function

        ''' <summary>
        ''' build a R# package file
        ''' </summary>
        ''' <param name="target">
        ''' the target directory that contains the necessary 
        ''' files for create a R# package file.</param>
        ''' <param name="outfile">the output zip file stream</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Build(desc As DESCRIPTION, target As String, outfile As Stream, Optional assemblyFilters As Index(Of String) = Nothing) As Message
            ' R build output
            '
            ' * checking for file '../mzkit/DESCRIPTION' ... OK
            ' * preparing 'mzkit':
            ' * checking DESCRIPTION meta-information ... OK
            ' * checking for LF line-endings in source and make files and shell scripts
            ' * checking for empty or unneeded directories
            ' * building 'mzkit_0.1.0.tar.gz'

            Dim srcR As String = $"{target}/R".GetDirectoryFullPath
            Dim file As New PackageModel With {
                .info = desc,
                .symbols = New Dictionary(Of String, Expression),
                .assembly = getAssemblyList($"{target}/assembly", assemblyFilters)
            }
            Dim loading As New List(Of Expression)
            Dim [error] As New Value(Of Message)
            Dim ignores As Rbuildignore = Rbuildignore.CreatePatterns($"{target}/.Rbuildignore")
            Dim resource As Rbuildignore = Rbuildignore.CreatePatterns($"{target}/.Rbuildassets")

            Call Console.Write($"* checking for file '{(target & "/DESCRIPTION").GetFullPath}' ... ")

            If Not ([error] = desc.checkIndex) Is Nothing Then
                Call Console.WriteLine("Failed!")
                Return [error]
            Else
                Call Console.WriteLine("OK")
            End If

            Call Console.WriteLine($"* preparing '{desc.Package}':")
            Call Console.WriteLine($"* checking DESCRIPTION meta-information ... OK")
            Call Console.WriteLine($"* checking for LF line-endings in source and make files and shell scripts")
            Call Console.WriteLine($"* checking for empty or unneeded directories")

            Call Console.WriteLine($"* building '{desc.Package}_{desc.Version}.zip'")
            Call Console.WriteLine($"     compile binary script...")

            For Each script As String In srcR.ListFiles("*.R")
                If Not ([error] = file.buildRscript(script, loading)) Is Nothing Then
                    Return [error]
                End If

                Call Console.WriteLine($"        {script.FileName}... done")
            Next

            Call Console.WriteLine($"     compile unix man pages...")

            If Not [error] = file.buildUnixMan(package_dir:=target) Is Nothing Then
                Call Console.WriteLine("Failed!")
                Return [error]
            End If

            Call Console.WriteLine($"     query data items for lazy loading...")
            file.dataSymbols = getDataSymbols($"{target}/data", ignores)

            Call Console.WriteLine($"     query package dependency...")
            file.loading = loading.loadingDependency.ToArray

            Call Console.WriteLine($"     write binary zip package...")
            file.Flush(outfile, createAssetList(resource, baseDir:=target.GetDirectoryFullPath))

            Call Console.WriteLine("* done!")

            Return Nothing
        End Function

        Private Function createAssetList(resource As Rbuildignore, baseDir As String) As Dictionary(Of String, String)
            Dim assets As New Dictionary(Of String, String)

            If baseDir.Last <> "/"c Then
                baseDir = $"{baseDir}/"
            End If

            For Each file As String In baseDir.ListFiles("*.*")
                Dim name As String = file.Replace("\", "/").Replace(baseDir, "")

                If resource.IsFileIgnored(name) Then
                    Call assets.Add(name, file)
                End If
            Next

            Return assets
        End Function

        <Extension>
        Private Function buildUnixMan(file As PackageModel, package_dir As String) As Message
            Dim REngine As New RInterpreter
            Dim plugin As String = LibDLL.GetDllFile("roxygenNet.dll", REngine.globalEnvir)

            file.unixman = New Dictionary(Of String, String)

            If Not plugin.FileExists Then
                Return Nothing
            Else
                Call PackageLoader.ParsePackages(plugin) _
                    .Where(Function(pkg) pkg.namespace = "roxygen") _
                    .FirstOrDefault _
                    .DoCall(Sub(pkg)
                                Call REngine.globalEnvir.ImportsStatic(pkg.package)
                            End Sub)
            End If

            Call Console.WriteLine("       ==> roxygen::roxygenize")

            Dim err As Message = REngine.Invoke("roxygen::roxygenize", {package_dir})

            If Not err Is Nothing Then
                Return err
            Else
                Dim symbolName As String

                For Each unixMan As String In ls - l - r - "*.1" <= $"{package_dir}/man"
                    symbolName = unixMan.BaseName
                    file.unixman(symbolName) = unixMan

                    Call Console.WriteLine("        " & symbolName)
                Next
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' this function just add <see cref="PackageModel.symbols"/>
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="script"></param>
        ''' <param name="loading">
        ''' a list of dependency expression
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Friend Function buildRscript(file As PackageModel, script As String, ByRef loading As List(Of Expression)) As Message
            Dim error$ = Nothing
            Dim exec As Program = Program.CreateProgram(Rscript.FromFile(script), [error]:=[error])

            If Not [error].StringEmpty Then
                Return New Message With {
                    .level = MSG_TYPES.ERR,
                    .message = {[error]}
                }
            End If

            For Each line As Expression In exec
                If TypeOf line Is DeclareNewSymbol Then
                    Dim var As DeclareNewSymbol = line

                    If var.isTuple Then
                        Return New Message With {
                            .message = {"top level declare new symbol is not allows tuple!"},
                            .level = MSG_TYPES.ERR
                        }
                    Else
                        file.symbols(var.names(Scan0)) = var
                    End If
                ElseIf line.IsFunctionDeclare Then
                    Dim func As DeclareNewFunction = line.MakeFunction

                    If func Is Nothing Then
                        Return New Message With {
                            .level = MSG_TYPES.ERR,
                            .message = {$"'{line.GetType.Name}' is not allow in top level script when create a R# package!"}
                        }
                    Else
                        file.symbols(func.funcName) = func
                    End If
                ElseIf TypeOf line Is [Imports] OrElse TypeOf line Is Require Then
                    loading.Add(line)
                Else
                    Return New Message With {
                        .level = MSG_TYPES.ERR,
                        .message = {$"'{line.GetType.Name}' is not allow in top level script when create a R# package!"}
                    }
                End If
            Next

            Return Nothing
        End Function

        <Extension>
        Public Function MakeFunction(line As Expression) As DeclareNewFunction
            If TypeOf line Is DeclareNewFunction Then
                Return line
            ElseIf TypeOf line Is ValueAssignExpression Then
                Dim assign As ValueAssignExpression = DirectCast(line, ValueAssignExpression)

                If TypeOf assign.value Is DeclareNewFunction Then
                    Dim func As DeclareNewFunction = assign.value
                    Dim symbolName As String = ValueAssignExpression.GetSymbol(assign.targetSymbols.First)

                    If assign.targetSymbols.Length > 1 Then
                        Return Nothing
                    Else
                        func.SetSymbol(symbolName)
                        Return func
                    End If
                Else
                    Return Nothing
                End If
            Else
                Return Nothing 
            End If
        End Function

        <Extension>
        Public Function IsFunctionDeclare(line As Expression) As Boolean
            If TypeOf line Is DeclareNewFunction Then
                Return True
            ElseIf TypeOf line Is ValueAssignExpression Then
                Dim assign As ValueAssignExpression = DirectCast(line, ValueAssignExpression)

                If TypeOf assign.value Is DeclareNewFunction Then
                    Return assign.targetSymbols.Length = 1
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        <Extension>
        Friend Iterator Function loadingDependency(loading As IEnumerable(Of Expression)) As IEnumerable(Of Dependency)
            Dim allDeps = loading _
                .Where(Function(i)
                           If Not TypeOf i Is [Imports] Then
                               Return True
                           Else
                               Return Not DirectCast(i, [Imports]).isImportsScript
                           End If
                       End Function) _
                .DoCall(AddressOf Dependency.GetDependency) _
                .ToArray
            Dim load As New List(Of (name$, pkg$))

            For Each dep As Dependency In allDeps
                If dep.packages.IsNullOrEmpty Then
                    load.Add(("", dep.library))
                Else
                    dep.packages _
                        .Select(Function(name) (name, dep.library)) _
                        .DoCall(AddressOf load.AddRange)
                End If
            Next

            For Each libfile In load.GroupBy(Function(a) a.pkg)
                If libfile.Any(Function(a) a.name = "") Then
                    Yield New Dependency With {.packages = {}, .library = libfile.Key}
                Else
                    Yield New Dependency With {
                        .library = libfile.Key,
                        .packages = libfile _
                            .Select(Function(d) d.name) _
                            .Distinct _
                            .ToArray
                    }
                End If
            Next
        End Function

        Private Function getDataSymbols(dir As String, ignores As Rbuildignore) As Dictionary(Of String, String)
            Dim projDir As String = dir.ParentPath

            If projDir.Last <> "/"c Then
                projDir = $"{projDir}/"
            End If

            Return EnumerateFiles(dir, "*.*") _
                .Where(Function(filepath)
                           Return Not ignores.IsFileIgnored(filepath.Replace("\", "/").Replace(projDir, ""))
                       End Function) _
                .Select(Function(filepath)
                            Return (filepath, read:=getFileReader(filepath))
                        End Function) _
                .ToDictionary(Function(path) path.filepath,
                              Function(path)
                                  Return path.read
                              End Function)
        End Function

        Private Function getFileReader(path As String) As String
            Select Case path.ExtensionSuffix.ToLower
                Case "csv" : Return "read.csv,%s,1%,TRUE,TRUE,utf8,FALSE,$"
                Case "txt" : Return "readLines,%s,NULL"
                Case "rda" : Return "load,%s,$,FALSE"
                Case "rds" : Return "readRDS,%s,NULL,$"

                Case Else
                    Return "readBin,%s"
            End Select
        End Function
    End Module
End Namespace

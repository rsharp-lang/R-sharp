#Region "Microsoft.VisualBasic::79eacdf892473aa6bd3a2f594d1e49e8, E:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/CreatePackage.vb"

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

    '   Total Lines: 505
    '    Code Lines: 370
    ' Comment Lines: 63
    '   Blank Lines: 72
    '     File Size: 21.19 KB


    '     Module CreatePackage
    ' 
    '         Function: Build, buildRscript, buildUnixMan, checkIndex, createAssetList
    '                   filter, getAssemblyList, getDataSymbols, getFileReader, getRuntimeTags
    '                   IsFunctionDeclare, loadingDependency, MakeFunction
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

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
        ''' scan for all dll files from the given package source <paramref name="dir"/>
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
                    .directory = dir.GetDirectoryFullPath
                }
            End If

#If netcore5 = 0 Then
            Dim dlls As String() = dir.EnumerateFiles("*.dll").ToArray
            
            If Not dlls.IsNullOrEmpty Then
                Return New AssemblyPack With {
                    .assembly = dlls.filter(assemblyFilters),
                    .directory = dir.GetDirectoryFullPath,
                    .framework = ".NET Framework 4.8"
                }
            Else
                Return New AssemblyPack With {
                    .assembly = {},
                    .directory = dir.GetDirectoryFullPath,
                    .framework = ".NET Framework 4.8"
                }
            End If
#Else
            If (framework = $"{dir}/{CreatePackage.getRuntimeTags}").DirectoryExists Then
                ' may contains some native library
                ' so we list all possible dll files 
                ' in all sub-directory
                Return New AssemblyPack With {
                    .assembly = framework _
                        .Value _
                        .ListFiles("*.dll") _
                        .filter(assemblyFilters) _
                        .ToArray,
                    .directory = CStr(framework).GetDirectoryFullPath,
                    .framework = CreatePackage.getRuntimeTags
                }
            Else
                Call Console.WriteLine("There is no .NET Core 5 assembly was found in source pack.")

                Return New AssemblyPack With {
                    .assembly = {},
                    .framework = CreatePackage.getRuntimeTags,
                    .directory = dir.GetDirectoryFullPath
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
                .assembly = getAssemblyList($"{target}/assembly", assemblyFilters),
                .pkg_dir = target
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

        ''' <summary>
        ''' get assembly runtime folder tag
        ''' </summary>
        ''' <returns>net48, net6.0, net5.0</returns>
        Friend Function getRuntimeTags() As String
            Dim runtime As Version = System.Environment.Version

#If legacy Then
            Return ""
#Else
            Select Case runtime.Major
                Case >= 5 : Return $"net{runtime.Major}.0"
                Case Else
                    Return "net48"
            End Select
#End If
        End Function

        ''' <summary>
        ''' create unix .1 man page file and html help documents
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="package_dir"></param>
        ''' <returns></returns>
        <Extension>
        Private Function buildUnixMan(file As PackageModel, package_dir As String) As Message
            Dim REngine As New RInterpreter
            Dim plugin As String = LibDLL.GetDllFile("roxygenNet.dll", REngine.globalEnvir)

            file.unixman = New List(Of String)
            file.vignettes = New List(Of String)

            If Not plugin.FileExists Then
                Return Nothing
            Else
                Call REngine.LoadLibrary("JSON", silent:=True)
                Call PackageLoader.ParsePackages(plugin) _
                    .Where(Function(pkg) pkg.namespace = "roxygen") _
                    .FirstOrDefault _
                    .DoCall(Sub(pkg)
                                Call REngine.globalEnvir.ImportsStatic(pkg.package)
                            End Sub)
            End If

            Call Console.WriteLine("       ==> roxygen::roxygenize")

            ' run documentation for rscript in R folder
            Dim err As Message = REngine.Invoke("roxygen::roxygenize", {package_dir})
            Dim out As String
            Dim outputHtml As String
            Dim runtime As String = getRuntimeTags()

            Call Console.WriteLine($"       ==> build package for .NET runtime [{runtime}].")

            ' run documentation for dll modules which is marked as r package
            ' unixMan(pkg As pkg, output As String, env As Environment)
            For Each dll As String In ls - l - r - "*.dll" <= $"{package_dir}/assembly/{runtime}/"
                Dim assembly As Assembly = deps.LoadAssemblyOrCache(dll, strict:=False)
                Dim attr As IEnumerable(Of RPackageModuleAttribute)

                If assembly Is Nothing Then
                    Dim tokens = dll.ParentPath.Split("\"c, "/"c)
                    Dim levelsToAssemblyFolder = tokens _
                        .Reverse _
                        .Where(Function(str) str <> "") _
                        .Select(Function(t, i) (t, i)) _
                        .Where(Function(s) s.t = "assembly") _
                        .First.i

                    If levelsToAssemblyFolder > 1 Then
                        ' assembly/net6.0/bin/nativeLibrary.dll
                        Continue For
                    Else
                        ' throw exception
                        Return Internal.debug.stop($"invalid library image file: {dll}", REngine.globalEnvir)
                    End If
                Else
                    attr = assembly.GetCustomAttributes(Of RPackageModuleAttribute)
                End If

                If attr Is Nothing OrElse Not attr.Any Then
                    Continue For
                End If

                For Each pkg As Package In PackageLoader.ParsePackages(dll:=dll)
                    out = $"{package_dir}/man/{dll.BaseName}/{pkg.namespace}"
                    outputHtml = $"{package_dir}/vignettes/{dll.BaseName}/{pkg.namespace}"

                    Call Console.WriteLine($"         -> load: {pkg.info.Namespace}")

                    Try
                        ' create unix man page
                        ' and then create html documents
                        Call REngine.Invoke("unixMan", pkg, out, REngine.globalEnvir)
                        Call REngine.Invoke("REnv::Rdocuments", pkg, outputHtml, file.info.Package, REngine.globalEnvir)
                    Catch ex As Exception

                    End Try
                Next
            Next

            If Not err Is Nothing Then
                Return err
            Else
                Call Console.WriteLine("        " & "[*] Loading unix man page index...")

                For Each unixMan As String In ls - l - r - "*.1" <= $"{package_dir}/man"
                    Call file.unixman.Add(unixMan)
                    Call Console.WriteLine("        " & unixMan.BaseName)
                Next

                Call Console.WriteLine("        " & "[*] Loading html vignettes index...")

                For Each htmlHelp As String In ls - l - r - "*.html" <= $"{package_dir}/vignettes"
                    Call file.vignettes.Add(htmlHelp)
                    Call Console.WriteLine("        " & htmlHelp.BaseName)
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

            projDir = projDir.Replace("\", "/")

            Return (ls - l - r - "*.*" <= dir) _
                .Where(Function(filepath)
                           filepath = filepath.Replace("\", "/").Replace(projDir, "")
                           Return Not ignores.IsFileIgnored(filepath)
                       End Function) _
                .Select(Function(filepath)
                            Return (filepath, read:=getFileReader(filepath))
                        End Function) _
                .ToDictionary(Function(path) path.filepath,
                              Function(path)
                                  Return path.read
                              End Function)
        End Function

        ''' <summary>
        ''' + ``%s``, means the <paramref name="path"/> parameter value
        ''' + ``1%``, means integer value 1
        ''' + ``1#``, means float value 1
        ''' + ``$``, means the <see cref="Environment"/> parameter reference
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Private Function getFileReader(path As String) As String
            Select Case path.ExtensionSuffix.ToLower
                Case "csv" : Return "read.csv,%s,1%,TRUE,TRUE,utf8,#,FALSE,-1%,$"
                Case "txt" : Return "readLines,%s,NULL"
                Case "rda" : Return "load,%s,$,FALSE"
                Case "rds" : Return "readRDS,%s,NULL,$"

                Case Else
                    Return "readBin,%s"
            End Select
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::77afadb58ef6c02c05ac2ef7f522d36d, F:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/PackageLoader.vb"

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

    '   Total Lines: 330
    '    Code Lines: 208
    ' Comment Lines: 73
    '   Blank Lines: 49
    '     File Size: 13.35 KB


    '     Module PackageLoader2
    ' 
    '         Function: callOnLoad, CheckPackage, FindAllDllFiles, GetPackageDirectory, GetPackageIndex
    '                   GetPackageName, Hotload, loadDependency, LoadPackage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Configuration
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Development.Package.File

    ''' <summary>
    ''' Load R package folder
    ''' 
    ''' 20221122
    '''
    ''' just load the R source file at here, the
    ''' package module inside the .NET dll file its
    ''' loading procedure is lazy, load a specific dll 
    ''' file until the ``imports`` expression is 
    ''' evaluated in the R script file
    ''' </summary>
    ''' <remarks>
    ''' the package loading procedure between the <see cref="Hotload(String, GlobalEnvironment, ByRef DESCRIPTION)"/> and
    ''' <see cref="LoadPackage(String, GlobalEnvironment)"/> is similar to each other, but have some significatent 
    ''' difference between each other:
    ''' 
    ''' 1. hotload function handling the source project folder, all of the R source is the original text file
    ''' 2. loadpackage function handling the package installed folder, all of the R source is serialized as binary data file
    ''' 3. the directory folder structure is also different between the source project folder and the package library folder
    ''' 
    ''' </remarks>
    Public Module PackageLoader2

        ''' <summary>
        ''' 检查程序包的指纹是否和checksum中的结果相匹配
        ''' </summary>
        ''' <param name="libDir"></param>
        ''' <returns></returns>
        Public Function CheckPackage(libDir As String) As Boolean
            ' Error: package or namespace load failed for 'mzkit':
            '  .onLoad failed in loadNamespace() for 'mzkit', details:
            '   call: fun(libname, pkgname)
            '   error: 1

            Return True
        End Function

        ''' <summary>
        ''' get the root dir of the target R#(nuget) package.
        ''' </summary>
        ''' <param name="opt"></param>
        ''' <param name="packageName">the R# package name</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetPackageDirectory(opt As Options, packageName$) As String
            Dim libDir As String

            If App.IsMicrosoftPlatform Then
                libDir = opt.lib_loc & $"/Library/{packageName}"
            Else
                libDir = $"{opt.lib_loc}/{packageName}"
            End If

            libDir = libDir.GetDirectoryFullPath

            Return libDir
        End Function

        Public Function GetPackageName(zipFile As String) As String
            Dim index As DESCRIPTION = GetPackageIndex(zipFile)

            If index Is Nothing Then
                Return Nothing
            Else
                Return index.Package
            End If
        End Function

        Public Function GetPackageIndex(zipFile As String) As DESCRIPTION
            If Not zipFile.FileExists Then
                Return Nothing
            End If

            Using zip As New ZipArchive(zipFile.Open(FileMode.Open, doClear:=False, [readOnly]:=True), ZipArchiveMode.Read)
                If zip.GetEntry("package/index.json") Is Nothing Then
                    Return Nothing
                Else
                    Using file As Stream = zip.GetEntry("package/index.json").Open, fs As New StreamReader(file)
                        Return fs.ReadToEnd.LoadJSON(Of DESCRIPTION)
                    End Using
                End If
            End Using
        End Function

        Private Function FindAllDllFiles(projDir As String) As Dictionary(Of String, String)
#If netcore5 = 1 Then
            Return ($"{projDir}/assembly/{CreatePackage.getRuntimeTags}/") _
                .EnumerateFiles("*.dll") _
                .ToDictionary(Function(dll) dll.FileName)
#Else
            Return ($"{projDir}/assembly") _
                .EnumerateFiles("*.dll") _
                .ToDictionary(Function(dll) dll.FileName)
#End If
        End Function

        ''' <summary>
        ''' 程序包调试，测试用api
        ''' 
        ''' 在启动的时候对未进行编译的程序包进行热加载
        ''' </summary>
        ''' <param name="projDir">
        ''' the package source project directory, which is should be un-build...
        ''' for load the package directory which is installed into the R library
        ''' folder, use the method <see cref="PackageLoader2.LoadPackage"/>
        ''' instead.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function Hotload(projDir As String, env As GlobalEnvironment, Optional ByRef meta As DESCRIPTION = Nothing) As Message
            Dim [error] As New Value(Of Message)
            Dim onload As DeclareNewFunction
            Dim temp As New PackageModel With {
                .symbols = New Dictionary(Of String, Expression)
            }

            projDir = projDir.GetDirectoryFullPath
            meta = DESCRIPTION.Parse($"{projDir}/DESCRIPTION")

            If meta.Package.StringEmpty Then
                Return Internal.debug.stop(
                    message:=$"invalid project source folder: {projDir}, please check of the DESCRIPTION is file exists or not, or is invalid file format?",
                    envir:=env
                )
            End If

            Dim pkg As New PackageNamespace With {
                .libPath = projDir,
                .meta = meta,
                .assembly = FindAllDllFiles(projDir)
            }
            Dim loading As New List(Of Expression)
            Dim pkgEnv As PackageEnvironment = env.attachedNamespace.Add(pkg)

            Call VBDebugger.EchoLine($"R# package '{meta.Package}' hot load:")

            ' 1. load R symbols
            For Each script As String In $"{projDir}/R".ListFiles("*.R")
                If Not ([error] = temp.buildRscript(script, loading)) Is Nothing Then
                    Return [error]
                End If

                Call VBDebugger.EchoLine($"   {script.FileName}... done")
            Next

            onload = temp.symbols.TryGetValue(".onLoad")
            pkg.dependency = loading.loadingDependency.ToArray

            For Each symbol As KeyValuePair(Of String, Expression) In temp.symbols _
                .OrderByDescending(Function(f)
                                       ' 20220915
                                       '
                                       ' load function at first
                                       ' then evaluate other symbol expression
                                       ' due to the reason of function evaluation
                                       ' is lazy
                                       If f.Value.isCallable Then
                                           Return 1
                                       Else
                                           Return 0
                                       End If
                                   End Function)

                If symbol.Key <> ".onLoad" Then
                    Call symbol.Value.Evaluate(pkgEnv)
                End If
            Next

            ' 2. load dependency
            If Not ([error] = pkgEnv.loadDependency(pkg)) Is Nothing Then
                Return [error]
            End If

            ' 3. run '.onLoad'
            If Not onload Is Nothing Then
                Dim result = onload.Invoke(pkgEnv, params:={})

                If Program.isException(result) Then
                    Return result
                End If
            End If

            Call (From symbol As Expression
                  In temp.symbols.Values
                  Let type As Type = symbol.GetType
                  Where type.ImplementInterface(Of RFunction)
                  Select DirectCast(symbol, RFunction)) _
                     .DoCall(Sub(list)
                                 Call pkgEnv.AddSymbols(list)
                             End Sub)

            Return Nothing
        End Function

        ''' <summary>
        ''' attach installed package
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <param name="env"></param>
        Public Function LoadPackage(dir As String, env As GlobalEnvironment) As Message
            Dim result As New Value(Of Message)

            If Not result = PackageNamespace.Check(dir, env) Is Nothing Then
                Return result
            End If

            Dim [namespace] As New PackageNamespace(dir)
            Dim debugEcho As Boolean = env.debugMode
            Dim symbolExpression As Expression
            Dim symbols As New List(Of RFunction)
            Dim pkgEnv As PackageEnvironment = env.attachedNamespace.Add([namespace])

            If debugEcho Then
                Call VBDebugger.EchoLine($"load package from directory: '{dir}'.")
            End If

            ' 1. load R symbols
            For Each symbol As NamedValue(Of String) In [namespace].EnumerateSymbols
                Using bin As New BinaryReader($"{dir}/lib/src/{symbol.Value}".OpenReadonly(retryOpen:=3, verbose:=env.verboseOption))
                    symbolExpression = BlockReader _
                        .Read(bin) _
                        .Parse(desc:=[namespace].meta)

                    Call symbolExpression.Evaluate(pkgEnv)

                    If symbolExpression.GetType.ImplementInterface(Of RFunction) Then
                        Call symbols.Add(symbolExpression)
                    End If
                End Using
            Next

            ' 2. load dependency
            If Not (result = pkgEnv.loadDependency(pkg:=[namespace])) Is Nothing Then
                Return result
            End If

            ' 3. run '.onLoad'
            If Not (result = pkgEnv.callOnLoad(pkg:=[namespace])) Is Nothing Then
                Return result
            End If

            Call pkgEnv.AddSymbols(symbols)

            Return Nothing
        End Function

        ''' <summary>
        ''' 这个函数会加载通过``imports...from...``语句进行显示加载的依赖程序集
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="pkg"></param>
        <Extension>
        Private Function loadDependency(env As PackageEnvironment, pkg As PackageNamespace) As Message
            Dim result As Object

            ' require(library) at first for mount library directory
            ' and then imports modules from library dll file
            For Each dependency As Dependency In pkg.dependency _
                .SafeQuery _
                .OrderBy(Function(dp)
                             Return If(dp.library.StringEmpty, 0, 100)
                         End Function)

                If dependency.library.StringEmpty Then
                    For Each pkgName As String In dependency.packages
                        Call env.globalEnvironment.LoadLibrary(pkgName)
                    Next
                Else
                    Dim dllFile As String = pkg.FindAssemblyPath(dependency.library)

                    If Not dllFile.FileExists Then
                        result = [Imports].GetDllFile($"{dependency.library}.dll", env)

                        If TypeOf result Is Message Then
                            Return result
                        Else
                            dllFile = result
                        End If
                    End If

                    Call [Imports].LoadLibrary(dllFile, env, dependency.packages)
                End If
            Next

            Return Nothing
        End Function

        <Extension>
        Private Function callOnLoad(env As PackageEnvironment, pkg As PackageNamespace) As Message
            Dim onLoad As String = $"{pkg.libPath}/.onload"
            Dim result As Object = Nothing

            If onLoad.FileExists Then
                Using bin As New BinaryReader(onLoad.OpenReadonly(retryOpen:=5, verbose:=env.verboseOption))
                    result = BlockReader.Read(bin) _
                        .Parse(desc:=pkg.meta) _
                        .DoCall(Function(func)
                                    Return DirectCast(func, DeclareNewFunction)
                                End Function) _
                        .Invoke(env, params:={})
                End Using
            End If

            If TypeOf result Is Message Then
                Return result
            Else
                Return Nothing
            End If
        End Function
    End Module
End Namespace

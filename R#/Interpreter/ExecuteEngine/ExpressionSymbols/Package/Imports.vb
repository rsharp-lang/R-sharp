﻿#Region "Microsoft.VisualBasic::6775521b5f18904563fd8550853b1a2b, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Package\Imports.vb"

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

    '   Total Lines: 380
    '    Code Lines: 240 (63.16%)
    ' Comment Lines: 92 (24.21%)
    '    - Xml Docs: 82.61%
    ' 
    '   Blank Lines: 48 (12.63%)
    '     File Size: 16.23 KB


    '     Delegate Function
    ' 
    ' 
    '     Class [Imports]
    ' 
    '         Properties: expressionName, isImportsScript, library, packages, scriptSource
    '                     type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Evaluate, GetDllFile, GetExternalScriptFile, importsLibrary, importsPackages
    '                   isImportsAllPackages, (+3 Overloads) LoadLibrary, ToString
    ' 
    '         Sub: hook_jsEnv, hook_jsEnv_Internal
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCoreApp
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols

    Public Delegate Function ScriptParser(script As Rscript, debug As Boolean, ByRef err As String) As Program

    ''' <summary>
    ''' A syntax from imports new package namespace from the external module assembly. 
    ''' 
    ''' ```
    ''' imports [namespace] from [module.dll]
    ''' imports "script.R"
    ''' ```
    ''' 
    ''' if the module file is missing file extension name, then the file extension name 
    ''' ``dll`` will be used as default. 
    ''' </summary>
    ''' <remarks>
    ''' ``imports``关键词的功能除了可以导入dll模块之中的包模块以外，也可以导入R脚本。
    ''' 导入R脚本的功能和``source``函数保持一致，但是存在一些行为上的区别：
    ''' 
    ''' + ``source``函数要求脚本文件必须是一个正确的绝对路径或者相对路径
    ''' + ``imports``关键词则无此要求，因为imports关键词会自动进行脚本文件的搜索操作
    ''' 
    ''' 为了兼容javascript的调用，在导入模块成功的时候还会在全局环境中生成模块的变量
    ''' </remarks>
    Public Class [Imports] : Inherits Expression

        Public ReadOnly Property packages As Expression

        ''' <summary>
        ''' ``*.dll/*.R`` file name
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' dll file path may be affected by the ``attach_lib_dir`` in options,
        ''' ``attach_lib_dir`` is defined based on the ``--attach`` argument value
        ''' from the commandline.
        ''' </remarks>
        Public ReadOnly Property library As Expression
        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Imports
            End Get
        End Property

        Public ReadOnly Property scriptSource As String

        Public Const attach_lib_dir As String = NameOf(attach_lib_dir)

        ''' <summary>
        ''' 当前的语句是否是用于导入其他的脚本文件模块？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isImportsScript As Boolean
            Get
                If Not packages Is Nothing Then
                    Return False
                ElseIf Not scriptSource.FileExists Then
                    Return False
                ElseIf Not TypeOf library Is Literal Then
                    Return False
                Else
                    Dim target As String = scriptSource.ParentPath & "/" & DirectCast(library, Literal).value.ToString
                    target = target.GetFullPath
                    Return target.FileExists
                End If
            End Get
        End Property

        ''' <summary>
        ''' imports package from library
        ''' </summary>
        ''' <param name="packages"></param>
        ''' <param name="library"></param>
        ''' <param name="source">The file path of the source R# script file</param>
        Sub New(packages As Expression, library As Expression, Optional source$ = Nothing)
            Me.packages = packages
            Me.library = library
            Me.scriptSource = source
        End Sub

        ''' <summary>
        ''' imports package from library
        ''' </summary>
        ''' <param name="package"></param>
        ''' <param name="library"></param>
        Sub New(package As String, library As String)
            Me.packages = New Literal(package)
            Me.library = New Literal(library)
        End Sub

        Public Overrides Function ToString() As String
            If packages Is Nothing Then
                Return $"imports {library}"
            Else
                Return $"imports {packages} from {library}"
            End If
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            If packages Is Nothing Then
                ' imports library
                Return importsLibrary(envir)
            Else
                ' imports packages from library
                Return importsPackages(envir)
            End If
        End Function

        ''' <summary>
        ''' ```js
        ''' import "library-name"
        ''' ```
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function importsLibrary(env As Environment) As Object
            Dim files$() = CLRVector.asCharacter(library.Evaluate(env))
            Dim result As Object
            Dim oldScript As Object = env.FindSymbol("!script")?.value
            Dim oldStackFrame As StackFrame = env.stackFrame
            Dim globalEnv = env.globalEnvironment

            ' imports dll/R files
            For Each libFile As String In files
                If libFile.ExtensionSuffix("dll") Then
                    ' imports * from lib_dll
                    ' 简写形式
                    result = LoadLibrary(GetDllFile(libFile, env), env, {"*"})
                ElseIf globalEnv.polyglot.CanHandle(libFile) Then
                    ' source外部的R#脚本
                    result = GetExternalScriptFile(libFile, scriptSource, env)

                    If Program.isException(result) Then
                        Return result
                    Else
                        result = globalEnv.polyglot.LoadScript(result, env)
                    End If

                    If Program.isException(result) Then
                        Return result
                    End If
                End If
            Next

            Call env.setStackInfo(oldStackFrame)

            If Not env.FindSymbol("!script") Is Nothing Then
                env.FindSymbol("!script").setValue(oldScript, env)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' ```js
        ''' import packages from "dll-file"
        ''' ```
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function importsPackages(env As Environment) As Object
            Dim names As Index(Of String) = CLRVector.asCharacter(Me.packages.Evaluate(env))
            Dim dllName As String = any.ToString(REnv.getFirst(library.Evaluate(env)))
            Dim libDll As Object = GetDllFile(dllName, env)

            If Program.isException(libDll) Then
                Return libDll
            End If

            Dim filepath As String = any.ToString(libDll)

load:       Return LoadLibrary(filepath, env, names)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="libFile"></param>
        ''' <param name="source">
        ''' + 如果源文件存在值，则会在source脚本所处的文件夹，其中的R文件夹，当前文件夹进行搜索
        ''' + 如果源文件是空值，则会在当前文件夹以及当前文件夹中的R文件夹进行搜索
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Shared Function GetExternalScriptFile(libFile As String, source$, env As Environment) As Object
            If libFile.StringEmpty Then
                Return Internal.debug.stop("No script module provided!", env)
            ElseIf Not libFile.FileExists Then
                If source.StringEmpty Then
                    For Each location As String In {
                        $"{App.CurrentDirectory}/{libFile}",
                        $"{App.CurrentDirectory}/R/{libFile}"
                    }
                        If location.FileExists Then
                            Return location
                        End If
                    Next
                Else
                    For Each location As String In {
                        $"{source.ParentPath}/{libFile}",
                        $"{source.ParentPath}/R/{libFile}",
                        $"{App.CurrentDirectory}/{libFile}"
                    }
                        If location.FileExists Then
                            Return location
                        End If
                    Next
                End If

                Return Internal.debug.stop($"Missing script file: '{libFile}'!", env)
            End If

            Return libFile
        End Function

        ''' <summary>
        ''' the clr dll assembly file name(full name)
        ''' </summary>
        ''' <param name="libDll">the file name of the dll file, example like: ``file.dll``</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Shared Function GetDllFile(libDll As String, env As Environment) As Object
            If libDll.StringEmpty Then
                Return Internal.debug.stop("No package module provided!", env)
            ElseIf libDll.IndexOf("::") > -1 Then
                ' package::dllfile
                Dim tupleRef As NamedValue(Of String) = libDll.GetTagValue("::", trim:=True)
                Dim pkgName As String = tupleRef.Name
                Dim err As Message = env.globalEnvironment.LoadLibrary(pkgName, silent:=False)

                If Not err Is Nothing Then
                    Return err
                Else
                    Return GetDllFile(libDll:=tupleRef.Value, env:=env)
                End If

            ElseIf Not libDll.FileExists Then
                Dim searchContext As New List(Of String)
                Dim location As String = Development.Package.LibDLL.GetDllFile(libDll, env, searchContext)

                If Not location.StringEmpty Then
                    Return location
                Else
                    Return Internal.debug.stop({
                        $"Missing library file: '{libDll}'!",
                        $"Search_Context: {searchContext.Distinct.JoinBy("; ")}"
                    }, env, suppress:=True)
                End If
            End If

            Return libDll
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function isImportsAllPackages(names As Index(Of String)) As Boolean
            Return names.Objects.Length = 1 AndAlso names.Objects(Scan0) = "*"
        End Function

        Public Shared Function LoadLibrary(libDll As String, env As CollectibleAssemblyLoadContext, names As Index(Of String)) As Object
            Dim assm_buf As MemoryStream = env.GetAssemblyStream(libDll)
            Dim assembly As Assembly = env.LoadFromStream(assm_buf)

            ' solve the package modules
            Return LoadLibrary(assembly, "in-zip-memory:" & libDll, names, env.runtime)
        End Function

        Private Shared Function LoadLibrary(package_asm As Assembly, libDll$, names As Index(Of String), envir As Environment) As Object
            ' test if it is imports all modules code
            Dim importsAll As Boolean = names.DoCall(AddressOf isImportsAllPackages)
            Dim packages As Dictionary(Of String, Package)
            Dim globalEnv As GlobalEnvironment = envir.globalEnvironment

            If globalEnv.debugMode Then
                Call base.print(libDll, , envir)
            End If

            ' get packages modules
            packages = PackageLoader.ParsePackages(package_asm,) _
                .Where(Function(pkg)
                           If importsAll Then
                               Return True
                           Else
                               Return pkg.info.Namespace Like names
                           End If
                       End Function) _
                .GroupBy(Function(pkg) pkg.namespace) _
                .ToDictionary(Function(pkg) pkg.Key,
                              Function(group)
                                  Return group.First
                              End Function)

            If importsAll Then
                For Each [namespace] As Package In packages.Values
                    Call ImportsPackage.ImportsStatic(globalEnv, [namespace].package)
                    Call hook_jsEnv(globalEnv, [namespace].namespace, [namespace].package)
                Next
            Else
                For Each required As String In names.Objects
                    If packages.ContainsKey(required) Then
                        Call ImportsPackage.ImportsStatic(globalEnv, packages(required).package)
                        Call hook_jsEnv(globalEnv, required, packages(required).package)
                    Else
                        Return Internal.debug.stop({
                            $"There is no package named '{required}' in given module!",
                            $"namespace: {required}",
                            $"library module: {libDll}"}, envir
                        )
                    End If
                Next
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Load packages from a given dll module file
        ''' </summary>
        ''' <param name="libDll">A given dll module file its file path</param>
        ''' <param name="envir"></param>
        ''' <param name="names">a list of package module in target assembly file <paramref name="libDll"/></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadLibrary(libDll$, envir As Environment, names As Index(Of String)) As Object
            Return LoadLibrary(deps.LoadAssemblyOrCache(libDll, ), libDll, names, envir)
        End Function

        Public Shared Sub hook_jsEnv(globalEnv As GlobalEnvironment, symbolName As String, ParamArray libs As Type())
            Call hook_jsEnv_Internal(globalEnv, symbolName, libs)

            For Each type As Type In libs
                Dim aliases As String() = RPolyglotSymbolAttribute.GetAlternativeNames(type).ToArray

                For Each name As String In aliases
                    Call hook_jsEnv_Internal(globalEnv, name, libs)
                Next
            Next
        End Sub

        Private Shared Sub hook_jsEnv_Internal(globalEnv As GlobalEnvironment, symbolName As String, ParamArray libs As Type())
            Dim symbol As Symbol = globalEnv.polyglot.interop.FindSymbol(symbolName)

            If symbol IsNot Nothing AndAlso symbol.typeCode = TypeCodes.list Then
                Dim interopTarget As list = DirectCast(symbol.value, list)

                If interopTarget.hasName(PolyglotInteropEnvironment.pkg_ref_libs) Then
                    libs = DirectCast(interopTarget.getByName(PolyglotInteropEnvironment.pkg_ref_libs), Type()) _
                        .JoinIterates(libs) _
                        .Distinct _
                        .ToArray
                End If
            End If

            Call globalEnv.polyglot.interop.AddInteropSymbol(symbolName, libs)
        End Sub
    End Class
End Namespace

#Region "Microsoft.VisualBasic::17cce4226dbc574df455718c81e8d36a, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Package\Imports.vb"

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

    '     Class [Imports]
    ' 
    '         Properties: library, packages, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, isImportsAllPackages, LoadLibrary, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Package

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' A syntax from imports new package namespace from the external module assembly. 
    ''' 
    ''' ```
    ''' imports [namespace] from [module.dll]
    ''' imports "script.R"
    ''' ```
    ''' </summary>
    ''' <remarks>
    ''' ``imports``关键词的功能除了可以导入dll模块之中的包模块以外，也可以导入R脚本。
    ''' 导入R脚本的功能和``source``函数保持一致，但是存在一些行为上的区别：
    ''' 
    ''' + ``source``函数要求脚本文件必须是一个正确的绝对路径或者相对路径
    ''' + ``imports``关键词则无此要求，因为imports关键词会自动进行脚本文件的搜索操作
    ''' </remarks>
    Public Class [Imports] : Inherits Expression

        Public ReadOnly Property packages As Expression
        ''' <summary>
        ''' ``*.dll`` file name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property library As Expression
        Public Overrides ReadOnly Property type As TypeCodes

        Sub New(packages As Expression, library As Expression)
            Me.packages = packages
            Me.library = library
        End Sub

        Public Overrides Function ToString() As String
            Return $"imports {packages} from {library}"
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As Index(Of String) = Runtime.asVector(Of String)(Me.packages.Evaluate(envir)) _
                .AsObjectEnumerator _
                .Select(Function(o)
                            Return Scripting.ToString(o, Nothing)
                        End Function) _
                .ToArray
            Dim libDll = Scripting.ToString(Runtime.getFirst(library.Evaluate(envir)))

            If libDll.StringEmpty Then
                Return Internal.stop("No package module provided!", envir)
            ElseIf Not libDll.FileExists Then
                For Each location As String In {$"{App.HOME}/{libDll}", $"{App.HOME}/Library/{libDll}"}
                    If location.FileExists Then
                        libDll = location
                        GoTo load
                    End If
                Next
            End If

load:       Return LoadLibrary(libDll, envir, names)
        End Function

        Private Shared Function isImportsAllPackages(names As Index(Of String)) As Boolean
            Return names.Objects.Length = 1 AndAlso names.Objects(Scan0) = "*"
        End Function

        ''' <summary>
        ''' Load packages from a given dll module file
        ''' </summary>
        ''' <param name="libDll">A given dll module file its file path</param>
        ''' <param name="envir"></param>
        ''' <param name="names"></param>
        ''' <returns></returns>
        Public Shared Function LoadLibrary(libDll$, envir As Environment, names As Index(Of String)) As Object
            Dim importsAll As Boolean = names.DoCall(AddressOf isImportsAllPackages)
            Dim packages = PackageLoader.ParsePackages(libDll) _
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
            Dim globalEnv As GlobalEnvironment = envir.globalEnvironment

            If importsAll Then
                For Each [namespace] As Package In packages.Values
                    Call ImportsPackage.ImportsStatic(globalEnv, [namespace].package)
                Next
            Else
                For Each required In names.Objects
                    If packages.ContainsKey(required) Then
                        Call ImportsPackage.ImportsStatic(globalEnv, packages(required).package)
                    Else
                        Return Internal.stop({
                            $"There is no package named '{required}' in given module!",
                            $"namespace: {required}",
                            $"library module: {libDll}"}, envir
                        )
                    End If
                Next
            End If

            Return Nothing
        End Function
    End Class
End Namespace

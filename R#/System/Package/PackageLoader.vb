#Region "Microsoft.VisualBasic::7dfc3623edb396cffa6bd1a03e16441f, G:/GCModeller/src/R-sharp/R#//System/Package/PackageLoader.vb"

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

    '   Total Lines: 83
    '    Code Lines: 54
    ' Comment Lines: 18
    '   Blank Lines: 11
    '     File Size: 3.33 KB


    '     Module PackageLoader
    ' 
    '         Function: ParsePackage, parsePackageInternal, (+2 Overloads) ParsePackages, ScanDllFiles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCoreApp
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Development.Package

    Public Module PackageLoader

        ''' <summary>
        ''' 应该是只会加载静态方法
        ''' </summary>
        ''' <param name="dll">the file full path of the dll package module file</param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ParsePackages(dll$, Optional strict As Boolean = True) As IEnumerable(Of Package)
            Return deps.LoadAssemblyOrCache(dll, strict:=strict).ParsePackages(strict)
        End Function

        <Extension>
        Public Iterator Function ParsePackages(assembly As Assembly, Optional strict As Boolean = True) As IEnumerable(Of Package)
            Dim types As Type() = assembly.GetTypes
            Dim package As New Value(Of Package)

            For Each type As Type In types
                If Not (package = parsePackageInternal(type, strict:=strict)) Is Nothing Then
                    Yield CType(package, Package)
                End If
            Next
        End Function

        ''' <summary>
        ''' 这个函数会将包信息缓存下来
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ParsePackage(type As Type, Optional strict As Boolean = True) As Package
            Static packages As New Dictionary(Of Type, Package)

            Return packages.ComputeIfAbsent(
                key:=type,
                lazyValue:=Function()
                               Return parsePackageInternal(type, strict)
                           End Function)
        End Function

        Private Function parsePackageInternal(type As Type, Optional strict As Boolean = True) As Package
            Dim package As PackageAttribute = type.GetCustomAttribute(Of PackageAttribute)

            If package Is Nothing Then
                If strict Then
                    Return Nothing
                Else
                    package = New PackageAttribute(type.Name) With {
                        .Description = type.Description
                    }
                End If
            End If

            Return New Package(package, package:=type)
        End Function

        ''' <summary>
        ''' Scan the given directory and parse package from dll files.
        ''' </summary>
        ''' <param name="directory"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        Public Iterator Function ScanDllFiles(directory As String, Optional strict As Boolean = True) As IEnumerable(Of Package)
            For Each dll As String In ls - l - r - "*.dll" <= directory
                For Each package As Package In dll.ParsePackages(strict)
                    Yield package
                Next
            Next
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::a66b3b1bbb1f5ee3ebae6ebd052e6497, D:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/AssemblyPack.vb"

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

    '   Total Lines: 69
    '    Code Lines: 41
    ' Comment Lines: 19
    '   Blank Lines: 9
    '     File Size: 2.65 KB


    '     Class AssemblyPack
    ' 
    '         Properties: assembly, directory, framework
    ' 
    '         Function: GenericEnumerator, GetAllPackageContentFiles, GetEnumerator, getRelativePath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Development.Package.File

    Public Class AssemblyPack : Implements Enumeration(Of String)

        Public Property framework As String
        ''' <summary>
        ''' dll files, apply for md5 checksum calculation
        ''' </summary>
        ''' <returns>get filtered dll file modules</returns>
        ''' <remarks>
        ''' 程序会将几乎所有的文件都打包进去: <see cref="GetAllPackageContentFiles()"/>
        ''' </remarks>
        Public Property assembly As String()
        Public Property directory As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getRelativePath(fullname As String) As String
            Return PathExtensions.RelativePath(directory, fullname) _
                .Replace("\", "/") _
                .Replace($"{framework}/", "") _
                .Trim("."c, "/"c)
        End Function

        ''' <summary>
        ''' enumerates all package contents files, includes:
        ''' 
        ''' 1. dll modules
        ''' 2. deps.json files
        ''' 3. app config files
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetAllPackageContentFiles() As IEnumerable(Of String)
            For Each dll As String In assembly
                Dim depsJson As String = $"{dll.ParentPath}/{dll.BaseName}.deps.json"
                Dim appConfig As String = $"{dll.ParentPath}/{dll.FileName}.config"
                Dim runtimeConfig As String = $"{dll.ParentPath}/{dll.BaseName}.runtimeconfig.json"

                If depsJson.FileExists Then
                    Yield depsJson.GetFullPath
                End If
                If appConfig.FileExists Then
                    Yield appConfig.GetFullPath
                End If
                If runtimeConfig.FileExists Then
                    Yield runtimeConfig.GetFullPath
                End If

                Yield dll
            Next
        End Function

        ''' <summary>
        ''' enumerate all xml files for clr assembly dll documentation
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetAllPackageClrXmlDocumentFiles() As IEnumerable(Of String)
            For Each dll As String In assembly
                Dim xml As String = $"{dll.ParentPath}/{dll.BaseName}.xml"

                If xml.FileExists Then
                    Yield xml.GetFullPath
                End If
            Next
        End Function

        ''' <summary>
        ''' enumerates all dll files
        ''' </summary>
        ''' <returns>dll file file path in full name</returns>
        Public Iterator Function GenericEnumerator() As IEnumerator(Of String) Implements Enumeration(Of String).GenericEnumerator
            For Each dll As String In assembly
                Yield dll
            Next
        End Function
    End Class
End Namespace

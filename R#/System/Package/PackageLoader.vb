#Region "Microsoft.VisualBasic::288883d42f601605b146d5dee2effd85, R#\System\Package\PackageLoader.vb"

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

    '     Module PackageLoader
    ' 
    '         Function: ParsePackages, ScanDllFiles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Package

    Public Module PackageLoader

        ''' <summary>
        ''' 应该是只会加载静态方法
        ''' </summary>
        ''' <param name="dll$"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function ParsePackages(dll$, Optional strict As Boolean = True) As IEnumerable(Of Package)
            Dim types As Type() = Assembly.LoadFrom(dll.GetFullPath).GetTypes
            Dim package As PackageAttribute

            For Each type As Type In types
                package = type.GetCustomAttribute(Of PackageAttribute)

                If package Is Nothing Then
                    If strict Then
                        Continue For
                    Else
                        package = New PackageAttribute(type.Name) With {
                            .Description = type.Description
                        }
                    End If
                End If

                Yield New Package(package, package:=type)
            Next
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

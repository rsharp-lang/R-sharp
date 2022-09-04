#Region "Microsoft.VisualBasic::339216abf13e9468064acc0467b653c4, R-sharp\R#\System\Package\PackageFile\PackageNamespace.vb"

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

    '   Total Lines: 100
    '    Code Lines: 69
    ' Comment Lines: 16
    '   Blank Lines: 15
    '     File Size: 4.10 KB


    '     Class PackageNamespace
    ' 
    '         Properties: assembly, checksum, datafiles, dependency, framework
    '                     libPath, meta, packageName, runtime, symbols
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Check, EnumerateSymbols, FindAssemblyPath, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development.Package.File

    Public Class PackageNamespace

        Public Property meta As DESCRIPTION
        Public Property checksum As String
        ''' <summary>
        ''' the root directory of the package module to load
        ''' </summary>
        ''' <returns></returns>
        Public Property libPath As String

        ''' <summary>
        ''' [library.dll => md5]
        ''' </summary>
        ''' <returns></returns>
        Public Property assembly As Dictionary(Of String, String)
        Public Property dependency As Dependency()
        Public Property symbols As Dictionary(Of String, String)
        Public Property datafiles As Dictionary(Of String, NamedValue)
        Public Property runtime As AssemblyInfo
        Public Property framework As AssemblyInfo

        Public ReadOnly Property packageName As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return meta.Package
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dir">
        ''' the root directory of the package module to load
        ''' </param>
        Sub New(dir As String)
            meta = $"{dir}/package/index.json".LoadJsonFile(Of DESCRIPTION)
            checksum = $"{dir}/CHECKSUM".ReadFirstLine
            libPath = dir
            assembly = $"{dir}/package/manifest/assembly.json".LoadJsonFile(Of Dictionary(Of String, String))
            dependency = $"{dir}/package/manifest/dependency.json".LoadJsonFile(Of Dependency())
            symbols = $"{dir}/package/manifest/symbols.json".LoadJsonFile(Of Dictionary(Of String, String))
            datafiles = $"{dir}/package/manifest/data.json".LoadJsonFile(Of Dictionary(Of String, NamedValue))
            runtime = $"{dir}/package/manifest/runtime.json".LoadJsonFile(Of AssemblyInfo)
            framework = $"{dir}/package/manifest/framework.json".LoadJsonFile(Of AssemblyInfo)
        End Sub

        Public Shared Function Check(dir As String, env As Environment) As Message
            If Not dir.DirectoryExists Then Return Internal.debug.stop({$"package '{dir.BaseName}' is not installed!", $"package: {dir.BaseName}"}, env)
            If Not $"{dir}/package/index.json".FileExists Then Return Internal.debug.stop("missing package index file!", env)
            If Not $"{dir}/CHECKSUM".FileExists Then Return Internal.debug.stop("no package checksum data!", env)

            Return Nothing
        End Function

        Public Function FindAssemblyPath(assemblyName As String) As String
            ' 20220904
            ' the native library name will not be matched
            '
            If assembly.ContainsKey($"{assemblyName}.dll") Then
#If netcore5 = 1 Then
                ' is an installed package
                Dim dllFile As String = $"{libPath}/lib/assembly/{assemblyName}.dll"

                ' try as a hot load package
                If Not dllFile.FileExists Then
                    dllFile = $"{libPath}/assembly/{CreatePackage.getRuntimeTags}/{assemblyName}.dll"
                End If

                Return dllFile
#Else
                Return $"{libPath}/assembly/{assemblyName}.dll"
#End If
            Else
                Return Nothing
            End If
        End Function

        Public Iterator Function EnumerateSymbols() As IEnumerable(Of NamedValue(Of String))
            For Each item In symbols.SafeQuery
                Yield New NamedValue(Of String)(item)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return meta.ToString
        End Function

    End Class
End Namespace

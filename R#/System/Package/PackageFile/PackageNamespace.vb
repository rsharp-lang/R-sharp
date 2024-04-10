#Region "Microsoft.VisualBasic::03e679296c8d10a5040938ba6cafc5c8, D:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/PackageNamespace.vb"

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

    '   Total Lines: 121
    '    Code Lines: 81
    ' Comment Lines: 23
    '   Blank Lines: 17
    '     File Size: 4.91 KB


    '     Class PackageNamespace
    ' 
    '         Properties: assembly, checksum, datafiles, dependency, framework
    '                     libPath, meta, packageName, runtime, symbols
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Check, EnumerateSymbols, FindAssemblyPath, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports AssemblyInfo = Microsoft.VisualBasic.ApplicationServices.Development.AssemblyInfo

Namespace Development.Package.File

    Public Class PackageNamespace

        Public Property meta As DESCRIPTION
        Public Property checksum As String

        ''' <summary>
        ''' the root directory of the package module to load
        ''' </summary>
        ''' <returns></returns>
        Public Property libPath As IFileSystemEnvironment

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

        ''' <summary>
        ''' the namespace of the package library
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property packageName As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return meta.Package
            End Get
        End Property

        ''' <summary>
        ''' current package is load from the in-memory stream zip archive?
        ''' </summary>
        ''' <returns>
        ''' return true means from a in-memory zip archive stream, false means
        ''' from local filesystem
        ''' </returns>
        Public ReadOnly Property inMemory As Boolean
            Get
                Return Not TypeOf libPath Is Directory
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' create a new package namespace from source development directory
        ''' </summary>
        ''' <param name="pkgName"></param>
        ''' <param name="libpath">the local source directory for the package development</param>
        Sub New(pkgName As String, libpath As String)
            Me.meta = New DESCRIPTION With {.Package = pkgName, .Title = pkgName}
            Me.libPath = Directory.FromLocalFileSystem(libpath)
            Me.checksum = "n/a"
            Me.assembly = New Dictionary(Of String, String)
            Me.dependency = {}
            Me.symbols = New Dictionary(Of String, String)
            Me.datafiles = New Dictionary(Of String, NamedValue)
            Me.runtime = New AssemblyInfo
            Me.framework = New AssemblyInfo
        End Sub

        ''' <summary>
        ''' Create package namespace model from a local package installation location
        ''' </summary>
        ''' <param name="dir">
        ''' the root directory of the package module to load, or a zip 
        ''' stream filesystem for attach package in memory.
        ''' </param>
        Sub New(dir As IFileSystemEnvironment)
            meta = dir.ReadAllText("/package/index.json").LoadJSON(Of DESCRIPTION)
            checksum = dir.ReadAllText("/CHECKSUM").ReadFirstLine
            libPath = dir
            assembly = dir.ReadAllText($"/package/manifest/assembly.json").LoadJSON(Of Dictionary(Of String, String))
            dependency = dir.ReadAllText($"/package/manifest/dependency.json").LoadJSON(Of Dependency())
            symbols = dir.ReadAllText($"/package/manifest/symbols.json").LoadJSON(Of Dictionary(Of String, String))
            datafiles = dir.ReadAllText($"/package/manifest/data.json").LoadJSON(Of Dictionary(Of String, NamedValue))
            runtime = dir.ReadAllText($"/package/manifest/runtime.json").LoadJSON(Of AssemblyInfo)
            framework = dir.ReadAllText($"/package/manifest/framework.json").LoadJSON(Of AssemblyInfo)
        End Sub

        Dim context As CollectibleAssemblyLoadContext

        Public Function CreateLoaderContext() As CollectibleAssemblyLoadContext
            If context Is Nothing Then
                context = New CollectibleAssemblyLoadContext(Me)
            End If

            Return context
        End Function

        ''' <summary>
        ''' check of a installed package libdir location
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Shared Function Check(ByRef dir As IFileSystemEnvironment, packageName As String, env As Environment) As Message
            If dir Is Nothing Then
                Return Internal.debug.stop({$"package '{packageName}' is not installed!", $"package: {packageName}"}, env)
            End If

            If Not dir.FileExists($"/package/index.json") Then Return Internal.debug.stop("missing package index file!", env)
            If Not dir.FileExists($"/CHECKSUM") Then Return Internal.debug.stop("no package checksum data!", env)

            Return Nothing
        End Function

        ''' <summary>
        ''' try to check of the clr assembly file is existed in current package?
        ''' </summary>
        ''' <param name="assemblyName"></param>
        ''' <returns></returns>
        Public Function CheckAssemblyExist(assemblyName As String) As Boolean
            If assembly.ContainsKey($"{assemblyName}.dll") Then
                ' check if is an installed package
                Dim check_installed As Boolean = libPath.FileExists($"/lib/assembly/{assemblyName}.dll")
                ' check if try as a hot load package
                Dim check_hotload As Boolean = libPath.FileExists($"/assembly/{CreatePackage.getRuntimeTags}/{assemblyName}.dll")

                Return check_installed OrElse check_hotload
            End If

            Return False
        End Function

        ''' <summary>
        ''' try to find the clr assembly file in current library folder
        ''' </summary>
        ''' <param name="assemblyName">the dll assembly name</param>
        ''' <returns>
        ''' this function maybe returns nothing if the required assembly file 
        ''' is not exists in the package directory.
        ''' </returns>
        Public Function FindAssemblyPath(assemblyName As String) As String
            ' 20220904
            ' the native library name will not be matched
            '
            If assembly.ContainsKey($"{assemblyName}.dll") Then
#If NETCOREAPP Then
                ' is an installed package
                Dim dllFile_installed As String = $"/lib/assembly/{assemblyName}.dll"
                Dim dllFile_hotload As String = $"/assembly/{CreatePackage.getRuntimeTags}/{assemblyName}.dll"

                ' try as a hot load package
                If libPath.FileExists(dllFile_installed) Then
                    Return libPath.GetFullPath(dllFile_installed)
                End If
                If libPath.FileExists(dllFile_hotload) Then
                    Return libPath.GetFullPath(dllFile_hotload)
                End If

                Return Nothing
#Else
                Dim dllfile As String = $"/assembly/{assemblyName}.dll"

                If libPath.FileExists(dllfile) Then
                    Return libPath.GetFullPath(dllfile)
                Else
                    Return Nothing
                End If
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

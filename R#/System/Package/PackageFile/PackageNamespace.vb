#Region "Microsoft.VisualBasic::07ec2e0ec3b80159c9463447de016a96, R#\System\Package\PackageFile\PackageNamespace.vb"

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

    '     Class PackageNamespace
    ' 
    '         Properties: assembly, checksum, datafiles, dependency, framework
    '                     libPath, meta, packageName, runtime, symbols
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: EnumerateSymbols, FindAssemblyPath, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

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
        Public Property datafiles As Dictionary(Of String, String)
        Public Property runtime As AssemblyInfo
        Public Property framework As AssemblyInfo

        Public ReadOnly Property packageName As String
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
            meta = $"{dir}/index.json".LoadJsonFile(Of DESCRIPTION)
            checksum = $"{dir}/CHECKSUM".ReadFirstLine
            libPath = dir
            assembly = $"{dir}/manifest/assembly.json".LoadJsonFile(Of Dictionary(Of String, String))
            dependency = $"{dir}/manifest/dependency.json".LoadJsonFile(Of Dependency())
            symbols = $"{dir}/manifest/symbols.json".LoadJsonFile(Of Dictionary(Of String, String))
            datafiles = $"{dir}/manifest/data.json".LoadJsonFile(Of Dictionary(Of String, String))
            runtime = $"{dir}/manifest/runtime.json".LoadJsonFile(Of AssemblyInfo)
            framework = $"{dir}/manifest/framework.json".LoadJsonFile(Of AssemblyInfo)
        End Sub

        Public Function FindAssemblyPath(assemblyName As String) As String
            If assembly.ContainsKey($"{assemblyName}.dll") Then
#If netcore5 = 1 Then
                ' is an installed package
                Dim dllFile As String = $"{libPath}/assembly/{assemblyName}.dll"

                ' try as a hot load package
                If Not dllFile.FileExists Then
                    dllFile = $"{libPath}/assembly/net5.0/{assemblyName}.dll"
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
            For Each item In symbols
                Yield New NamedValue(Of String)(item)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return meta.ToString
        End Function

    End Class
End Namespace

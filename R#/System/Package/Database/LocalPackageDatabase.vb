#Region "Microsoft.VisualBasic::26ab89b5eb6baea4dc70db97ff116135, G:/GCModeller/src/R-sharp/R#//System/Package/Database/LocalPackageDatabase.vb"

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

    '   Total Lines: 125
    '    Code Lines: 88
    ' Comment Lines: 21
    '   Blank Lines: 16
    '     File Size: 4.82 KB


    '     Class LocalPackageDatabase
    ' 
    '         Properties: assemblies, numOfpackages, packages, system
    ' 
    '         Function: Build, EmptyRepository, FindPackage, GenericEnumerator, GetEnumerator
    '                   hasLibFile, hasLibPackage, Load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Development.Package

    Public Class LocalPackageDatabase : Inherits XmlDataModel
        Implements IList(Of PackageLoaderEntry)

        <XmlAttribute>
        Public Property numOfpackages As Integer Implements IList(Of PackageLoaderEntry).size
            Get
                Return assemblies.SafeQuery.Count + packages.SafeQuery.Count
            End Get
            Set(value As Integer)
                ' readonly / do nothing
            End Set
        End Property

        <XmlElement>
        Public Property system As AssemblyInfo
        ''' <summary>
        ''' .NET assembly list
        ''' </summary>
        ''' <returns></returns>
        Public Property assemblies As XmlList(Of PackageLoaderEntry)
        ''' <summary>
        ''' installed R# zip package
        ''' </summary>
        ''' <returns></returns>
        Public Property packages As XmlList(Of PackageInfo)

        ''' <summary>
        ''' Check if the given dll module <paramref name="libraryFileName"/> is exists in database or not.
        ''' </summary>
        ''' <param name="libraryFileName"></param>
        ''' <returns></returns>
        Public Function hasLibFile(libraryFileName As String) As Boolean
            If assemblies Is Nothing Then
                assemblies = New XmlList(Of PackageLoaderEntry)
            End If

            Return assemblies.items _
                .SafeQuery _
                .Any(Function(pkg)
                         Return pkg.module.assembly = libraryFileName
                     End Function)
        End Function

        ''' <summary>
        ''' If the package is not exists or load package failure
        ''' then this function returns nothing
        ''' </summary>
        ''' <param name="packageName"></param>
        ''' <param name="exception"></param>
        ''' <returns></returns>
        Public Function FindPackage(packageName As String, ByRef exception As Exception) As Package
            Dim entry As PackageLoaderEntry = assemblies.items _
                .SafeQuery _
                .Where(Function(pkg)
                           Return pkg.namespace = packageName
                       End Function) _
                .FirstOrDefault()

            If entry Is Nothing Then
                Return Nothing
            Else
                Return entry.GetLoader(exception)
            End If
        End Function

        Public Function hasLibPackage(pkgName As String) As Boolean
            If packages Is Nothing Then
                packages = New XmlList(Of PackageInfo)
            End If

            Return packages.items _
                .SafeQuery _
                .Any(Function(pkg)
                         Return pkg.namespace = pkgName
                     End Function)
        End Function

        Public Shared Function EmptyRepository() As LocalPackageDatabase
            Return New LocalPackageDatabase With {
                .assemblies = {},
                .system = GetType(LocalPackageDatabase).Assembly.FromAssembly
            }
        End Function

        Public Shared Function Load(database As String) As LocalPackageDatabase
            If database.FileLength < 100 Then
                Return EmptyRepository()
            Else
                Return database.LoadXml(Of LocalPackageDatabase)
            End If
        End Function

        Public Shared Function Build(repository As String) As LocalPackageDatabase
            Dim packages As PackageLoaderEntry() = PackageLoader _
                .ScanDllFiles(directory:=repository) _
                .Select(Function(pkg) PackageLoaderEntry.FromLoaderInfo(pkg)) _
                .ToArray
            Dim systemInfo As AssemblyInfo = GetType(LocalPackageDatabase).Assembly.FromAssembly
            Dim localDb As New LocalPackageDatabase With {
                .assemblies = packages,
                .system = systemInfo
            }

            Return localDb
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of PackageLoaderEntry) Implements Enumeration(Of PackageLoaderEntry).GenericEnumerator
            For Each package As PackageLoaderEntry In assemblies.AsEnumerable
                Yield package
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of PackageLoaderEntry).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace

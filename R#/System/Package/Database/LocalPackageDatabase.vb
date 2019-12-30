#Region "Microsoft.VisualBasic::b7cd866196844c50617646ab7df8be9f, R#\System\Package\Database\LocalPackageDatabase.vb"

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

    '     Class LocalPackageDatabase
    ' 
    '         Properties: localDb, numOfpackages, packages, system
    ' 
    '         Function: Build, EmptyRepository, FindPackage, GenericEnumerator, GetEnumerator
    '                   Load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace System.Package

    Public Class LocalPackageDatabase : Inherits XmlDataModel
        Implements IList(Of PackageLoaderEntry)

        <XmlAttribute>
        Public Property numOfpackages As Integer Implements IList(Of PackageLoaderEntry).size
            Get
                Return packages.SafeQuery.Count
            End Get
            Set(value As Integer)
                ' readonly / do nothing
            End Set
        End Property

        <XmlElement> Public Property system As AssemblyInfo

        <XmlArray>
        Public Property packages As PackageLoaderEntry()

        ''' <summary>
        ''' If the package is not exists or load package failure
        ''' then this function returns nothing
        ''' </summary>
        ''' <param name="packageName"></param>
        ''' <param name="exception"></param>
        ''' <returns></returns>
        Public Function FindPackage(packageName As String, ByRef exception As Exception) As Package
            Dim entry As PackageLoaderEntry = packages _
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

        Public Shared ReadOnly Property localDb As String = App.LocalData & "/packages.xml"

        Public Shared Function EmptyRepository() As LocalPackageDatabase
            Return New LocalPackageDatabase With {
                .packages = {},
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
                .packages = packages,
                .system = systemInfo
            }

            Return localDb
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of PackageLoaderEntry) Implements Enumeration(Of PackageLoaderEntry).GenericEnumerator
            For Each package As PackageLoaderEntry In packages
                Yield package
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of PackageLoaderEntry).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace

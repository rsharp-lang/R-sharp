Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Runtime.Package

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

        Public Function FindPackage(packageName As String) As Package
            Dim entry As PackageLoaderEntry = packages _
                .Where(Function(pkg)
                           Return pkg.namespace = packageName
                       End Function) _
                .FirstOrDefault()

            If entry Is Nothing Then
                Return Nothing
            Else
                Return entry.GetLoader
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
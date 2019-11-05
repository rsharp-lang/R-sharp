Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Runtime.Package

    Public Class LocalPackageDatabase : Inherits XmlDataModel
        Implements IList(Of PackageLoaderEntry)

        Public Property numOfpackages As Integer Implements IList(Of PackageLoaderEntry).size
            Get
                Return packages.SafeQuery.Count
            End Get
            Set(value As Integer)
                ' readonly / do nothing
            End Set
        End Property

        <XmlElement>
        Public Property packages As PackageLoaderEntry()

        Public Function GenericEnumerator() As IEnumerator(Of PackageLoaderEntry) Implements Enumeration(Of PackageLoaderEntry).GenericEnumerator
            Throw New NotImplementedException()
        End Function

        Public Function GetEnumerator() As IEnumerator Implements Enumeration(Of PackageLoaderEntry).GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
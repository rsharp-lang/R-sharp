Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Runtime.Components.Configuration

    Public Class ConfigFile : Inherits XmlDataModel
        Implements IList(Of NamedValue)

        <XmlAttribute>
        Public Property size As Integer Implements IList(Of NamedValue).size

        <XmlElement> Public Property system As AssemblyInfo
        <XmlElement> Public Property config As NamedValue()

        Public Shared ReadOnly Property localConfigs As String = App.LocalData & "/R#.configs.xml"

        Public Shared Function EmptyConfigs() As ConfigFile
            Return New ConfigFile With {
                .config = {},
                .system = GetType(ConfigFile).Assembly.FromAssembly
            }
        End Function

        Public Shared Function Load(configs As String) As ConfigFile
            If configs.FileLength < 100 Then
                Return EmptyConfigs()
            Else
                Return configs.LoadXml(Of ConfigFile)
            End If
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedValue) Implements Enumeration(Of NamedValue).GenericEnumerator
            For Each item As NamedValue In config
                Yield item
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of NamedValue).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace
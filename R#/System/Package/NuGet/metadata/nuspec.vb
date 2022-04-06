Imports System.Xml.Serialization

Namespace Development.Package.NuGet.metadata

    <XmlRoot("package", [Namespace]:=nuspec.xmlnamespace)>
    <XmlType("package", [Namespace]:=nuspec.xmlnamespace)>
    Public Class nuspec

        Public Const xmlnamespace As String = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"

        Public Property metadata As nugetmeta

        Public Overrides Function ToString() As String
            Return metadata.ToString
        End Function

    End Class

    Public Class nugetmeta

        Public Property id As String
        Public Property version As String
        <XmlElement>
        Public Property authors As String()
        Public Property requireLicenseAcceptance As Boolean
        Public Property license As tagValue
        Public Property licenseUrl As String
        Public Property icon As String
        Public Property projectUrl As String
        Public Property description As String
        Public Property copyright As String
        Public Property tags As String
        Public Property repository As tagValue
        Public Property dependencies As dependencies()
        Public Property frameworkAssemblies As frameworkAssembly()

        Public Overrides Function ToString() As String
            Return $"[{id}_{version}] {description}"
        End Function
    End Class

    Public Class tagValue

        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property url As String

        <XmlText>
        Public Property value As String

    End Class

    <XmlType("group")>
    Public Class dependencies

        <XmlAttribute>
        Public Property targetFramework As String
        <XmlElement>
        Public Property dependency As dependency()
    End Class

    Public Class dependency

        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property version As String
        <XmlAttribute> Public Property exclude As String

    End Class

    Public Class frameworkAssembly
        <XmlAttribute> Public Property assemblyName As String
        <XmlAttribute> Public Property targetFramework As String
    End Class
End Namespace
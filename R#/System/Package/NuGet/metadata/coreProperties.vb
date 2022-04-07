Imports System.Xml.Serialization
Imports openXml = SMRUCC.Rsharp.Development.Package.NuGet.metadata.Xmlns

Namespace Development.Package.NuGet.metadata

    <XmlRoot("coreProperties", [Namespace]:=OpenXML.cp)>
    Public Class coreProperties

        <XmlElement(ElementName:=NameOf(creator), [Namespace]:=openXml.dc)>
        Public Property creator As String
        <XmlElement(ElementName:=NameOf(description), [Namespace]:=openXml.dc)>
        Public Property description As String
        <XmlElement(ElementName:=NameOf(identifier), [Namespace]:=openXml.dc)>
        Public Property identifier As String
        Public Property version As String
        Public Property keywords As String
        <XmlElement(ElementName:=NameOf(lastModifiedBy), [Namespace]:=openXml.cp)>
        Public Property lastModifiedBy As String

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("cp", OpenXML.cp)
            xmlns.Add("dc", OpenXML.dc)
            xmlns.Add("dcterms", OpenXML.dcterms)
            xmlns.Add("dcmitype", OpenXML.dcmitype)
            xmlns.Add("xsi", OpenXML.xsi)
        End Sub

    End Class

    Public Module Xmlns

        Public Const cp$ = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties"
        Public Const dc$ = "http://purl.org/dc/elements/1.1/"
        Public Const dcterms$ = "http://purl.org/dc/terms/"
        Public Const dcmitype$ = "http://purl.org/dc/dcmitype/"
        Public Const xsi$ = "http://www.w3.org/2001/XMLSchema-instance"
        Public Const vt$ = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"

        Public Const r$ = "http://schemas.openxmlformats.org/officeDocument/2006/relationships"
        Public Const mc$ = "http://schemas.openxmlformats.org/markup-compatibility/2006"
        Public Const x15$ = "http://schemas.microsoft.com/office/spreadsheetml/2010/11/main"
        Public Const xr2$ = "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2"
        Public Const x15ac$ = "http://schemas.microsoft.com/office/spreadsheetml/2010/11/ac"

        Public Const x14$ = "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main"
        Public Const x14ac$ = "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"
        Public Const xr$ = "http://schemas.microsoft.com/office/spreadsheetml/2014/revision"
        Public Const xr3$ = "http://schemas.microsoft.com/office/spreadsheetml/2016/revision3"

        Public Const x16r2$ = "http://schemas.microsoft.com/office/spreadsheetml/2015/02/main"

        Public Const a$ = "http://schemas.openxmlformats.org/drawingml/2006/main"

        Public Const worksheet$ = "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"
    End Module
End Namespace
#Region "Microsoft.VisualBasic::5d838d85e4e797bc2dc1cdc3e0af9c2b, G:/GCModeller/src/R-sharp/R#//System/Package/NuGet/metadata/coreProperties.vb"

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

    '   Total Lines: 83
    '    Code Lines: 66
    ' Comment Lines: 0
    '   Blank Lines: 17
    '     File Size: 3.95 KB


    '     Class coreProperties
    ' 
    '         Properties: creator, description, identifier, keywords, lastModifiedBy
    '                     version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateMetaData, ToString
    ' 
    '     Module Xmlns
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports openXml = SMRUCC.Rsharp.Development.Package.NuGet.metadata.Xmlns

Namespace Development.Package.NuGet.metadata

    <XmlRoot("coreProperties", [Namespace]:=openXml.cp)>
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

            xmlns.Add("cp", openXml.cp)
            xmlns.Add("dc", openXml.dc)
            xmlns.Add("dcterms", openXml.dcterms)
            xmlns.Add("dcmitype", openXml.dcmitype)
            xmlns.Add("xsi", openXml.xsi)
        End Sub

        Public Overrides Function ToString() As String
            Return identifier
        End Function

        Public Shared Function CreateMetaData(index As DESCRIPTION) As coreProperties
            Dim interpreter As String = GetType(RInterpreter).Assembly.ToString
            Dim os As String = Environment.OSVersion.VersionString
            Dim runtime As String = If(Environment.Version.Major <= 4, $".NET Framework {Environment.Version}", $".NET {Environment.Version}")
            Dim core As New coreProperties With {
                .creator = index.Author,
                .description = index.Description,
                .identifier = index.Package,
                .keywords = index.meta.TryGetValue("keywords"),
                .lastModifiedBy = {interpreter, os, runtime}.JoinBy(";"),
                .version = index.Version
            }

            Return core
        End Function

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

Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Utilities

Public Module FileAndContentTypeDefinitions

    <Export>
    <Name("hid")>
    <BaseDefinition("text")>
    Public hidingContentTypeDefinition As ContentTypeDefinition

    <Export>
    <FileExtension(".hid")>
    <ContentType("hid")>
    Public hiddenFileExtensionDefinition As FileExtensionToContentTypeDefinition

End Module

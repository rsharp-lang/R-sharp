#Region "Microsoft.VisualBasic::9b3bfcefeffea78a8a9e33bd2b9cde5b, studio\VSIX-Rsharp-language\VSIX\FileAndContentTypeDefinitions.vb"

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

    ' Module FileAndContentTypeDefinitions
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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


#Region "Microsoft.VisualBasic::87d2965bc11c47634030c8b8250670fe, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//docs.vb"

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

'   Total Lines: 335
'    Code Lines: 267
' Comment Lines: 22
'   Blank Lines: 46
'     File Size: 13.04 KB


' Module docs
' 
'     Function: apiDocsHtml, getDefaultTemplate, makeHtmlDocs, makeMarkdownDocs, PackageIndex
'               parameterTable
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports MarkdownHTML = Microsoft.VisualBasic.MIME.text.markdown.MarkdownRender

''' <summary>
''' R# help document tools
''' </summary>
<Package("utils.docs", Category:=APICategories.SoftwareTools, Publisher:="I@xieguigang.me")>
Module docs

    ReadOnly markdown As New MarkdownHTML

    <ExportAPI("markdown.docs")>
    Public Function makeMarkdownDocs(package$, Optional globalEnv As GlobalEnvironment = Nothing) As String
        Dim apis As NamedValue(Of MethodInfo)() = globalEnv.packages _
            .FindPackage(package, globalEnv, Nothing) _
            .DoCall(AddressOf ImportsPackage.GetAllApi) _
            .ToArray
        Dim docs As New ScriptBuilder("")
        Dim dllName As String = "*"

        If apis.Length > 0 Then
            dllName = apis(Scan0).Value.DeclaringType.Assembly.Location.BaseName
        End If

        With docs
            !base_dll = dllName
            !packageName = package
            !packageDescription = globalEnv.packages _
                .GetPackageDocuments(package) _
                .DoCall(AddressOf markdown.Transform)
        End With

        Return docs.ToString
    End Function

    ''' <summary>
    ''' Create html help document for the specific package module
    ''' </summary>
    ''' <param name="pkgName">The package name</param>
    ''' <param name="globalEnv"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' This method create a single html help page file for generates 
    ''' pdf help manual file.
    ''' </remarks>
    <ExportAPI("makehtml.docs")>
    <RApiReturn(TypeCodes.string)>
    Public Function makeHtmlDocs(pkgName As Object,
                                 Optional template$ = Nothing,
                                 Optional package As String = Nothing,
                                 Optional globalEnv As GlobalEnvironment = Nothing) As Object

        Dim clr_pkg As [Variant](Of Message, Package) = rdocumentation.getPkg(pkgName, env:=globalEnv)

        If clr_pkg Like GetType(Message) Then
            Return clr_pkg.TryCast(Of Message)
        Else
            Return packageHelp.createHtml(
                clr_pkg:=clr_pkg.TryCast(Of Package),
                globalEnv:=globalEnv,
                template:=template,
                package:=package
            )
        End If
    End Function

    <ExportAPI("markdown_transform")>
    <Extension>
    Public Function MarkdownTransform(doc As Document) As Document
        Return New Document With {
            .author = doc.author.SafeQuery.ToArray,
            .declares = doc.declares,
            .description = roxygen.markdown.Transform(doc.description),
            .details = roxygen.markdown.Transform(doc.details),
            .examples = doc.examples,
            .keywords = doc.keywords.SafeQuery.ToArray,
            .parameters = doc.parameters _
                .SafeQuery _
                .Select(Function(par)
                            Return New NamedValue(
                                name:=par.name,
                                value:=roxygen.markdown.Transform(par.text)
                            )
                        End Function) _
                .ToArray,
            .returns = roxygen.markdown.Transform(doc.returns),
            .see_also = doc.see_also,
            .title = roxygen.markdown.Transform(doc.title)
        }
    End Function
End Module

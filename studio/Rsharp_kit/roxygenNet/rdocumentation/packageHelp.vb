#Region "Microsoft.VisualBasic::5cbd444ce6d152b05c3940bd8401af01, studio\Rsharp_kit\roxygenNet\rdocumentation\packageHelp.vb"

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

    '   Total Lines: 275
    '    Code Lines: 204 (74.18%)
    ' Comment Lines: 24 (8.73%)
    '    - Xml Docs: 12.50%
    ' 
    '   Blank Lines: 47 (17.09%)
    '     File Size: 10.37 KB


    ' Module packageHelp
    ' 
    '     Function: apiDocsHtml, buildClrExportHelpIndex, buildclrOverloads, buildExportApiHelpIndex, createHtml
    '               parameterTable
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' helper class module for create help documents for the clr type package
''' </summary>
Public Module packageHelp

    <Extension>
    Private Iterator Function buildExportApiHelpIndex(clr_pkg As Package, globalEnv As GlobalEnvironment) As IEnumerable(Of String)
        Dim apis = rdocumentation.getPkgApisList(clr_pkg, globalEnv)
        Dim Rapi As RMethodInfo
        Dim annotations As AnnotationDocs = globalEnv.packages.packageDocs

        If apis Like GetType(Message) Then
            Call globalEnv.AddMessage(apis.TryCast(Of Message))
        End If

        For Each api As NamedValue(Of MethodInfo) In apis.TryCast(Of NamedValue(Of MethodInfo)()).SafeQuery
            Rapi = New RMethodInfo(api)

            Yield annotations _
                .GetAnnotations(api.Value, requireNoneNull:=True) _
                .DoCall(AddressOf Rapi.apiDocsHtml)
        Next
    End Function

    <Extension>
    Private Iterator Function buildclrOverloads(clr_pkg As Package, globalEnv As GlobalEnvironment) As IEnumerable(Of String)
        Dim [overloads] As RGenericOverloads() = RGenericOverloads _
            .GetOverloads(clr_pkg.package) _
            .OrderBy(Function(f) f.name) _
            .ToArray
        Dim Rdocs As AnnotationDocs = globalEnv.packages.packageDocs

        For Each method As RGenericOverloads In [overloads]
            For Each type As Type In method.overloads
                Dim name As String = $"{method.name}.{type.Name.ToLower}"
                Dim innerLink As String = $"./{clr_pkg.namespace}/{name}.html"
                Dim docs =
                    <tr>
                        <td id=<%= name %>><a href=<%= innerLink %>><%= name %></a></td>
                        <td>{$summary}</td>
                    </tr>
                Dim html As New ScriptBuilder(docs)
                Dim xml As ProjectType = If(Rdocs.GetAnnotations(type), New ProjectType)

                html!summary =
                    clr_xml.typeLink(type) & ": " &
                    clr_xml.HandlingTypeReferenceInDocs(roxygen.markdown.Transform(xml.Summary))

                clr_xml.push_clr(type)

                Yield html.ToString
            Next
        Next
    End Function

    <Extension>
    Private Iterator Function buildClrExportHelpIndex(clr_pkg As Package, globalEnv As GlobalEnvironment) As IEnumerable(Of String)
        Dim clr_exports As New List(Of NamedValue(Of Type))

        For Each export As RTypeExportAttribute In clr_pkg.package _
            .GetCustomAttributes(Of RTypeExportAttribute) _
            .SafeQuery

            Call clr_exports.Add(New NamedValue(Of Type)(
                name:=export.name,
                value:=export.model
            ))
        Next

        Dim Rdocs As AnnotationDocs = globalEnv.packages.packageDocs

        For Each type As NamedValue(Of Type) In clr_exports
            Dim clr_name As String = type.Value.GetTypeElement(strict:=False).Name
            Dim clr_docs As New ScriptBuilder(
                <tr>
                    <td id=<%= type.Name %>><a href="{$link}"><%= type.Name %>: <%= clr_name %></a></td>
                    <td>{$summary}</td>
                </tr>)
            Dim xml As ProjectType = Rdocs.GetAnnotations(type)

            If xml Is Nothing Then
                xml = New ProjectType
            End If

            clr_docs!link = clr_xml.typeLink(type.Value).href([default]:="#")
            clr_docs!summary = clr_xml.HandlingTypeReferenceInDocs(roxygen.markdown.Transform(xml.Summary))

            clr_xml.push_clr(type)

            Yield clr_docs.ToString
        Next
    End Function

    Public Function createHtml(clr_pkg As Package, globalEnv As GlobalEnvironment,
                               Optional template$ = Nothing,
                               Optional package As String = Nothing) As String

        Static defaultTemplate As [Default](Of String) = "<!DOCTYPE html>" & package_template.getDefaultTemplate().ToString

        ' get template
        Dim docs As New ScriptBuilder(template Or defaultTemplate)
        Dim annotations As AnnotationDocs = globalEnv.packages.packageDocs
        ' extract all clr function which tagged with
        ' exportapi attribute
        Dim apiList As String() = clr_pkg _
            .buildclrOverloads(globalEnv) _
            .JoinIterates(clr_pkg _
                .buildExportApiHelpIndex(globalEnv)
            ) _
            .ToArray
        ' extract all clr type export data
        Dim export_docs As String() = clr_pkg _
            .buildClrExportHelpIndex(globalEnv) _
            .ToArray

        'If TypeOf pkgName Is String Then
        '    package = If(package, any.ToString(pkgName))
        '    desc = globalEnv.packages _
        '        .GetPackageDocuments(any.ToString(pkgName)) _
        '        .DoCall(AddressOf markdown.Transform)

        '    With docs
        '        !packageName = any.ToString(pkgName)
        '        !packageDescription = desc
        '        !packageRemarks = globalEnv.packages _
        '            .GetPackageDocuments(any.ToString(pkgName), remarks:=True) _
        '            .DoCall(AddressOf markdown.Transform)
        '        !apiList = apiList.JoinBy(vbCrLf)
        '        !base_dll = "*"
        '        !package = package
        '    End With
        'Else
        Dim remakrs As String = clr_pkg _
            .GetPackageDescription(globalEnv, remarks:=True) _
            .DoCall(AddressOf markdown.Transform)
        Dim desc As String = clr_pkg _
            .GetPackageDescription(globalEnv) _
            .DoCall(AddressOf markdown.Transform)

        package = If(package, clr_pkg.namespace)

        With docs
            !packageName = clr_pkg.namespace
            !packageDescription = desc
            !packageRemarks = remakrs
            !apiList = apiList.JoinBy(vbCrLf)
            !typeList = export_docs.JoinBy(vbCrLf)
            !base_dll = clr_pkg.dllName
            !package = package

            If export_docs.IsNullOrEmpty Then
                !clr_type_display = "none"
            Else
                !clr_type_display = "block"
            End If
        End With

        docs!shortDescription = Mid(desc.StripHTMLTags.TrimNewLine, 1, 64) & "..."

        If Not desc.StringEmpty Then
            desc = desc.LineTokens.FirstOrDefault
            desc = "#' " & desc.StripHTMLTags.TrimNewLine.Trim
            docs!desc_comments = desc
        End If

        Return docs.ToString
    End Function

    <Extension>
    Private Function apiDocsHtml(api As RMethodInfo, apiDocs As ProjectMember) As String
        Dim innerLink As String = $"./{api.namespace}/{api.name}.html"
        Dim docs =
            <tr>
                <td id=<%= api.name %>><a href=<%= innerLink %>><%= api.name %></a></td>
                <td>{$summary}</td>
            </tr>
        Dim html As New ScriptBuilder(docs)
        Dim displayRemarks As String
        Dim parameters$ = ""

        If api.parameters.Length > 0 Then
            parameters = apiDocs.parameterTable(api)
        End If

        With html
            !summary = markdown.Transform(apiDocs.Summary)
            !remarks = markdown.Transform(apiDocs.Remarks)
            !usage = api.GetPrintContent _
                .DoCall(AddressOf markdown.Transform) _
                .Replace("<br />", "")
            !parameters = parameters

            If apiDocs.Returns.StringEmpty Then
                !value = ""
            Else
                !value = "<h4>Value</h4>" & markdown.Transform(apiDocs.Returns)
            End If

            If apiDocs.Remarks.StringEmpty Then
                displayRemarks = "none"
            Else
                displayRemarks = "block"
            End If
        End With

        Return sprintf(html.ToString, displayRemarks)
    End Function

    <Extension>
    Private Function parameterTable(docs As ProjectMember, api As RMethodInfo) As String
        Dim list As New ScriptBuilder(
            <div>
                <h4>Arguments</h4>

                <ul>
                   {$list}
                </ul>
            </div>)
        Dim args As New List(Of XElement)
        Dim paramIndex = api.parameters.ToDictionary(Function(a) a.name)

        If docs.Params.IsNullOrEmpty Then
            Return ""
        End If

        Static env_names As Index(Of String) = {"env", "envir"}

        For Each arg In docs.Params.SafeQuery
            ' skip of the default environment parameter
            If arg.name Like env_names AndAlso paramIndex.ContainsKey(arg.name) Then
                If paramIndex(arg.name).type.mode = TypeCodes.environment Then
                    Continue For
                End If
            End If

            args +=
                <li>
                    <code><%= arg.name %></code>: {$info}
                </li>
        Next

        list!list = args _
            .Select(Function(e, i)
                        Dim li As New ScriptBuilder(e)

                        li!info = markdown _
                            .Transform(docs.Params(i).text) _
                            .GetStackValue("<p>", "</p>")

                        Return li.ToString
                    End Function) _
            .JoinBy(vbCrLf)

        Return list.ToString
    End Function

End Module

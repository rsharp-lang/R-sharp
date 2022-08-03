#Region "Microsoft.VisualBasic::2877ea3899ddba233732fc039d97ab65, R-sharp\studio\Rsharp_kit\roxygenNet\docs.vb"

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

    '   Total Lines: 235
    '    Code Lines: 185
    ' Comment Lines: 13
    '   Blank Lines: 37
    '     File Size: 9.11 KB


    ' Module docs
    ' 
    '     Function: apiDocsHtml, getDefaultTemplate, makeHtmlDocs, makeMarkdownDocs, PackageIndex
    '               parameterTable
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

''' <summary>
''' R# help document tools
''' </summary>
<Package("utils.docs", Category:=APICategories.SoftwareTools, Publisher:="I@xieguigang.me")>
Module docs

    Private Function getDefaultTemplate() As XElement
        Return <html lang="zh-CN">
                   <head>
                       <meta http-equiv="content-type" content="text/html; charset=UTF-8"/>
                       <meta http-equiv="X-UA-Compatible" content="IE=Edge"/>
                       <meta charset="utf-8"/>
                       <meta name="viewport" content="width=device-width, minimum-scale=1.0, maximum-scale=1.0"/>

                       <title>{$packageName}</title>

                       <meta name="author" content="xie.guigang@gcmodeller.org"/>
                       <meta name="copyright" content="SMRUCC genomics Copyright (c) 2020"/>
                       <meta name="keywords" content="GCModeller; Xanthomonas; Artificial Life"/>
                       <meta name="generator" content="https://github.com/xieguigang/xDoc"/>
                       <meta name="theme-color" content="#333"/>
                       <meta name="last-update" content=<%= Now.ToString(format:="yyyy-MM-dd") %>/>
                       <meta name="description" content="A software system aim at Artificial Life system design and analysis."/>

                       <meta class="foundation-data-attribute-namespace"/>
                       <meta class="foundation-mq-xxlarge"/>
                       <meta class="foundation-mq-xlarge"/>
                       <meta class="foundation-mq-large"/>
                       <meta class="foundation-mq-medium"/>
                       <meta class="foundation-mq-small"/>
                       <meta class="foundation-mq-topbar"/>
                   </head>
                   <body>
                       <table width="100%" summary=<%= "page for {{$packageName}}" %>>
                           <tbody>
                               <tr>
                                   <td>{{$packageName}}</td><td style="text-align: right;">R# Documentation</td>
                               </tr>
                           </tbody>
                       </table>

                       <h1>{$packageName}</h1>
                       <hr/>
                       <p>{$packageDescription}</p>

                       <div id="main-wrapper">
                           {$apiList}
                       </div>
                   </body>
               </html>
    End Function

    ReadOnly markdown As New MarkdownHTML

    <ExportAPI("markdown.docs")>
    Public Function makeMarkdownDocs(package$, Optional globalEnv As GlobalEnvironment = Nothing) As String
        Dim apis As NamedValue(Of MethodInfo)() = globalEnv.packages _
            .FindPackage(package, Nothing) _
            .DoCall(AddressOf ImportsPackage.GetAllApi) _
            .ToArray
        Dim docs As New ScriptBuilder("")

        With docs
            !packageName = package
            !packageDescription = globalEnv.packages _
                .GetPackageDocuments(package) _
                .DoCall(AddressOf markdown.Transform)
            ' !apiList = apiList.JoinBy("<br />")
        End With

        Return docs.ToString
    End Function

    ''' <summary>
    ''' Create html help document for the specific package module
    ''' </summary>
    ''' <param name="package">The package name</param>
    ''' <param name="globalEnv"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' This method create a single html help page file for generates pdf help manual file.
    ''' </remarks>
    <ExportAPI("makehtml.docs")>
    Public Function makeHtmlDocs(package As Object, Optional template$ = Nothing, Optional globalEnv As GlobalEnvironment = Nothing) As String
        Dim apis = rdocumentation.getPkgApisList(package, globalEnv)

        Static defaultTemplate As [Default](Of String) = "<!DOCTYPE html>" & getDefaultTemplate().ToString

        Dim docs As New ScriptBuilder(template Or defaultTemplate)
        Dim apiList As New List(Of String)
        Dim annotations As AnnotationDocs = globalEnv.packages.packageDocs
        Dim Rapi As RMethodInfo

        For Each api As NamedValue(Of MethodInfo) In apis.TryCast(Of NamedValue(Of MethodInfo)())
            Rapi = New RMethodInfo(api)
            apiList += annotations _
                .GetAnnotations(api.Value, requireNoneNull:=True) _
                .DoCall(AddressOf Rapi.apiDocsHtml)
        Next

        If TypeOf package Is String Then
            With docs
                !packageName = any.ToString(package)
                !packageDescription = globalEnv.packages _
                    .GetPackageDocuments(any.ToString(package)) _
                    .DoCall(AddressOf markdown.Transform)
                !apiList = apiList.JoinBy("<br />")
            End With
        Else
            With docs
                !packageName = DirectCast(package, Development.Package.Package).namespace
                !packageDescription = DirectCast(package, Development.Package.Package).GetPackageDescription(globalEnv)
                !apiList = apiList.JoinBy("<br />")
            End With
        End If

        Return docs.ToString
    End Function

    <Extension>
    Private Function apiDocsHtml(api As RMethodInfo, apiDocs As ProjectMember) As String
        Dim innerLink As String = $"./{api.namespace}/{api.name}.html"
        Dim docs =
            <div>
                <h2 id=<%= api.name %>><a href=<%= innerLink %>><%= api.name %></a></h2>
                <hr/>

                <p>
                    {$summary}  
                
                    <pre>{$usage}</pre>

                    {$parameters}
                       
                    {$value}

                    <span style="font-size:0.9em; display: %s">
                        <h4>Details</h4>
                        <blockquote>{$remarks}</blockquote>
                    </span>
                </p>
            </div>
        Dim html As New ScriptBuilder(docs)
        Dim displayRemarks As String
        Dim parameters$ = ""

        If api.parameters.Length > 0 Then
            parameters = apiDocs.parameterTable
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
    Private Function parameterTable(docs As ProjectMember) As String
        Dim list As New ScriptBuilder(
            <div>
                <h4>Arguments</h4>

                <ul>
                   {$list}
                </ul>
            </div>)
        Dim args As New List(Of XElement)

        If docs.Params.IsNullOrEmpty Then
            Return ""
        End If

        For Each arg In docs.Params.SafeQuery
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

    Public Function PackageIndex()
        Throw New NotImplementedException
    End Function
End Module

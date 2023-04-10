#Region "Microsoft.VisualBasic::87d2965bc11c47634030c8b8250670fe, E:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//docs.vb"

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
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

''' <summary>
''' R# help document tools
''' </summary>
<Package("utils.docs", Category:=APICategories.SoftwareTools, Publisher:="I@xieguigang.me")>
Module docs

    ''' <summary>
    ''' default template for the package module in a dll assembly file:
    ''' 
    ''' ```
    ''' imports name from dll
    ''' ```
    ''' </summary>
    ''' <returns></returns>
    Private Function getDefaultTemplate() As XElement
        Return <html lang="zh-CN">
                   <head>
                       <meta http-equiv="content-type" content="text/html; charset=UTF-8"/>
                       <meta http-equiv="X-UA-Compatible" content="IE=Edge"/>
                       <meta charset="utf-8"/>
                       <meta name="viewport" content="width=device-width, minimum-scale=1.0, maximum-scale=1.0"/>

                       <title>{$packageName}</title>

                       <meta name="author" content="xie.guigang@gcmodeller.org"/>
                       <meta name="copyright" content="SMRUCC genomics Copyright (c) 2022"/>
                       <meta name="keywords" content="R#; {$packageName}; {$base_dll}"/>
                       <meta name="generator" content="https://github.com/rsharp-lang"/>
                       <meta name="theme-color" content="#333"/>
                       <meta name="description" content="{$shortDescription}"/>

                       <meta class="foundation-data-attribute-namespace"/>
                       <meta class="foundation-mq-xxlarge"/>
                       <meta class="foundation-mq-xlarge"/>
                       <meta class="foundation-mq-large"/>
                       <meta class="foundation-mq-medium"/>
                       <meta class="foundation-mq-small"/>
                       <meta class="foundation-mq-topbar"/>

                       <style>

.table-three-line {
border-collapse:collapse; /* 关键属性：合并表格内外边框(其实表格边框有2px，外面1px，里面还有1px哦) */
border:solid #000000; /* 设置边框属性；样式(solid=实线)、颜色(#999=灰) */
border-width:2px 0 2px 0px; /* 设置边框状粗细：上 右 下 左 = 对应：1px 0 0 1px */
}
.left-1{
    border:solid #000000;border-width:1px 1px 2px 0px;padding:2px;
    font-weight:bolder;
}
.right-1{
    border:solid #000000;border-width:1px 0px 2px 1px;padding:2px;
    font-weight:bolder;
}
.mid-1{
    border:solid #000000;border-width:1px 1px 2px 1px;padding:2px;
    font-weight:bolder;
}
.left{
    border:solid #000000;border-width:1px 1px 1px 0px;padding:2px;
}
.right{
    border:solid #000000;border-width:1px 0px 1px 1px;padding:2px;
}
.mid{
    border:solid #000000;border-width:1px 1px 1px 1px;padding:2px;
}
table caption {font-size:14px;font-weight:bolder;}
</style>
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
                       <p style="
    font-size: 1.125em;
    line-height: .8em;
    margin-left: 0.5%;
    background-color: #fbfbfb;
    padding: 24px;
">
                           <code>
                               <span style="color: blue;">require</span>(<span style="color: black; font-weight: bold;">{$package}</span>);
                               <br/>
                               <br/>
                               <span style="color: green;">{$desc_comments}</span><br/>
                               <span style="color: blue;">imports</span><span style="color: brown"> "{$packageName}"</span><span style="color: blue;"> from</span><span style="color: brown"> "{$base_dll}"</span>;
                           </code>
                       </p>

                       <p>{$packageDescription}</p>

                       <blockquote>
                           <p style="font-style: italic; font-size: 0.9em;">
                           {$packageRemarks}
                           </p>
                       </blockquote>

                       <div id="main-wrapper">
                           <table class="table-three-line">
                               <tbody>{$apiList}</tbody>
                           </table>
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
    Public Function makeHtmlDocs(pkgName As Object,
                                 Optional template$ = Nothing,
                                 Optional package As String = Nothing,
                                 Optional globalEnv As GlobalEnvironment = Nothing) As String

        Dim apis = rdocumentation.getPkgApisList(pkgName, globalEnv)

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

        Dim desc As String = ""

        If TypeOf pkgName Is String Then
            package = If(package, any.ToString(pkgName))
            desc = globalEnv.packages _
                .GetPackageDocuments(any.ToString(pkgName)) _
                .DoCall(AddressOf markdown.Transform)

            With docs
                !packageName = any.ToString(pkgName)
                !packageDescription = desc
                !packageRemarks = globalEnv.packages _
                    .GetPackageDocuments(any.ToString(pkgName), remarks:=True) _
                    .DoCall(AddressOf markdown.Transform)
                !apiList = apiList.JoinBy(vbCrLf)
                !base_dll = "*"
                !package = package
            End With
        Else
            Dim remakrs As String = DirectCast(pkgName, Development.Package.Package) _
                .GetPackageDescription(globalEnv, remarks:=True) _
                .DoCall(AddressOf markdown.Transform)

            desc = DirectCast(pkgName, Development.Package.Package) _
                .GetPackageDescription(globalEnv) _
                .DoCall(AddressOf markdown.Transform)
            package = If(package, DirectCast(pkgName, Development.Package.Package).namespace)

            With docs
                !packageName = DirectCast(pkgName, Development.Package.Package).namespace
                !packageDescription = desc
                !packageRemarks = remakrs
                !apiList = apiList.JoinBy(vbCrLf)
                !base_dll = DirectCast(pkgName, Development.Package.Package).dllName
                !package = package
            End With
        End If

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

    Public Function PackageIndex()
        Throw New NotImplementedException
    End Function
End Module

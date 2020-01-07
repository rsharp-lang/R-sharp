Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Package

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
                       <meta name="copyright" content="SMRUCC genomics Copyright (c) 2019"/>
                       <meta name="keywords" content="GCModeller; Xanthomonas; Artificial Life"/>
                       <meta name="generator" content="https://github.com/xieguigang/xDoc"/>
                       <meta name="theme-color" content="#333"/>
                       <meta name="last-update" content="2019-07-21 10:11:22"/>
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

    ''' <summary>
    ''' Create html help document for the specific package module
    ''' </summary>
    ''' <param name="package$"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("makehtml.docs")>
    Public Function makeHtmlDocs(package$, Optional template$ = Nothing, Optional env As Environment = Nothing) As String
        Dim globalEnv As GlobalEnvironment = env.globalEnvironment
        Dim apis As NamedValue(Of MethodInfo)() = globalEnv.packages _
            .FindPackage(package, Nothing) _
            .DoCall(AddressOf ImportsPackage.GetAllApi) _
            .ToArray

        Static defaultTemplate As [Default](Of String) = "<!DOCTYPE html>" & getDefaultTemplate().ToString

        Dim docs As New ScriptBuilder(template Or defaultTemplate)
        Dim apiList As New List(Of String)
        Dim annotations As AnnotationDocs = globalEnv.packages.packageDocs
        Dim Rapi As RMethodInfo

        For Each api As NamedValue(Of MethodInfo) In apis
            Rapi = New RMethodInfo(api)
            apiList += annotations _
                .GetAnnotations(api.Value, requireNoneNull:=True) _
                .DoCall(AddressOf Rapi.apiDocsHtml)
        Next

        With docs
            !packageName = package
            !packageDescription = globalEnv.packages _
                .GetPackageDocuments(package) _
                .DoCall(AddressOf markdown.Transform)
            !apiList = apiList.JoinBy("<br />")
        End With

        Return docs.ToString
    End Function

    <Extension>
    Private Function apiDocsHtml(api As RMethodInfo, apiDocs As ProjectMember) As String
        Dim docs =
            <div>
                <h2 id=<%= api.name %>><%= api.name %></h2>
                <hr/>

                <p>
                    {$summary}                    
                    <pre><code>{$usage}</code></pre>

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
                .Replace("<", "&lt;") _
                .DoCall(AddressOf markdown.Transform) _
                .Replace(" ", "&nbsp;")
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
End Module

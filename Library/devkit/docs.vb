Imports System.Reflection
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

        For Each api As NamedValue(Of MethodInfo) In apis
            apiList += apiDocs(api.Name, api.Value, annotations.GetAnnotations(api.Value))
        Next

        With docs
            !packageName = package
            !packageDescription = globalEnv.packages.GetPackageDocuments(package)
            !apiList = apiList.JoinBy("<br />")
        End With

        Return docs.ToString
    End Function

    Private Function apiDocs(name$, declares As MethodInfo, api As ProjectMember) As String
        Dim docs =
            <div>
                <h3 id=<%= name %>><%= name %></h3>

                <p>{$summary}</p>
                <p style="display: %s">{$remarks}</p>
                <hr/>

            </div>
        Dim html As New ScriptBuilder(docs)
        Dim markdown As New MarkdownHTML
        Dim displayRemarks As String

        With html
            !summary = markdown.Transform(api.Summary)
            !remarks = markdown.Transform(api.Remarks)

            If api.Remarks.StringEmpty Then
                displayRemarks = "none"
            Else
                displayRemarks = "block"
            End If
        End With

        Return sprintf(html.ToString, displayRemarks)
    End Function
End Module

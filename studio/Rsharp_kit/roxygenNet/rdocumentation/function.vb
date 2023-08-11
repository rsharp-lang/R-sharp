#Region "Microsoft.VisualBasic::086620b7522d450ede3f2a86c68f4233, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//rdocumentation/function.vb"

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

'   Total Lines: 353
'    Code Lines: 301
' Comment Lines: 1
'   Blank Lines: 51
'     File Size: 16.67 KB


' Class [function]
' 
'     Function: (+2 Overloads) argument, blankTemplate, (+3 Overloads) createHtml, typeLink
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports RPackage = SMRUCC.Rsharp.Development.Package.Package

Public Class [function]

    ReadOnly markdown As New MarkdownHTML

    Public Function createHtml(api As RFunction, env As Environment) As String
        If TypeOf api Is RMethodInfo Then
            Return createHtml(DirectCast(api, RMethodInfo), env)
        Else
            Throw New NotImplementedException(api.GetType.FullName)
        End If
    End Function

    Public Function createHtml(api As RMethodInfo, template As String, env As Environment) As String
        Dim xml As ProjectMember = env.globalEnvironment _
            .packages _
            .packageDocs _
            .GetAnnotations(api.GetNetCoreCLRDeclaration)
        Dim func As New FunctionDeclare With {
            .name = api.name,
            .parameters = api.parameters _
                .Where(Function(p)
                           Return Not p.type.raw Is GetType(Environment)
                       End Function) _
                .Select(AddressOf argument) _
                .ToArray
        }
        Dim pkg As RPackage = api.GetPackageInfo
        Dim docs As New Document With {
            .declares = func,
            .title = func.name,
            .description = func.name,
            .parameters = {},
            .returns = "",
            .details = ""
        }

        If Not xml Is Nothing Then
            docs.title = Strings.Trim(xml.Summary.LineIterators.FirstOrDefault).Trim(" "c, "#"c)
            docs.description = xml.Summary _
                .LineIterators _
                .Skip(1) _
                .JoinBy("<br />") _
                .DoCall(AddressOf markdown.Transform)
            docs.parameters = xml.Params _
                .Select(AddressOf argument) _
                .ToArray
            docs.returns = markdown.Transform(xml.Returns)
            docs.details = markdown.Transform(xml.Remarks)
            docs.examples = xml.example
        End If

        If docs.returns.StringEmpty Then
            ' generate document automatically based on the return type
            Dim types As Type() = api.GetUnionTypes.ToArray

            If types.Length = 1 Then
                docs.returns = $"this function returns data object of type {typeLink(types(Scan0))}."
            ElseIf types.Length > 1 Then
                docs.returns = $"this function returns data object in these one of the listed data types: {types.Select(AddressOf typeLink).JoinBy(", ")}."
            End If
        End If

        Dim rtvl = api.GetRApiReturns

        If Not rtvl Is Nothing AndAlso rtvl.isClassGraph Then
            docs.returns = docs.returns &
                " the list data also has some specificied data fields: <code>" &
                rtvl.fields.JoinBy(", ") & "</code>."
        End If

        Return createHtml(docs, template, pkg)
    End Function

    Private Shared Function typeLink(type As Type) As String
        Dim rtype As RType = RType.GetRSharpType(type)

        Select Case rtype.mode
            Case TypeCodes.boolean,
                 TypeCodes.double,
                 TypeCodes.integer,
                 TypeCodes.list,
                 TypeCodes.NA,
                 TypeCodes.string

                Return rtype.mode.Description
            Case Else

                If type Is GetType(Object) Then
                    Return "<i>any</i> kind"
                Else
                    Return $"<a href=""/clr/{type.FullName.Replace("."c, "/"c)}.html"">{type.Name}</a>"
                End If
        End Select
    End Function

    Private Function argument(arg As param) As NamedValue
        Return New NamedValue With {
            .name = arg.name,
            .text = markdown.Transform(arg.text)
        }
    End Function

    Private Function argument(arg As RMethodArgument) As NamedValue
        Dim argName As String
        Dim argText As String

        If arg.isObjectList Then
            argName = "..."
        Else
            argName = arg.name
        End If

        If arg.isOptional Then
            If TypeOf arg.default Is Boolean Then
                argText = arg.default.ToString.ToUpper
            ElseIf TypeOf arg.default Is String OrElse TypeOf arg.default Is Char Then
                argText = $"""{arg.default}"""
            ElseIf arg.default Is Nothing Then
                argText = "NULL"
            Else
                argText = any.ToString(arg.default)
            End If
        Else
            argText = Nothing
        End If

        Return New NamedValue With {
            .name = argName.Replace("_", "."),
            .text = argText
        }
    End Function

    Public Function createHtml(docs As Document, template As String, pkg As RPackage) As String
        Dim assembly As AssemblyInfo = pkg.package.Assembly.FromAssembly

        If template.StringEmpty Then
            template = blankTemplate.ToString
        End If

        With New ScriptBuilder(template)
            !name_title = docs.declares.name
            !usage = docs.declares _
                .ToString(html:=True) _
                .Trim(" "c, ASCII.CR, ASCII.LF)
            !title = docs.title
            !summary = docs.description
            !arguments = docs.parameters _
                .Select(Function(arg)
                            Return $"
<dt>{arg.name.Replace("_", ".")}</dt>
<dd><p>{arg.text}</p></dd>
"
                        End Function) _
                .JoinBy(vbCrLf)
            !value = docs.returns Or "This function has no value returns.".AsDefault
            !details = docs.details
            !package = pkg.namespace
            !version = assembly.AssemblyVersion
            !copyright = assembly.AssemblyCopyright
            !show_details = If(docs.details.StringEmpty, "none", "block")
            !examples = docs.examples

            If docs.examples.StringEmpty Then
                !show_examples = "none"
            Else
                !show_examples = "block"
            End If

            If docs.keywords.IsNullOrEmpty Then
                !display_keywords = "none"
            End If

            If docs.author.IsNullOrEmpty Then
                !author = assembly.AssemblyCompany.Replace("<", "&lt;")
            Else
                !author = docs.author.JoinBy(", ")
            End If

            Return .ToString
        End With
    End Function

    Private Shared Function blankTemplate() As XElement
        Return <html>
                   <head>
                       <!-- Viewport mobile tag for sensible mobile support -->
                       <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1"/>
                       <meta http-equiv="Content-Type" content="text/html" charset="UTF-8"/>
                       <meta name="description" content="{$abstract}"/>

                       <title>{$name_title} function | R Documentation</title>

                       <base href="https://www.rdocumentation.org"/>
                       <link href="{$canonical_link}" rel="canonical"/>

                       <!--STYLES-->
                       <link rel="stylesheet" href="/min/production.min.702e152d1c072db370ae8520b7e2d417.css"/>
                       <link href='https://fonts.googleapis.com/css?family=Open+Sans:400,300,300italic,400italic,600,600italic,700,700italic' rel='stylesheet' type='text/css'/>
                       <link rel="stylesheet" href="https://cdn.jsdelivr.net/simplemde/latest/simplemde.min.css"/>
                       <link rel="stylesheet" href='/css/nv.d3.min.css'/>
                       <link rel="stylesheet" href='/css/bootstrap-treeview.css'/>
                       <link rel="stylesheet" href='/css/bootstrap-glyphicons.css'/>
                       <!--STYLES END-->
                   </head>

                   <body>
                       <div id="content">

                           <section class="navbar navbar-color navbar-fixed-top">
                               <nav class="clearfix">
                                   <div class="navbar--title">
                                       <a href="/">
                                           <div class="logo"></div>
                                           <div class="logo-title"><span>RDocumentation</span></div>
                                       </a>
                                   </div>
                                   <ul class="navbar--navigation largescreen">
                                       <li>
                                           <a href="/login?rdr=%2Fpackages%2Fregtomean%2Fversions%2F1.0%2Ftopics%2Flanguage_test" class="btn btn-primary">Sign in</a>
                                       </li>
                                   </ul>

                                   <div class="navbar--search">
                                       <form class="search" action="/search" method="get">
                                           <input name="q" id="searchbar" type="text" placeholder="Search for packages, functions, etc" autocomplete="off"/>
                                           <input name="latest" id="hidden_latest" type="hidden"/>
                                           <div class="search--results"></div>
                                       </form>
                                   </div>
                               </nav>
                           </section>

                           <div class="page-wrap">

                               <section class="topic packageData" data-package-name="{$package}" data-latest-version="1.0" data-dcl='false'>

                                   <header class='topic-header'>
                                       <div class="container">

                                           <div class="th--flex-position">
                                               <div>
                                                   <!-- Do not remove this div, needed for th-flex-position -->
                                                   <h1>{$name_title}</h1>
                                               </div>
                                               <div>
                                                   <!-- Do not remove this div, needed for th-flex-position -->
                                                   <div class="th--pkg-info">
                                                       <div class="th--origin">
                                                           <span>From <a href="/packages/{$package}/versions/{$version}">{$package}</a></span>
                                                           <span>by <a href="/collaborators/name/{$author}">{$author}</a></span>
                                                       </div>
                                                       <div class="th--percentile">
                                                           <div class="percentile-widget percentile-task" data-url="/api/packages/regtomean/percentile">
                                                               <span class="percentile-th">
                                                                   <span class='percentile'>0th</span>
                                                               </span>
                                                               <p>Percentile</p>
                                                           </div>
                                                       </div>
                                                   </div>
                                               </div>
                                           </div>

                                       </div>
                                   </header>

                                   <div class="container">
                                       <section>
                                           <h5>{$title}</h5>
                                           <p>{$summary}</p>
                                       </section>

                                       <section class="topic--keywords" style="display: {$display_keywords};">
                                           <div class="anchor" id="l_keywords"></div>
                                           <dl>
                                               <dt>Keywords</dt>
                                               <dd><a href="/search/keywords/datasets">datasets</a></dd>
                                           </dl>
                                       </section>

                                       <section id="usage">
                                           <div class="anchor" id="l_usage"></div>
                                           <h5 class="topic--title">Usage</h5>
                                           <pre><code class="R">{$usage}</code></pre>
                                       </section>

                                       <!-- Other info -->
                                       <div class="anchor" id="l_sections"></div>

                                       <section>
                                           <h5 class="topic--title">Arguments</h5>
                                           <dl>
                                               {$arguments}
                                           </dl>
                                       </section>

                                       <section>
                                           <div class="anchor" id="l_details"></div>
                                           <h5 class="topic--title">Details</h5>
                                           <p>{$details}</p>
                                       </section>

                                       <section class="topic--value">
                                           <div class="anchor" id="l_value"></div>
                                           <h5 class="topic--title">Value</h5>
                                           <p>{$value}</p>
                                       </section>

                                       <section class="topic--examples">
                                           <div class="anchor" id="l_examples"></div>
                                           <h5 class="topic--title">Examples</h5>
                                           <p><pre><code>{$examples}</code></pre></p>
                                       </section>

                                       <section style="display: none;">
                                           <div class="anchor" id="alss"></div>
                                           <h5 class="topic--title">Aliases</h5>
                                           <ul class="topic--aliases">
                                               <li>{$name_title}</li>
                                           </ul>
                                       </section>

                                       <small>
                                           <i>Documentation reproduced from package <span itemprop="name">{$package}</span>, version <span itemprop="version">{$version}</span>, License: {$copyright}</i>
                                       </small>
                                   </div>
                               </section>

                           </div>

                           <div class="footer">
                               <div class="navbar--title footer-largescreen pull-right">

                                   <a href="https://github.com/SMRUCC/R-sharp" class="js-external">
                                       <div class="github"></div>
                                       <div class="logo-title">R# language</div>
                                   </a>

                               </div>
                               <div class="footer--credits--title">
                                   <p class="footer--credits">Created by <a href="https://github.com/SMRUCC/R-sharp" class="js-external">roxygenNet for R# language</a></p>
                               </div>
                           </div>

                       </div>
                   </body>
               </html>
    End Function
End Class

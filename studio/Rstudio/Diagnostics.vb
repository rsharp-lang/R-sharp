#Region "Microsoft.VisualBasic::d32ab64173bf21e24c25d8280bdb510d, studio\Rstudio\Diagnostics.vb"

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

    '   Total Lines: 217
    '    Code Lines: 74 (34.10%)
    ' Comment Lines: 126 (58.06%)
    '    - Xml Docs: 81.75%
    ' 
    '   Blank Lines: 17 (7.83%)
    '     File Size: 10.31 KB


    ' Module Diagnostics
    ' 
    '     Function: help
    ' 
    '     Sub: view
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports MarkdownHTML = Microsoft.VisualBasic.MIME.text.markdown.MarkdownRender

<Package("diagnostics")>
Module Diagnostics

    ''' <summary>
    ''' A helper api for invoke a Data Viewer
    ''' 
    ''' Invoke a spreadsheet-style data viewer on a matrix-like R object.
    ''' </summary>
    ''' <param name="symbol">
    ''' an R object which can be coerced to a data frame with non-zero 
    ''' numbers of rows and columns.
    ''' </param>
    ''' <param name="env"></param>
    ''' <remarks>
    ''' Object x is coerced (if possible) to a data frame, then columns 
    ''' are converted to character using format.data.frame. The object 
    ''' is then viewed in a spreadsheet-like data viewer, a read-only 
    ''' version of data.entry.
    '''
    ''' If there are row names On the data frame that are Not 1:nrow, 
    ''' they are displayed in a separate first column called 
    ''' ``row.names``.
    '''
    ''' Objects with zero columns Or zero rows are Not accepted.
    '''
    ''' The array Of cells can be navigated via the scrollbars And by 
    ''' the cursor keys, Home, End, Page Up And Page Down.
    '''
    ''' The initial size Of the data viewer window Is taken from the 
    ''' Default dimensions Of a pager (see Rconsole), but adjusted 
    ''' downwards To show a whole number Of rows And columns.
    ''' </remarks>
    <ExportAPI("view")>
    Public Sub view(<RRawVectorArgument> symbol As Object, Optional env As Environment = Nothing)
        Dim buffer = env.globalEnvironment.stdout

        If symbol Is Nothing Then
            buffer.Write("null", "inspector/json")
        ElseIf TypeOf symbol Is dataframe Then
            buffer.Write(DirectCast(symbol, dataframe), env.globalEnvironment, "inspector/csv")
        ElseIf TypeOf symbol Is Image OrElse TypeOf symbol Is Bitmap Then
            buffer.Write(New DataURI(CType(symbol, Image)).ToString, "inspector/image")
        ElseIf TypeOf symbol Is GraphicsData Then
            Using bytes As New MemoryStream
                With DirectCast(symbol, GraphicsData)
                    Call .Save(bytes)
                    Call bytes.Flush()

                    Dim base64 As New DataURI(
                        base64:=bytes.ToArray.ToBase64String,
                        mime:= .content_type
                    )

                    buffer.Write(base64.ToString, "inspector/image")
                End With
            End Using
        Else
            Dim digest As New Dictionary(Of Type, Func(Of Object, Object))

            digest.Add(GetType(list), Function(obj) DirectCast(obj, list).slots)
            digest.Add(GetType(vector), Function(obj) DirectCast(obj, vector).data)
            digest.Add(GetType(vbObject), Function(obj) DirectCast(obj, vbObject).target)

            Dim opts As New JSONSerializerOptions With {.digest = digest}
            Dim json As String = ObjectSerializer _
                .GetJsonElement(symbol.GetType(), symbol, opts) _
                .BuildJsonString(opts)

            Call buffer.Write(json, "inspector/json")
        End If
    End Sub

    ''' <summary>
    ''' ``help`` is the primary interface to the help systems.
    ''' </summary>
    ''' <param name="symbol">
    ''' usually, a name or character string specifying the topic for 
    ''' which help is sought. A character string (enclosed in explicit 
    ''' single or double quotes) is always taken as naming a topic.
    ''' 
    ''' If the value Of topic Is a length-one character vector the 
    ''' topic Is taken To be the value Of the only element. Otherwise 
    ''' topic must be a name Or a reserved word (If syntactically 
    ''' valid) Or character String.
    ''' 
    ''' See 'Details’ for what happens if this is omitted.
    ''' 
    ''' this symbol can be given a prefix of package name, which is 
    ''' a name or character vector giving the packages to look into for 
    ''' documentation, or NULL. By default, all packages whose namespaces 
    ''' are loaded are used. To avoid a name being deparsed use e.g. 
    ''' (pkg_ref) (see the examples).
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' The following types of help are available:
    ''' 
    '''  + Plain text help
    '''  + HTML help pages With hyperlinks To other topics, shown In a browser by 
    '''    browseURL. If For some reason HTML help Is unavailable (see startDynamicHelp), 
    '''    plain text help will be used instead.
    '''  + For help only, typeset as PDF – see the section on 'Offline help’.
    ''' 
    ''' The Default For the type Of help Is selected When R Is installed – 
    ''' the 'factory-fresh’ default is HTML help.
    ''' 
    ''' The rendering Of text help will use directional quotes In suitable 
    ''' locales (UTF-8 And Single-Byte Windows locales): sometimes the fonts 
    ''' used Do Not support these quotes so this can be turned off by setting 
    ''' options(useFancyQuotes = False).
    ''' 
    ''' topic Is Not optional If it Is omitted R will give
    ''' 
    '''  + If a package Is specified, (text Or, In interactive use only, HTML) 
    '''    information On the package, including hints/links To suitable help 
    '''    topics.
    '''  + If lib.loc only Is specified, a (text) list of available packages.
    '''  + Help on help itself if none of the first three arguments Is specified.
    ''' 
    ''' Some topics need To be quoted (by backticks) Or given As a character String. 
    ''' These include those which cannot syntactically appear On their own such 
    ''' As unary And binary operators, Function And control-flow reserved words 
    ''' (including If, Else For, In, repeat, While, break And Next). The other 
    ''' reserved words can be used As If they were names, For example True, NA And 
    ''' Inf.
    ''' 
    ''' If multiple help files matching topic are found, In interactive use a menu 
    ''' Is presented For the user To choose one: in batch use the first on the search 
    ''' path Is used. (For HTML help the menu will be an HTML page, otherwise a 
    ''' graphical menu if possible if getOption("menu.graphics") Is true, the default.)
    ''' 
    ''' Note that HTML help does Not make use Of Lib.loc: it will always look first 
    ''' In the loaded packages And Then along .libPaths().
    ''' 
    ''' Offline help
    ''' 
    ''' Typeset documentation Is produced by running the LaTeX version Of the help 
    ''' page through pdflatex: this will produce a PDF file.
    ''' 
    ''' The appearance Of the output can be customized through a file 'Rhelp.cfg’ 
    ''' somewhere in your LaTeX search path: this will be input as a LaTeX style 
    ''' file after Rd.sty. Some environment variables are consulted, notably 
    ''' R_PAPERSIZE (via getOption("papersize")) and R_RD4PDF (see ‘Making manuals’ 
    ''' in the ‘R Installation and Administration Manual’).
    ''' 
    ''' If there Is a Function offline_help_helper In the workspace Or further 
    ''' down the search path it Is used To Do the typesetting, otherwise the 
    ''' Function Of that name In the utils Namespace (To which the first paragraph 
    ''' applies). It should accept at least two arguments, the name Of the LaTeX 
    ''' file To be typeset And the type (which Is nowadays ignored). It accepts 
    ''' a third argument, texinputs, which will give the graphics path When the 
    ''' help document contains figures, And will otherwise Not be supplied.
    ''' 
    ''' Note
    ''' 
    ''' Unless lib.loc Is specified explicitly, the loaded packages are searched 
    ''' before those in the specified libraries. This ensures that if a library 
    ''' Is loaded from a library Not in the known library trees, then the help from 
    ''' the loaded library Is used. If lib.loc Is specified explicitly, the loaded 
    ''' packages are Not searched.
    ''' 
    ''' If this search fails And argument Try.all.packages Is True And neither 
    ''' packages nor Lib.loc Is specified, Then all the packages In the known 
    ''' library trees are searched For help On topic And a list Of (any) packages 
    ''' where help may be found Is displayed (With hyperlinks For help_type = "html"). 
    ''' NB: searching all packages can be slow, especially the first time (caching 
    ''' Of files by the OS can expedite subsequent searches dramatically).
    ''' </remarks>
    <ExportAPI("help")>
    Public Function help(symbol As Object, Optional env As Environment = Nothing) As Message
        If TypeOf symbol Is String Then
            symbol = env.FindSymbol(symbol)?.value
        End If

        If symbol Is Nothing Then
            Return debug.stop("symbol object can not be nothing!", env)
        ElseIf Not TypeOf symbol Is RMethodInfo Then
            Return debug.stop("unsupport symbol object type!", env)
        End If

        Dim markdownText As String

        Using buffer As New MemoryStream
            Dim markdown As New RContentOutput(New StreamWriter(buffer, Encodings.UTF8WithoutBOM.CodePage), OutputEnvironments.Html)

            Call env.globalEnvironment _
               .packages _
               .packageDocs _
               .PrintHelp(DirectCast(symbol, RMethodInfo), markdown)
            Call markdown.Flush()

            markdownText = Encoding.UTF8.GetString(buffer.ToArray)
        End Using

        Dim html As String = New MarkdownHTML().Transform(markdownText)

        Call env.globalEnvironment.stdout.Write(html, "inspector/api")

        Return Nothing
    End Function
End Module

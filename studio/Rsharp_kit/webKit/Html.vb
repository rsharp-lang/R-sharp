#Region "Microsoft.VisualBasic::3fa24e75f9cd7d012ff7cc5aea59a6dd, studio\Rsharp_kit\webKit\Html.vb"

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

    '   Total Lines: 282
    '    Code Lines: 182 (64.54%)
    ' Comment Lines: 60 (21.28%)
    '    - Xml Docs: 88.33%
    ' 
    '   Blank Lines: 40 (14.18%)
    '     File Size: 10.89 KB


    ' Module Html
    ' 
    '     Function: anchor_table, castModel, documentDebugView, getElementById, getElementsByClass
    '               getElementsByTagName, getPlainText, links, parse, QueryHtmlTables
    '               title, to_html, TrimWhitespace
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports Microsoft.VisualBasic.Text.Xml
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' Html document tools
''' </summary>
<Package("Html", Category:=APICategories.UtilityTools)>
Module Html

    Sub Main()
        Call RInternal.ConsolePrinter.AttachConsoleFormatter(Of HtmlElement)(AddressOf documentDebugView)
        Call RInternal.ConsolePrinter.AttachConsoleFormatter(Of HtmlDocument)(AddressOf documentDebugView)

        Call RInternal.generic.add(htmlPrinter.toHtml_apiName, GetType(HtmlElement), AddressOf to_html)

        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(Anchor()), AddressOf anchor_table)
    End Sub

    ''' <summary>
    ''' get html text
    ''' </summary>
    ''' <param name="html"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <RGenericOverloads(htmlPrinter.toHtml_apiName)>
    Private Function to_html(html As HtmlElement, args As list, env As Environment) As String
        Return html.GetHtmlText
    End Function

    <RGenericOverloads("as.data.frame")>
    Private Function anchor_table(anchors As Anchor(), args As list, env As Environment) As dataframe
        Dim df As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        Call df.add("id", From a As Anchor In anchors Select a.id)
        Call df.add("class", From a As Anchor In anchors Select a.class)
        Call df.add("style", From a As Anchor In anchors Select a.style)
        Call df.add("href", From a As Anchor In anchors Select a.href)
        Call df.add("target", From a As Anchor In anchors Select a.target)
        Call df.add("rel", From a As Anchor In anchors Select a.rel)
        Call df.add("title", From a As Anchor In anchors Select a.title)
        Call df.add("download", From a As Anchor In anchors Select a.download)
        Call df.add("hreflang", From a As Anchor In anchors Select a.hreflang)
        Call df.add("type", From a As Anchor In anchors Select a.type)
        Call df.add("media", From a As Anchor In anchors Select a.media)
        Call df.add("text", From a As Anchor In anchors Select a.text)

        Return df
    End Function

    Private Function documentDebugView(doc As HtmlElement) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine($"<{doc.TagName}>...</{doc.TagName}>")

        If Not doc.Attributes.IsNullOrEmpty Then
            Call sb.AppendLine($" {doc.Attributes.Length} attributes:")

            For Each attr As ValueAttribute In doc.Attributes
                Call sb.AppendLine($" {attr.Name} = {attr.Value}")
            Next
        End If
        If Not doc.HtmlElements.IsNullOrEmpty Then
            Call sb.AppendLine()
            Call sb.AppendLine($" {doc.HtmlElements.Length} child nodes:")

            For Each node As InnerPlantText In doc.HtmlElements
                Call sb.AppendLine(node.ToString)
            Next
        End If

        Return sb.ToString
    End Function

    <ExportAPI("parse")>
    Public Function parse(html As String, Optional strip As Boolean = False) As HtmlDocument
        Return HtmlDocument.LoadDocument(html, strip:=strip)
    End Function

    ''' <summary>
    ''' get html element by its id
    ''' </summary>
    ''' <param name="doc"></param>
    ''' <param name="id"></param>
    ''' <returns></returns>
    <ExportAPI("getElementById")>
    <RApiReturn(GetType(HtmlElement))>
    Public Function getElementById(doc As HtmlElement, id As String) As Object
        Return doc.getElementById(id)
    End Function

    ''' <summary>
    ''' get a collection of elements with specific tag name
    ''' </summary>
    ''' <param name="doc"></param>
    ''' <param name="name"></param>
    ''' <returns>a vector of the target html elements</returns>
    <ExportAPI("getElementsByTagName")>
    <RApiReturn(GetType(HtmlElement))>
    Public Function getElementsByTagName(doc As HtmlElement, name As String) As Object
        Return doc.getElementsByTagName(name)
    End Function

    <ExportAPI("getElementsByClass")>
    <RApiReturn(GetType(HtmlElement))>
    Public Function getElementsByClass(doc As HtmlElement, [class] As String) As Object
        Return doc.getElementsByClassName([class]).ToArray
    End Function

    ''' <summary>
    ''' cast the document element model as the node element object model?
    ''' </summary>
    ''' <param name="doc"></param>
    ''' <returns></returns>
    <ExportAPI("cast")>
    Public Function castModel(<RRawVectorArgument> doc As Object, Optional env As Environment = Nothing) As Object
        If doc Is Nothing Then
            Return Nothing
        End If

        Dim pull As pipeline = pipeline.TryCreatePipeline(Of HtmlElement)(doc, env)

        If pull.isError Then
            Return pull.getError
        End If

        ' 20241121 do not use List(of Node)
        ' or the vector this function produced will be cast to Node[] generic array
        ' due to the reason of array of node is already generic
        Dim castTo As New List(Of Object)

        For Each element As HtmlElement In pull.populates(Of HtmlElement)(env)
            Select Case Strings.LCase(element.TagName)
                Case "a" : Call castTo.Add(Anchor.FromElement(element))
                Case Else
                    Throw New NotImplementedException(element.TagName)
            End Select
        Next

        Return TryCastGenericArray(castTo.ToArray, env)
    End Function

    ''' <summary>
    ''' query a list of html tables in the given html page text document
    ''' </summary>
    ''' <param name="html">text string in html format</param>
    ''' <param name="plain_text">
    ''' parse the table cell content into plain text data. set this parameter false will keeps the input html content for cell data. 
    ''' </param>
    ''' <returns>
    ''' a tuple list of the dataframe data that parsed from the html tables.
    ''' the tuple list key names is used the table id prefer.
    ''' </returns>
    ''' 
    <ExportAPI("tables")>
    Public Function QueryHtmlTables(html As String,
                                    Optional del_newline As Boolean = True,
                                    Optional filter As Boolean = False,
                                    Optional plain_text As Boolean = True) As list

        Dim tables As String() = html.GetTablesHTML
        Dim result As New list(RType.GetRSharpType(GetType(dataframe))) With {
            .slots = New Dictionary(Of String, Object)
        }

        For Each text As String In tables
            Dim tableHtml As HtmlElement = HtmlDocument.LoadDocument(text, strip:=True)(0)
            Dim id As String = tableHtml.id
            Dim rows As String() = text.GetRowsHTML
            Dim rowsData As String()() = rows _
                .Select(Function(r) r.GetColumnsHTML) _
                .ToArray

            If rowsData.Length = 0 Then
                Continue For
            End If

            Dim cols As Integer = rowsData _
                .Select(Function(a) a.Length) _
                .GroupBy(Function(a) a) _
                .OrderByDescending(Function(a) a.Count) _
                .First.Key
            ' 20251113 filterout the colspan rows
            ' this may missing some data rows
            Dim matrix As String()() = rowsData _
                .Where(Function(r) r.Length = cols) _
                .MatrixTranspose(safecheck_dimension:=True) _
                .ToArray
            Dim table As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }

            For Each column As String() In matrix
                ' 20210422 请注意，下面的两个空格符号是不一样的
                Dim name As String = column(Scan0).getPlainText
                Dim data As String() = column _
                    .Skip(1) _
                    .Select(Function(si)
                                If plain_text Then
                                    Return getPlainText(si, strip_inner:=del_newline)
                                ElseIf del_newline Then
                                    Return si.TrimNewLine.TrimWhitespace
                                Else
                                    Return si.TrimWhitespace
                                End If
                            End Function) _
                    .ToArray

                table.columns(name) = data
            Next

            If filter AndAlso table.nrows = 0 Then
                Continue For
            Else
                id = If(id.StringEmpty(, True), App.NextTempName, id)
            End If

            Call result.unique_add(id, table)
        Next

        Return result
    End Function

    ''' <summary>
    ''' Parsing the title text from the html inputs.
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    <ExportAPI("title")>
    Public Function title(html As String) As String
        Return html.HTMLTitle
    End Function

    ''' <summary>
    ''' parse one line url from the given html document string fragment
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    <ExportAPI("link")>
    Public Function links(html As String) As String
        Return html.href
    End Function

    ''' <summary>
    ''' get all internal document text from the given html document text. 
    ''' </summary>
    ''' <param name="html"></param>
    ''' <returns></returns>
    <ExportAPI("plainText")>
    <Extension>
    Public Function getPlainText(html As String, Optional strip_inner As Boolean = True) As String
        Dim text As String = html _
            .StripHTMLTags _
            .UnescapeHTML _
            .TrimWhitespace

        If strip_inner Then
            text = text.TrimNewLine.TrimWhitespace
        End If

        Return text
    End Function

    <Extension>
    Private Function TrimWhitespace(s As String) As String
        Return s.TrimNull.Trim(" "c, ASCII.CR, ASCII.LF, ASCII.TAB, " "c, ASCII.NUL)
    End Function
End Module

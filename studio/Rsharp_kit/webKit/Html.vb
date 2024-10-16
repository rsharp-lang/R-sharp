﻿#Region "Microsoft.VisualBasic::575bafd51a2066bcca2941d210bf6c67, studio\Rsharp_kit\webKit\Html.vb"

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

    '   Total Lines: 128
    '    Code Lines: 88 (68.75%)
    ' Comment Lines: 20 (15.62%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 20 (15.62%)
    '     File Size: 4.51 KB


    ' Module Html
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: documentDebugView, getPlainText, links, parse, QueryHtmlTables
    '               title
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports Microsoft.VisualBasic.Text.Xml
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' Html document tools
''' </summary>
<Package("Html", Category:=APICategories.UtilityTools)>
Module Html

    Sub New()
        Call REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of HtmlElement)(AddressOf documentDebugView)
        Call REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of HtmlDocument)(AddressOf documentDebugView)
    End Sub

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
    ''' query a list of html tables in the given html page text document
    ''' </summary>
    ''' <param name="html">text string in html format</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("tables")>
    Public Function QueryHtmlTables(html As String, Optional del_newline As Boolean = True) As list
        Dim tables As String() = html.GetTablesHTML
        Dim result As New list(RType.GetRSharpType(GetType(dataframe))) With {
            .slots = New Dictionary(Of String, Object)
        }

        For Each text As String In tables
            Dim rows = text.GetRowsHTML
            Dim matrix = rows _
                .Select(Function(r) r.GetColumnsHTML) _
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
                    .Select(Function(si) getPlainText(si, strip_inner:=del_newline)) _
                    .ToArray

                table.columns(name) = data
            Next

            Call result.add(App.NextTempName, table)
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
            .TrimNull _
            .Trim(" "c, ASCII.CR, ASCII.LF, ASCII.TAB, " "c, ASCII.NUL)

        If strip_inner Then
            text = text.TrimNewLine.StringReplace("\s+", " ")
        End If

        Return text
    End Function
End Module

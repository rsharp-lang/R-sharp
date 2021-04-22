#Region "Microsoft.VisualBasic::14f4efbd5b99a4b4bcccb6811df69bf3, studio\Rsharp_kit\webKit\Html.vb"

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

    ' Module Html
    ' 
    '     Function: QueryHtmlTables
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports Microsoft.VisualBasic.Text.Xml
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' Html document tools
''' </summary>
<Package("Html", Category:=APICategories.UtilityTools)>
Module Html

    ''' <summary>
    ''' query a list of html tables in the given html page text document
    ''' </summary>
    ''' <param name="html">text string in html format</param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("tables")>
    Public Function QueryHtmlTables(html As String) As list
        Dim tables As String() = html.GetTablesHTML
        Dim result As New list(RType.GetRSharpType(GetType(dataframe))) With {
            .slots = New Dictionary(Of String, Object)
        }

        For Each text As String In tables
            Dim rows = text.GetRowsHTML
            Dim matrix = rows.Select(Function(r) r.GetColumnsHTML).MatrixTranspose.ToArray
            Dim table As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }

            For Each column As String() In matrix
                ' 20210422 请注意，下面的两个空格符号是不一样的
                Dim name As String = column(Scan0) _
                    .StripHTMLTags _
                    .UnescapeHTML _
                    .Trim(" "c, ASCII.CR, ASCII.LF, ASCII.TAB, " "c, ASCII.NUL)
                Dim data As String() = column _
                    .Skip(1) _
                    .Select(Function(cell)
                                Return cell _
                                    .StripHTMLTags _
                                    .UnescapeHTML.TrimNull _
                                    .Trim(" "c, ASCII.CR, ASCII.LF, ASCII.TAB, " "c, ASCII.NUL)
                            End Function) _
                    .ToArray

                table.columns(name) = data
            Next

            result.add(App.NextTempName, table)
        Next

        Return result
    End Function

End Module


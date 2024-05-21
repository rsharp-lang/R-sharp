#Region "Microsoft.VisualBasic::77df31e27a3bbee384d6d665db44f37c, R#\System\Document\Printer\HtmlMarkdownPrinter.vb"

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

    '   Total Lines: 77
    '    Code Lines: 59 (76.62%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (23.38%)
    '     File Size: 2.98 KB


    '     Module HtmlMarkdownPrinter
    ' 
    '         Sub: printHtml, printMarkdownDocs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Development

    Module HtmlMarkdownPrinter

        Public Sub printHtml(api As RMethodInfo, docs As ProjectMember, markdown As RContentOutput)
            If Not docs Is Nothing Then
                Call printMarkdownDocs(docs, markdown)
            End If

            Call markdown.WriteLine("```")
            Call markdown.WriteLine(api.GetPrintContent.Replace("`", ""))
            Call markdown.WriteLine("```")

            Dim enums = api.parameters _
                .Where(Function(par) par.type.raw.IsEnum) _
                .Select(Function(par) par.type.raw) _
                .GroupBy(Function(type) type.FullName) _
                .ToArray

            If enums.Length > 0 Then
                Call markdown.WriteLine(" where have enum values:")
                Call markdown.WriteLine("```")

                For Each [enum] As REnum In enums.Select(Function(tg) REnum.GetEnumList(tg.First))
                    Call markdown.WriteLine()
                    Call markdown.WriteLine($"{New String(" "c, 2)}let {[enum].name} as integer = {{")

                    For Each value As Object In [enum].values
                        Call markdown.WriteLine($"{New String(" "c, 6)}{value.ToString} = {[enum].IntValue(value)};")
                    Next

                    Call markdown.WriteLine($"{New String(" "c, 2)}}}")
                Next

                Call markdown.WriteLine("```")
            End If
        End Sub

        Private Sub printMarkdownDocs(docs As ProjectMember, markdown As RContentOutput)
            Dim summaryLines = docs.Summary.Trim(" "c, ASCII.CR, ASCII.LF).LineTokens
            Dim first = summaryLines.First

            If Not first.StartsWith("#") Then
                first = "#" & first
            End If

            Call markdown.WriteLine(first)
            Call markdown.WriteLine(summaryLines.Skip(1).JoinBy(vbCrLf))
            Call markdown.WriteLine()

            Call markdown.WriteLine("## arguments")

            For Each param As param In docs.Params
                Call markdown.WriteLine($"``{param.name}:``  " & param.text.Trim(" "c, ASCII.CR, ASCII.LF))
            Next

            Call markdown.WriteLine()

            If Not docs.Returns.StringEmpty Then
                Call markdown.WriteLine("## value")
                Call markdown.WriteLine(docs.Returns)
                Call markdown.WriteLine()
            End If

            If Not docs.Remarks.StringEmpty Then
                Call markdown.WriteLine("## details")
                Call markdown.WriteLine(docs.Remarks)
            End If
        End Sub
    End Module
End Namespace

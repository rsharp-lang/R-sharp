#Region "Microsoft.VisualBasic::bfcd8abb6eecfb2186cf6d2b801d06b4, R#\System\Document\ConsoleMarkdownPrinter.vb"

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

    '     Module ConsoleMarkdownPrinter
    ' 
    '         Sub: printConsole, printDocs, printFuncBody
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Development

    Module ConsoleMarkdownPrinter

        ReadOnly markdown As MarkdownRender = MarkdownRender.DefaultStyleRender

        Public Sub printConsole(api As RMethodInfo, docs As ProjectMember)
            If Not docs Is Nothing Then
                Call docs.DoCall(AddressOf printDocs)
            Else
                Call Console.WriteLine()
            End If

            Call api.DoCall(AddressOf printFuncBody)

            Dim enums = api.parameters _
                .Where(Function(par) par.type.raw.IsEnum) _
                .Select(Function(par) par.type.raw) _
                .GroupBy(Function(type) type.FullName) _
                .ToArray

            If enums.Length > 0 Then
                Call Console.WriteLine(" where have enum values:")

                For Each [enum] As REnum In enums.Select(Function(tg) REnum.GetEnumList(tg.First))
                    Call Console.WriteLine()
                    Call Console.WriteLine($"{New String(" "c, 2)}let {[enum].name} as integer = {{")

                    For Each value As Object In [enum].values
                        Call Console.WriteLine($"{New String(" "c, 6)}{value.ToString} = {[enum].IntValue(value)};")
                    Next

                    Call Console.WriteLine($"{New String(" "c, 2)}}}")
                Next
            End If
        End Sub

        Private Sub printFuncBody(api As RMethodInfo)
            Dim contentLines As List(Of String) = api _
                .GetPrintContent _
                .LineTokens _
                .AsList
            Dim offset% = 1
            Dim indent%

            Call markdown.DoPrint(contentLines(Scan0), 2)

            If api.parameters.Length > 3 Then
                indent = 19 + api.name.Length
                offset = api.parameters.Length

                For Each line As String In contentLines.Skip(1).Take(offset - 1)
                    Call markdown.DoPrint(line, indent)
                Next
            End If

            For Each line As String In contentLines.Skip(offset).Take(7)
                Call markdown.DoPrint("# " & line.Trim, 6)
            Next

            Call Console.WriteLine()

            Call markdown.DoPrint(contentLines(-2).Trim, 6)
            Call markdown.DoPrint(contentLines(-1).Trim, 2)

            Call Console.WriteLine()
        End Sub

        Private Sub printDocs(docs As ProjectMember)
            Call markdown.DoPrint(docs.Summary, 1)
            Call Console.WriteLine()

            For Each param As param In docs.Params
                Call markdown.DoPrint($"``{param.name}:``  " & param.text.Trim(" "c, ASCII.CR, ASCII.LF), 3)
            Next

            Call Console.WriteLine()

            If Not docs.Returns.StringEmpty Then
                Call markdown.DoPrint(" [**returns**]: ", 0)
                Call markdown.DoPrint(docs.Returns, 1)
                Call Console.WriteLine()
            End If
        End Sub
    End Module
End Namespace

#Region "Microsoft.VisualBasic::44014e3e51384ba739da6e1270dbbcc9, R#\System\Document\Printer\ConsoleMarkdownPrinter.vb"

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

    '   Total Lines: 206
    '    Code Lines: 149 (72.33%)
    ' Comment Lines: 13 (6.31%)
    '    - Xml Docs: 69.23%
    ' 
    '   Blank Lines: 44 (21.36%)
    '     File Size: 7.55 KB


    '     Module ConsoleMarkdownPrinter
    ' 
    '         Function: getMarkdownDocs, TryGetHelpDocument
    ' 
    '         Sub: printConsole, (+2 Overloads) printDocs, printFuncBody, PrintText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Interop
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Namespace Development

    Public Module ConsoleMarkdownPrinter

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
            Dim indent_space As New String(" "c, 6)

            If enums.Length > 0 Then
                Call Console.WriteLine(" where have enum values:")

                For Each [enum] As REnum In enums.Select(Function(tg) REnum.GetEnumList(tg.First))
                    Call Console.WriteLine()
                    Call Console.WriteLine($"{New String(" "c, 2)}let {[enum].name} as integer = {{")

                    For Each value As Object In [enum].values
                        Dim desc As String = DirectCast(value, [Enum]).Description
                        Dim toStr As String = value.ToString

                        Call Console.WriteLine($"{indent_space}{If(desc.StringEmpty, "", $"[{desc}] ")}{toStr} = {[enum].IntValue(value)};")
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

        <Extension>
        Public Function TryGetHelpDocument(f As SymbolExpression) As Document
            Dim doc_json As String = f.GetAttributeValue(PackageLoader2.RsharpHelp).DefaultFirst
            Dim help As Document = doc_json.LoadJSON(Of Document)(throwEx:=False)

            Return help
        End Function

        ''' <summary>
        ''' print highlighted markdown on console
        ''' </summary>
        ''' <param name="f"></param>
        Public Sub printDocs(f As SymbolExpression)
            Dim help = f.TryGetHelpDocument

            ' skip print if contains no help document
            ' of current symbol
            If help Is Nothing Then
                Return
            End If

            Call markdown.DoPrint(If(help.title, ""), 1)
            Call Console.WriteLine()
            Call markdown.DoPrint(If(help.description, ""), 1)
            Call Console.WriteLine()
            Call markdown.DoPrint(If(help.details, ""), 2)
            Call Console.WriteLine()

            For Each param As NamedValue In help.parameters.SafeQuery
                Call markdown.DoPrint($"``{param.name}``: " & If(param.text, "").Trim(" "c, ASCII.CR, ASCII.LF), 3)
            Next

            Call Console.WriteLine()

            If Not help.returns.StringEmpty Then
                Call markdown.DoPrint("**returns**: ", 1)
                Call markdown.DoPrint(help.returns, 1)
                Call Console.WriteLine()
            End If
        End Sub

        ''' <summary>
        ''' get help document content in markdown format
        ''' </summary>
        ''' <param name="f"></param>
        ''' <returns></returns>
        Public Function getMarkdownDocs(f As SymbolExpression) As String
            Dim s As New StringBuilder
            Dim help As Document = f.TryGetHelpDocument

            ' skip print if contains no help document
            ' of current symbol
            If help Is Nothing Then
                Return ""
            End If

            Call s.AppendLine(If(help.title, ""))
            Call s.AppendLine()
            Call s.AppendLine(If(help.description, ""))
            Call s.AppendLine()
            Call s.AppendLine(If(help.details, ""))
            Call s.AppendLine()

            Call s.AppendLine("|name|description|")
            Call s.AppendLine("|----|-----------|")

            For Each param As NamedValue In help.parameters.SafeQuery
                Call s.AppendLine($"|{param.name}|" & If(param.text, "").Trim(" "c, ASCII.CR, ASCII.LF) & "|")
            Next

            Call s.AppendLine()

            If Not help.returns.StringEmpty Then
                Call s.AppendLine("**returns** ")
                Call s.AppendLine(help.returns)
                Call s.AppendLine()
            End If

            Return s.ToString
        End Function

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

        <Extension>
        Public Sub PrintText(docs As ProjectMember, text As TextWriter)
            Call text.WriteLine(docs.Summary)
            Call text.WriteLine()

            For Each param As param In docs.Params
                Call text.WriteLine($"``{param.name}:``  " & param.text.Trim(" "c, ASCII.CR, ASCII.LF), 3)
            Next

            Call text.WriteLine()

            If Not docs.Returns.StringEmpty Then
                Call text.WriteLine(" [**returns**]: ")
                Call text.WriteLine(docs.Returns)
                Call text.WriteLine()
            End If

            Call text.Flush()
        End Sub
    End Module
End Namespace

Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace System

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
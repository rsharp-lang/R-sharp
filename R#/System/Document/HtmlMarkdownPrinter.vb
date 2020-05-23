Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace System

    Module HtmlMarkdownPrinter

        Public Sub printHtml(api As RMethodInfo, docs As ProjectMember, markdown As RContentOutput)
            If Not docs Is Nothing Then
                Call markdown.WriteLine(docs.Summary)
                Call markdown.WriteLine()

                For Each param As param In docs.Params
                    Call markdown.WriteLine($"``{param.name}:``  " & param.text.Trim(" "c, ASCII.CR, ASCII.LF))
                Next

                Call markdown.WriteLine()

                If Not docs.Returns.StringEmpty Then
                    Call markdown.WriteLine(" [**returns**]: ")
                    Call markdown.WriteLine(docs.Returns)
                    Call markdown.WriteLine()
                End If
            End If

            Call markdown.WriteLine(api.GetPrintContent)

            Dim enums = api.parameters _
                .Where(Function(par) par.type.raw.IsEnum) _
                .Select(Function(par) par.type.raw) _
                .GroupBy(Function(type) type.FullName) _
                .ToArray

            If enums.Length > 0 Then
                Call markdown.WriteLine(" where have enum values:")

                For Each [enum] As REnum In enums.Select(Function(tg) REnum.GetEnumList(tg.First))
                    Call markdown.WriteLine()
                    Call markdown.WriteLine($"{New String(" "c, 2)}let {[enum].name} as integer = {{")

                    For Each value As Object In [enum].values
                        Call markdown.WriteLine($"{New String(" "c, 6)}{value.ToString} = {[enum].IntValue(value)};")
                    Next

                    Call markdown.WriteLine($"{New String(" "c, 2)}}}")
                Next
            End If
        End Sub
    End Module
End Namespace
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace System

    Module UnixManPagePrinter

        Public Sub printManPage(api As RMethodInfo, docs As ProjectMember, markdown As RContentOutput)
            Dim man As UnixManPage = api.CreateManPage(docs)

            Call markdown.Write(UnixManPage.ToString(man, ""))
            Call markdown.Flush()
        End Sub

        <Extension>
        Public Function CreateManPage(api As RMethodInfo, docs As ProjectMember) As UnixManPage
            Dim targetModule As Type = api.GetRawDeclares.DeclaringType
            Dim package As PackageAttribute = targetModule.GetCustomAttribute(Of PackageAttribute)
            Dim info = api.GetRawDeclares.DeclaringType.Assembly.FromAssembly

            If package Is Nothing Then
                package = New PackageAttribute(targetModule.NamespaceEntry)
            End If

            Dim man As New UnixManPage With {
                .AUTHOR = package.Publisher,
                .BUGS = "",
                .COPYRIGHT = info.AssemblyCopyright,
                .DESCRIPTION = docs.Summary _
                    .DoCall(AddressOf Strings.Trim) _
                    .Trim(" "c, "#"c, "-"c) _
                    .stylingMarkdownElements,
                .index = New Index With {
                    .category = package.Category,
                    .index = package.Namespace,
                    .[date] = info.BuiltTime,
                    .keyword = api.name,
                    .title = api.name
                },
                .DETAILS = docs.Remarks _
                    .stylingMarkdownElements,
                .LICENSE = "",
                .NAME = api.name,
                .SEE_ALSO = package.Namespace,
                .FILES = targetModule.Assembly.Location.FileName,
                .SYNOPSIS = $"{api.name}({api.parameters.JoinBy(", ").stylingMarkdownElements});",
                .PROLOG = docs.Summary _
                    .LineTokens _
                    .FirstOrDefault _
                    .DoCall(AddressOf Strings.Trim) _
                    .Trim(" "c, "#"c, "-"c) _
                    .stylingMarkdownElements,
                .OPTIONS = docs.Params _
                    .SafeQuery _
                    .Select(Function(a)
                                Return New NamedValue(Of String)(a.name, a.text.stylingMarkdownElements)
                            End Function) _
                    .ToArray
            }

            Return man
        End Function

        <Extension>
        Private Function stylingMarkdownElements(text As String) As String
            Dim sb As New StringBuilder(Strings.Trim(text))
            Dim codes = text.Matches("[`]{2}.+?[`]{2}")

            For Each code As String In codes
                Call sb.Replace(code, $"\fB{code.Trim("`"c)}\fR")
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace
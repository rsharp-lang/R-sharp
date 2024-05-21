#Region "Microsoft.VisualBasic::8a347cbfac8019f2e60337ddd5f83340, R#\System\Document\Printer\UnixManPagePrinter.vb"

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

    '   Total Lines: 89
    '    Code Lines: 78 (87.64%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (12.36%)
    '     File Size: 3.70 KB


    '     Module UnixManPagePrinter
    ' 
    '         Function: CreateManPage, stylingMarkdownElements
    ' 
    '         Sub: printManPage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

Namespace Development

    Public Module UnixManPagePrinter

        Public Sub printManPage(api As RMethodInfo, docs As ProjectMember, markdown As RContentOutput)
            Dim man As UnixManPage = api.CreateManPage(docs)

            Call markdown.Write(UnixManPage.ToString(man, ""))
            Call markdown.Flush()
        End Sub

        <Extension>
        Public Function CreateManPage(api As RMethodInfo, docs As ProjectMember) As UnixManPage
            Dim targetModule As Type = api.GetNetCoreCLRDeclaration.DeclaringType
            Dim package As PackageAttribute = targetModule.GetCustomAttribute(Of PackageAttribute)
            Dim info = api.GetNetCoreCLRDeclaration.DeclaringType.Assembly.FromAssembly

            If package Is Nothing Then
                package = New PackageAttribute(targetModule.NamespaceEntry)
            End If
            If docs Is Nothing Then
                docs = New ProjectMember
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
                .SYNOPSIS = $"{api.name}({api.parameters.Select(Function(a) a.ToString.stylingMarkdownElements).JoinBy(", " & vbCrLf)});",
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
                    .ToArray,
                .VALUE = docs.Returns.stylingMarkdownElements
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

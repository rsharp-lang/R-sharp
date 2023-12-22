#Region "Microsoft.VisualBasic::086620b7522d450ede3f2a86c68f4233, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//rdocumentation/function.vb"

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

'   Total Lines: 353
'    Code Lines: 301
' Comment Lines: 1
'   Blank Lines: 51
'     File Size: 16.67 KB


' Class [function]
' 
'     Function: (+2 Overloads) argument, blankTemplate, (+3 Overloads) createHtml, typeLink
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports RPackage = SMRUCC.Rsharp.Development.Package.Package

Public Class [function]

    ReadOnly markdown As New MarkdownHTML

    Public Function createHtml(api As RFunction, env As Environment) As String
        If TypeOf api Is RMethodInfo Then
            Return createHtml(DirectCast(api, RMethodInfo), env)
        Else
            Throw New NotImplementedException(api.GetType.FullName)
        End If
    End Function

    Public Function createHtml(api As RMethodInfo, template As String, env As Environment) As String
        Dim xml As ProjectMember = env.globalEnvironment _
            .packages _
            .packageDocs _
            .GetAnnotations(api.GetNetCoreCLRDeclaration)
        Dim func As New FunctionDeclare With {
            .name = api.name,
            .parameters = api.parameters _
                .Where(Function(p)
                           Return Not p.type.raw Is GetType(Environment)
                       End Function) _
                .Select(AddressOf argument) _
                .ToArray
        }
        Dim pkg As RPackage = api.GetPackageInfo
        Dim docs As New Document With {
            .declares = func,
            .title = func.name,
            .description = func.name,
            .parameters = {},
            .returns = "",
            .details = ""
        }

        If Not xml Is Nothing Then
            docs.title = Strings.Trim(xml.Summary.LineIterators.FirstOrDefault).Trim(" "c, "#"c)
            docs.description = xml.Summary _
                .LineIterators _
                .Skip(1) _
                .JoinBy("<br />") _
                .DoCall(AddressOf markdown.Transform)
            docs.parameters = xml.Params _
                .Select(AddressOf argument) _
                .ToArray
            docs.returns = markdown.Transform(xml.Returns)
            docs.details = markdown.Transform(xml.Remarks)
            docs.examples = " " & xml.example
        End If

        If docs.returns.StringEmpty Then
            ' generate document automatically based on the return type
            Dim types As Type() = api.GetUnionTypes.ToArray

            If types.Length = 1 Then
                docs.returns = $"this function returns data object of type {typeLink(types(Scan0))}."
            ElseIf types.Length > 1 Then
                docs.returns = $"this function returns data object in these one of the listed data types: {types.Select(AddressOf typeLink).JoinBy(", ")}."
            End If
        End If

        Dim rtvl = api.GetRApiReturns

        If Not rtvl Is Nothing AndAlso rtvl.isClassGraph Then
            docs.returns = docs.returns &
                " the list data also has some specificied data fields: <code>list(" &
                rtvl.fields.JoinBy(", ") & ")</code>."
        End If

        Return createHtml(docs, template, pkg)
    End Function

    Private Shared Function typeLink(type As Type) As String
        Dim rtype As RType = RType.GetRSharpType(type)

        Select Case rtype.mode
            Case TypeCodes.boolean,
                 TypeCodes.double,
                 TypeCodes.integer,
                 TypeCodes.list,
                 TypeCodes.NA,
                 TypeCodes.string

                Return rtype.mode.Description
            Case Else

                If type Is GetType(Object) Then
                    Return "<i>any</i> kind"
                Else
                    Return $"<a href=""/clr/{type.FullName.Replace("."c, "/"c)}.html"">{type.Name}</a>"
                End If
        End Select
    End Function

    Private Function argument(arg As param) As NamedValue
        Return New NamedValue With {
            .name = arg.name,
            .text = markdown.Transform(arg.text)
        }
    End Function

    Private Function argument(arg As RMethodArgument) As NamedValue
        Dim argName As String
        Dim argText As String

        If arg.isObjectList Then
            argName = "..."
        Else
            argName = arg.name
        End If

        If arg.isOptional Then
            If TypeOf arg.default Is Boolean Then
                argText = arg.default.ToString.ToUpper
            ElseIf TypeOf arg.default Is String OrElse TypeOf arg.default Is Char Then
                argText = $"""{arg.default}"""
            ElseIf arg.default Is Nothing Then
                argText = "NULL"
            Else
                argText = any.ToString(arg.default)
            End If
        Else
            argText = Nothing
        End If

        Return New NamedValue With {
            .name = argName.Replace("_", "."),
            .text = argText
        }
    End Function

    Public Function createHtml(docs As Document, template As String, pkg As RPackage) As String
        Dim assembly As AssemblyInfo = pkg.package.Assembly.FromAssembly

        If template.StringEmpty Then
            template = blankTemplate.ToString
        End If

        With New ScriptBuilder(template)
            !name_title = docs.declares.name
            !usage = docs.declares _
                .ToString(html:=True) _
                .Trim(" "c, ASCII.CR, ASCII.LF)
            !title = docs.title
            !summary = docs.description
            !arguments = docs.parameters _
                .Select(Function(arg)
                            Return $"
<dt>{arg.name.Replace("_", ".")}</dt>
<dd><p>{arg.text}</p></dd>
"
                        End Function) _
                .JoinBy(vbCrLf)
            !value = docs.returns Or "This function has no value returns.".AsDefault
            !details = docs.details
            !package = pkg.namespace
            !version = assembly.AssemblyVersion
            !copyright = assembly.AssemblyCopyright
            !show_details = If(docs.details.StringEmpty, "none", "block")

            If Strings.Trim(docs.examples).StringEmpty Then
                !show_examples = "none"
                !examples = ""
            Else
                !show_examples = "block"
                !examples = $"<pre><code id=""example_r"">{docs.examples}</code></pre>"
            End If

            If docs.keywords.IsNullOrEmpty Then
                !display_keywords = "none"
            End If

            If docs.author.IsNullOrEmpty Then
                !author = assembly.AssemblyCompany.Replace("<", "&lt;")
            Else
                !author = docs.author.JoinBy(", ")
            End If

            Return .ToString
        End With
    End Function

End Class

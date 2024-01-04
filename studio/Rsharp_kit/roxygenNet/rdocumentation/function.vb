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
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports RPackage = SMRUCC.Rsharp.Development.Package.Package

Public Class [function]

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

        Dim unions_type As Type() = api.GetUnionTypes.ToArray

        If docs.returns.StringEmpty Then
            ' generate document automatically based on the return type
            If unions_type.Length = 1 Then
                docs.returns = $"this function returns data object of type { clr_xml.typeLink(unions_type(Scan0))}."
            ElseIf unions_type.Length > 1 Then
                docs.returns = $"this function returns data object in these one of the listed data types: {unions_type.Select(AddressOf clr_xml.typeLink).JoinBy(", ")}."
            End If
        Else
            docs.returns = clr_xml.HandlingTypeReferenceInDocs(docs.returns)
        End If

        docs.description = clr_xml.HandlingTypeReferenceInDocs(docs.description)
        docs.details = clr_xml.HandlingTypeReferenceInDocs(docs.details)

        Dim rtvl = api.GetRApiReturns

        If Not rtvl Is Nothing AndAlso rtvl.isClassGraph Then
            docs.returns = docs.returns &
                " the list data also has some specificied data fields: <code>list(" &
                rtvl.fields.JoinBy(", ") & ")</code>."
        End If

        If Not unions_type.IsNullOrEmpty Then
            docs.returns = docs.returns & "<h4>clr value class</h4>"
            docs.returns = docs.returns & "<ul>"

            For Each type As Type In unions_type
                clr_xml.push_clr(type)
                docs.returns = docs.returns & $"<li>{clr_xml.typeLink(type)}</li>"
            Next

            docs.returns = docs.returns & "</ul>"
        End If

        Return createHtml(docs, template, pkg)
    End Function

    Private Function argument(arg As param) As NamedValue
        Return New NamedValue With {
            .name = arg.name,
            .text = clr_xml.HandlingTypeReferenceInDocs(markdown.Transform(arg.text))
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

    Private Function createHtml(docs As Document, template As String, pkg As RPackage) As String
        Dim assembly As AssemblyInfo = pkg.package.Assembly.FromAssembly
        Dim ns_str As String = pkg.namespace
        Dim default_author$ = assembly.AssemblyCompany
        Dim version As String = assembly.AssemblyVersion
        Dim copyright As String = assembly.AssemblyCopyright

        Return createHtml(docs, template, ns_str, version, default_author, copyright)
    End Function

    Public Shared Function createHtml(docs As Document, template$, namespace$, ver$, default_author$, copyright$) As String
        With New ScriptBuilder(If(template, function_template.blankTemplate.ToString))
            !name_title = docs.declares.name
            !usage = docs.declares _
                .ToString(html:=True) _
                .Trim(" "c, ASCII.CR, ASCII.LF)
            !title = docs.title
            !summary = docs.description
            !arguments = docs.parameters _
                .SafeQuery _
                .Select(Function(arg)
                            Return $"
<dt>{arg.name.Replace("_", ".")}</dt>
<dd><p>{arg.text}</p></dd>
"
                        End Function) _
                .JoinBy(vbCrLf)
            !value = docs.returns Or "This function has no value returns.".AsDefault
            !details = docs.details
            !package = namespace$
            !version = ver
            !copyright = copyright
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
                !author = Strings.Trim(default_author).Replace("<", "&lt;")
            Else
                !author = docs.author.JoinBy(", ")
            End If

            Return .ToString
        End With
    End Function

End Class

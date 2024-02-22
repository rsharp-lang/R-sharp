#Region "Microsoft.VisualBasic::72b39157b2f0b2b4c892af3b380a73af, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//rdocumentation.vb"

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

'   Total Lines: 67
'    Code Lines: 51
' Comment Lines: 8
'   Blank Lines: 8
'     File Size: 2.56 KB


' Module rdocumentation
' 
'     Function: getFunctions, getPkgApisList, rdocumentation
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.ConstrainedExecution
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

''' <summary>
''' Make documents for R# symbol object
''' </summary>
<Package("rdocumentation")>
Public Module rdocumentation

    ''' <summary>
    ''' make documentation for a R# function or symbol
    ''' </summary>
    ''' <param name="func">the specific R# function or symbol</param>
    ''' <param name="template">the html template string</param>
    ''' <param name="desc">the package <see cref="DESCRIPTION"/> metadata for the R# symbol if 
    ''' the given <paramref name="func"/> object is a kind of the R source code <see cref="Document"/> 
    ''' object.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("documentation")>
    <RApiReturn(TypeCodes.string)>
    Public Function rdocumentation(func As Object,
                                   Optional template As String = Nothing,
                                   Optional desc As DESCRIPTION = Nothing,
                                   Optional env As Environment = Nothing) As Object

        If TypeOf func Is RFunction Then
            Return New [function]().createHtml(DirectCast(func, RFunction), template, env)
        ElseIf TypeOf func Is Document Then
            Dim docs As Document = DirectCast(func, Document).MarkdownTransform
            Dim html As String = [function].createHtml(docs, template, desc.Package, desc.Version, desc.Author, desc.License)

            Return html
        Else
            Return Message.InCompatibleType(GetType(RFunction), func.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' pull all clr type in the cache and then clear up the cache
    ''' </summary>
    ''' <param name="generic_excludes"></param>
    ''' <returns></returns>
    <ExportAPI("pull_clr_types")>
    Public Function pull_clr_types(Optional generic_excludes As Boolean = False) As Type()
        Dim pull As Type()
        Dim outlist As New List(Of Type)

        Static visited As New Index(Of Type)

        If Not generic_excludes Then
            ' gets all
            pull = clr_xml.clr_types.PopAll
        Else
            pull = clr_xml.clr_types.PopAll _
                .Where(Function(t) t.GetGenericArguments.IsNullOrEmpty) _
                .ToArray
        End If

        ' break the circle reference dead loop
        For Each clr_type As Type In pull
            If Not clr_type Like visited Then
                Call visited.Add(clr_type)
                Call outlist.Add(clr_type)
            End If
        Next

        Return outlist.ToArray
    End Function

    ''' <summary>
    ''' get overloads functions
    ''' </summary>
    ''' <param name="pkg"></param>
    ''' <returns></returns>
    <ExportAPI("get_overloads")>
    Public Function getOverloads(pkg As Type) As Object
        Return New list With {
            .slots = RGenericOverloads _
                .GetOverloads(pkg) _
                .GroupBy(Function(f) f.name) _
                .ToDictionary(Function(f) f.Key,
                              Function(f)
                                  Return CObj(f.ToArray)
                              End Function)
        }
    End Function

    ''' <summary>
    ''' make documents based on a given clr <see cref="Type"/> meta data
    ''' </summary>
    ''' <param name="clr">a given clr type</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this function just works on the properties
    ''' </remarks>
    <ExportAPI("clr_docs")>
    Public Function clr_docs(clr As Type, template As String, Optional env As Environment = Nothing) As String
        Dim xml As ProjectType = env.globalEnvironment _
           .packages _
           .packageDocs _
           .GetAnnotations(clr)

        If xml Is Nothing Then
            xml = New ProjectType
        End If

        Dim html As New ScriptBuilder(template)
        Dim desc As String = xml.Summary
        Dim tree As New List(Of (title As String, Type))

        If desc.StringEmpty Then
            desc = xml.Remarks
        Else
            desc = {desc, If(xml.Remarks, "")}.JoinBy(vbCrLf & vbCrLf)
        End If

        desc = roxygen.markdown.Transform(desc)

        With html
            !title = clr.FullName
            !name_title = clr.Name
            !namespace = clr.Namespace
            !summary = clr_xml.HandlingTypeReferenceInDocs(desc)
            !declare = ts_code(clr, xml, tree:=tree)
        End With

        Dim tree_html As New StringBuilder

        If tree.Any Then
            tree_html.AppendLine("<ol>")

            For Each r As (title As String, type As Type) In tree
                Call clr_xml.push_clr(r.type)

                ' generates the clr reference tree list node
                tree_html.AppendLine($"<li>{r.title}: {clr_xml.typeLink(r.type, show_clr_array:=False)}</li>")
            Next

            tree_html.AppendLine("</ol>")
        Else
            tree_html.AppendLine("this clr type has no other .net clr type reference.")
        End If

        html!clr_tree = tree_html.ToString

        Return html.ToString
    End Function

    <Extension>
    Private Function hasBaseClass(type As Type) As Boolean
        If type Is Nothing Then
            Return False
        ElseIf type.BaseType Is Nothing Then
            Return False
        ElseIf type.BaseType.FullName = "System.Object" Then
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' generates the type definition code of the given .net clr <paramref name="type"/>
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="xml"></param>
    ''' <param name="tree"></param>
    ''' <returns></returns>
    Private Function ts_code(type As Type, xml As ProjectType, ByRef tree As List(Of (title As String, type As Type))) As String
        Dim ts As New StringBuilder
        Dim cls_name As String = type.Name
        Dim members As IEnumerable(Of MemberInfo)
        Dim is_enum As Boolean = type.IsEnum
        Dim docs As String = Nothing
        Dim extends As String = ""

        If type.hasBaseClass Then
            Call clr_xml.push_clr(type.BaseType)

            ' add code show the class extends tree
            extends = $"extends {clr_xml.typeLink(type.BaseType)} "
            tree.Add(($"this class extends from {clr_xml.typeLink(type.BaseType)} class", type.BaseType))
        End If

        Call ts.AppendLine()
        Call ts.AppendLine($"# namespace {type.Namespace}")
        Call ts.AppendLine($"export class {cls_name} {extends}{{")

        If is_enum Then
            members = type.GetFields _
                .Where(Function(f) f.FieldType Is type) _
                .Select(Function(f) DirectCast(f, MemberInfo)) _
                .ToArray
        Else
            members = type.GetProperties(PublicProperty) _
                .Where(Function(p)
                           Return p.CanRead AndAlso p.GetIndexParameters.IsNullOrEmpty
                       End Function) _
                .Select(Function(p) DirectCast(p, MemberInfo))
        End If

        For Each member As MemberInfo In members.OrderBy(Function(m) m.Name)
            If is_enum Then
                docs = xml.GetField(member.Name)?.Summary
            Else
                docs = xml.GetProperties(member.Name) _
                    .SafeQuery _
                    .Select(Function(i) i.Summary) _
                    .JoinBy(vbCrLf)
            End If

            docs = roxygen.markdown.Transform(docs)
            docs = clr_xml.HandlingTypeReferenceInDocs(docs)
            docs = docs _
                .Replace("<p>", "") _
                .Replace("</p>", "") _
                .Replace("<br />", "") _
                .Trim

            For Each line As String In docs.LineTokens
                Call ts.AppendLine($"   # {line}")
            Next

            If TypeOf member Is PropertyInfo Then
                type = DirectCast(member, PropertyInfo).PropertyType
            Else
                type = DirectCast(member, FieldInfo).FieldType
            End If

            If is_enum Then
                Dim desc As DescriptionAttribute = member.GetCustomAttribute(Of DescriptionAttribute)

                If Not desc Is Nothing Then
                    Call ts.AppendLine($"   [@desc ""{desc.Description}""]")
                End If

                Call ts.AppendLine($"   {member.Name}: {clr_xml.typeLink(type)} = {CULng(DirectCast(member, FieldInfo).GetValue(Nothing))};")
                Call ts.AppendLine()
            Else
                Call ts.AppendLine($"   {member.Name}: {clr_xml.typeLink(type)};")
            End If

            Dim rtype As RType = RType.GetRSharpType(type)

            If Not rtype.isPrimitive Then
                Call tree.Add(($"use by <i>{If(TypeOf member Is PropertyInfo, "property", "field")}</i> member <code>{member.Name}</code>", type))
            End If
        Next

        Call ts.AppendLine("}")

        Return ts.ToString
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="package">
    ''' the ``R#`` package module name or the module object itself.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>A tuple list of the R# functions that parsed
    ''' from the target clr package module.</returns>
    <ExportAPI("getFunctions")>
    Public Function getFunctions(package As Object, Optional env As Environment = Nothing) As Object
        Dim apis = getPkgApisList(package, env)

        If apis Like GetType(Message) Then
            Return apis.TryCast(Of Message)
        End If

        Dim funcs As New list With {.slots = New Dictionary(Of String, Object)}
        Dim func As RMethodInfo

        For Each f As NamedValue(Of MethodInfo) In apis.TryCast(Of NamedValue(Of MethodInfo)())
            func = New RMethodInfo(f)
            funcs.add(func.name, func)
        Next

        Return funcs
    End Function

    Friend Function getPkgApisList(package As Object, env As Environment) As [Variant](Of Message, NamedValue(Of MethodInfo)())
        If TypeOf package Is String Then
            Return env.globalEnvironment.packages _
                .FindPackage(any.ToString(package), Nothing) _
                .DoCall(AddressOf ImportsPackage.GetAllApi) _
                .ToArray
        ElseIf TypeOf package Is Development.Package.Package Then
            Return ImportsPackage _
                .GetAllApi(DirectCast(package, Development.Package.Package)) _
                .ToArray
        ElseIf TypeOf package Is Type Then
            Return ImportsPackage _
                .GetAllApi(Development.Package.Package.ParseClrType(DirectCast(package, Type))) _
                .ToArray
        Else
            Return Components _
                .Message _
                .InCompatibleType(GetType(String), package.GetType, env)
        End If
    End Function

    Friend Function getPkg(package As Object, env As Environment) As [Variant](Of Message, Development.Package.Package)
        If TypeOf package Is String Then
            Return env.globalEnvironment.packages _
                .FindPackage(any.ToString(package), Nothing)
        ElseIf TypeOf package Is Development.Package.Package Then
            Return DirectCast(package, Development.Package.Package)
        ElseIf TypeOf package Is Type Then
            Return Development.Package.Package.ParseClrType(DirectCast(package, Type))
        Else
            Return Components _
                .Message _
                .InCompatibleType(GetType(String), package.GetType, env)
        End If
    End Function
End Module

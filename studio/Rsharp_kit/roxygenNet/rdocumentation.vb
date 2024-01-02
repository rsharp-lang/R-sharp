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

Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
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
''' 
''' </summary>
<Package("rdocumentation")>
Public Module rdocumentation

    <ExportAPI("documentation")>
    <RApiReturn(TypeCodes.string)>
    Public Function rdocumentation(func As Object,
                                   Optional template As String = Nothing,
                                   Optional desc As DESCRIPTION = Nothing,
                                   Optional env As Environment = Nothing) As Object

        If TypeOf func Is RFunction Then
            Return New [function]().createHtml(DirectCast(func, RFunction), template, env)
        ElseIf TypeOf func Is Document Then
            Dim docs As Document = DirectCast(func, Document)
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
        If Not generic_excludes Then
            ' gets all
            Return [function].clr_types.PopAll
        Else
            Return [function].clr_types.PopAll _
                .Where(Function(t) t.GetGenericArguments.IsNullOrEmpty) _
                .ToArray
        End If
    End Function

    ''' <summary>
    ''' make documents based on a given clr type meta data
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

        Dim html As New StringBuilder(template)
        Dim desc As String = xml.Summary

        If desc.StringEmpty Then
            desc = xml.Remarks
        Else
            desc = {desc, If(xml.Remarks, "")}.JoinBy(vbCrLf & vbCrLf)
        End If

        desc = [function].markdown.Transform(desc)

        Call html.Replace("{$title}", clr.FullName)
        Call html.Replace("{$name_title}", clr.Name)
        Call html.Replace("{$namespace}", clr.Namespace)
        Call html.Replace("{$summary}", [function].HandlingTypeReferenceInDocs(desc))
        Call html.Replace("{$declare}", ts_code(clr, xml))

        Return html.ToString
    End Function

    Private Function ts_code(type As Type, xml As ProjectType) As String
        Dim ts As New StringBuilder

        Call ts.AppendLine()
        Call ts.AppendLine($"# namespace {type.Namespace}")
        Call ts.AppendLine($"export class {type.Name} {{")

        For Each member As PropertyInfo In type.GetProperties(PublicProperty)
            If Not member.CanRead Then
                Continue For
            ElseIf Not member.GetIndexParameters.IsNullOrEmpty Then
                Continue For
            End If

            Dim docs As String = xml.GetProperties(member.Name) _
                .SafeQuery _
                .Select(Function(i) i.Summary) _
                .JoinBy(vbCrLf)

            docs = [function].markdown.Transform(docs)
            docs = [function].HandlingTypeReferenceInDocs(docs)

            For Each line As String In docs.LineTokens
                Call ts.AppendLine($"   # {line}")
            Next

            Call ts.AppendLine($"   {member.Name}: {[function].typeLink(member.PropertyType)};")
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
    ''' <returns></returns>
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
        Else
            Return Components _
                .Message _
                .InCompatibleType(GetType(String), package.GetType, env)
        End If
    End Function
End Module

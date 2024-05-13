#Region "Microsoft.VisualBasic::68824c0fe624aa44595d23d574dd6093, studio\Rsharp_kit\roxygenNet\rdocumentation\clr_xml.vb"

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

    '   Total Lines: 253
    '    Code Lines: 200
    ' Comment Lines: 8
    '   Blank Lines: 45
    '     File Size: 8.70 KB


    ' Class clr_xml
    ' 
    '     Function: HandlingTypeReferenceInDocs, isGeneralCollection, isGeneralTuple, ParsePropertyReference, ParseTypeReference
    '               typeLink
    ' 
    '     Sub: push_clr
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Public Class clr_xml

    Friend Shared ReadOnly clr_types As New List(Of Type)

    Shared ReadOnly regexp_clrtags As New Regex("[@][<]code[>].*?[<]/code[>]", RegexICSng)

    Public Shared Iterator Function ParseTypeReference(doc_str As String) As IEnumerable(Of (str As String, Type))
        Dim list = regexp_clrtags.Matches(doc_str).ToArray
        Dim type As Type
        Dim ref As NamedValue(Of String)

        For Each link As String In list
            ref = link.GetValue.GetTagValue(":", trim:=True)

            If ref.Name <> "T" Then
                Continue For
            End If

            type = AssemblyInfo.GetType(ref.Value)

            If Not type Is Nothing Then
                push_clr(type)
                Yield (link, type)
            End If
        Next
    End Function

    Public Shared Iterator Function ParsePropertyReference(doc_str As String) As IEnumerable(Of (str As String, html_replace As String))
        Dim list = regexp_clrtags.Matches(doc_str).ToArray
        Dim type As Type
        Dim ref As NamedValue(Of String)
        Dim clr_type As String()
        Dim html As String
        Dim pname As String
        Dim [property] As PropertyInfo
        Dim field As FieldInfo
        Dim method As MethodInfo
        Dim ptype As Type

        For Each link As String In list
            ref = link.GetValue.GetTagValue(":", trim:=True)

            If ref.Name <> "P" AndAlso ref.Name <> "F" AndAlso ref.Name <> "M" Then
                Continue For
            End If

            clr_type = ref.Value.Split("."c)
            type = AssemblyInfo.GetType(clr_type.Take(clr_type.Length - 1).JoinBy("."))
            pname = clr_type.Last

            If Not type Is Nothing Then
                ' 20240304 xml documentation mis-matched with clr assembly
                ' will cause the property/field/method is nothing
                ' skip of such situation
                Select Case ref.Name
                    Case "P"
                        [property] = (From p In type.GetProperties() Where p.Name = pname).DefaultFirst

                        If [property] Is Nothing Then
                            Continue For
                        Else
                            ptype = [property].PropertyType
                        End If
                    Case "F"
                        field = (From f In type.GetFields Where f.Name = pname).DefaultFirst

                        If field Is Nothing Then
                            Continue For
                        Else
                            ptype = field.FieldType
                        End If
                    Case "M"
                        method = (From m In type.GetMethods Where m.Name = pname).DefaultFirst

                        If method Is Nothing Then
                            Continue For
                        Else
                            ptype = method.ReturnType
                        End If
                    Case Else
                        Throw New Exception("this exception will be never happended!")
                End Select

                html = $"{typeLink(type)}.<a href=""{typeLink(ptype).href([default]:="#")}"">{pname}</a>"

                push_clr(type)
                push_clr(ptype)

                Yield (link, html)
            End If
        Next
    End Function

    Public Shared Function HandlingTypeReferenceInDocs(doc_str As String) As String
        If doc_str.StringEmpty Then
            Return ""
        End If

        For Each link In ParseTypeReference(doc_str)
            doc_str = doc_str.Replace(link.str, typeLink(link.Item2))
        Next
        For Each link In ParsePropertyReference(doc_str)
            doc_str = doc_str.Replace(link.str, link.html_replace)
        Next

        Return doc_str
    End Function

    Friend Shared Sub push_clr(t As Type)
        If t Is Nothing Then
            Return
        End If

        If t.IsArray Then
            t = t.GetElementType
        ElseIf isGeneralCollection(t) Then
            t = t.GetGenericArguments.First
        End If

        Call clr_types.Add(t)
    End Sub

    Private Shared Function isGeneralCollection(type As Type) As Boolean
        If Not type.IsConstructedGenericType Then
            Return False
        End If

        Dim base As Type = type.UnderlyingSystemType

        For Each generic As Type In {
            GetType(IEnumerable(Of )),
            GetType(List(Of )),
            GetType(Queue(Of )),
            GetType(IDynamicMeta(Of )),
            GetType(DynamicPropertyBase(Of )),
            GetType(Dictionary(Of )),
            GetType(LinkedList(Of )),
            GetType(IList(Of )),
            GetType(ICollection(Of )),
            GetType(NamedCollection(Of ))
        }
            If base.Name = generic.Name Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Shared Function isGeneralTuple(type As Type) As Boolean
        If Not type.IsConstructedGenericType Then
            Return False
        End If

        Dim base As Type = type.UnderlyingSystemType

        For Each generic As Type In {
            GetType(KeyValuePair(Of ,)),
            GetType(ValueTuple(Of ,)),
            GetType([Variant](Of ,))
        }
            If base.Name = generic.Name Then
                Return True
            End If
        Next

        Return False
    End Function

    ''' <summary>
    ''' this function generates the html anchor html text for link to the document of clr type
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    Public Shared Function typeLink(type As Type, Optional show_clr_array As Boolean = True) As String
        Dim rtype As RType = RType.GetRSharpType(type)
        Dim desc As String = ""

        If rtype Is Nothing Then
            Return ""
        End If

        If type.ImplementInterface(GetType(IDictionary)) Then
            rtype = RType.list
        End If
        If isGeneralCollection(type) Then
            type = type.GetGenericArguments.First
            push_clr(type)
            desc = "iterates"
        End If
        If isGeneralTuple(type) Then
            Dim t1 As Type = type.GetGenericArguments.First
            Dim t2 As Type = type.GetGenericArguments.Last

            Call push_clr(t1)
            Call push_clr(t2)

            Return $"tuple[{typeLink(t1, show_clr_array)}, {typeLink(t2, show_clr_array)}]"
        End If

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
                    If desc.StringEmpty Then
                        Return "<i>any</i> kind"
                    Else
                        Return $"<i>{desc}(any)</i> kind"
                    End If
                Else
                    Dim ns As String = type.Namespace.Replace("."c, "/"c)
                    Dim fileName As String = type.Name
                    Dim typeName As String = type.Name

                    If type.IsArray Then
                        fileName = type.GetElementType.Name

                        If Not show_clr_array Then
                            typeName = fileName
                        End If
                    End If

                    If Not desc.StringEmpty Then
                        Return $"<a href=""/vignettes/clr/{ns}/{fileName}.html"">{desc}({typeName})</a>"
                    Else
                        Return $"<a href=""/vignettes/clr/{ns}/{fileName}.html"">{typeName}</a>"
                    End If
                End If
        End Select
    End Function

End Class

Imports System.Reflection
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
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

        For Each link As String In list
            ref = link.GetValue.GetTagValue(":", trim:=True)

            If ref.Name <> "P" Then
                Continue For
            End If

            clr_type = ref.Value.Split("."c)
            type = AssemblyInfo.GetType(clr_type.Take(clr_type.Length - 1).JoinBy("."))
            pname = clr_type.Last

            If Not type Is Nothing Then
                [property] = type.GetProperty(name:=pname)
                html = $"{typeLink(type)}.<a href=""{typeLink([property].PropertyType).href([default]:="#")}"">{pname}</a>"

                push_clr(type)
                push_clr([property].PropertyType)

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
        If t.IsArray Then
            t = t.GetElementType
        End If

        Call clr_types.Add(t)
    End Sub

    ''' <summary>
    ''' this function generates the html anchor html text for link to the document of clr type
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns></returns>
    Friend Shared Function typeLink(type As Type) As String
        Dim rtype As RType = RType.GetRSharpType(type)
        Dim desc As String = ""

        If type.ImplementInterface(GetType(IDictionary)) Then
            rtype = RType.list
        End If
        If type Is GetType(IEnumerable(Of )) Then
            type = type.GetGenericArguments.First
            desc = "iterates"
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

                    If type.IsArray Then
                        fileName = type.GetElementType.Name
                    End If

                    If Not desc.StringEmpty Then
                        Return $"<a href=""/vignettes/clr/{ns}/{fileName}.html"">{desc}({type.Name})</a>"
                    Else
                        Return $"<a href=""/vignettes/clr/{ns}/{fileName}.html"">{type.Name}</a>"
                    End If
                End If
        End Select
    End Function

End Class

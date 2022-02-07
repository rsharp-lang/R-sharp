Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Text.Xml.Models

''' <summary>
''' The R document model
''' </summary>
Public Class RDoc : Implements INamedValue

#Region "Identifier"
    ' 这两个是纯字符串的对象标记信息 
    Public Property name As String Implements IKeyedEntity(Of String).Key
    Public Property [alias] As String
#End Region

    Public Property title As Doc
    Public Property usage As String
    Public Property arguments As Item()
    Public Property description As Doc
    Public Property examples As String
    ''' <summary>
    ''' 整个文档内的注释，这个属性值不是对目标函数对象的注释
    ''' </summary>
    ''' <returns></returns>
    Public Property comments As String

    Public Function GetHtmlDoc() As String
        Dim html As New XmlBuilder

        If Not comments.StringEmpty Then
            Call html.AddComment(comments)
        End If

        Return html.ToString
    End Function

    Public Function GetMarkdownDoc() As String
        Throw New NotImplementedException
    End Function
End Class

Public Class Item : Implements INamedValue

    Public Property name As String Implements IKeyedEntity(Of String).Key
    Public Property description As Doc

End Class

Public Class Enumerate

    Public Property items As Doc()

End Class

#Region "Doc content model"

Public Class Doc

    Public Property Fragments As DocFragment()

    Public ReadOnly Property PlainText As String
        Get
            Return ToString()
        End Get
    End Property

    Public Function GetMarkdown() As String
        Throw New NotImplementedException
    End Function

    Public Function GetHtml() As String
        Throw New NotImplementedException
    End Function

    Public Overrides Function ToString() As String
        Return Fragments.Select(Function(frag) frag.ToString).JoinBy(" ")
    End Function
End Class

Public MustInherit Class DocFragment

End Class

Public Class PlainText : Inherits DocFragment

    Public Property text As String

    Public Overrides Function ToString() As String
        Return text
    End Function
End Class

Public Class Code : Inherits DocFragment

    Public Property content As DocFragment

    Public Overrides Function ToString() As String
        Return content.ToString
    End Function
End Class

Public Class Link : Inherits DocFragment

    Public Property target As String

    Public Overrides Function ToString() As String
        Return target.ToString
    End Function
End Class

#End Region

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

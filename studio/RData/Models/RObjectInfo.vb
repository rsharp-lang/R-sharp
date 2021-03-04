

''' <summary>
''' Internal attributes of a R object.
''' </summary>
Public Class RObjectInfo

    Public type As RObjectType
    Public [object] As Boolean
    Public attributes As Boolean
    Public tag As Boolean
    Public gp As Integer
    Public reference As Integer

    Public Overrides Function ToString() As String
        Return type.Description
    End Function

End Class
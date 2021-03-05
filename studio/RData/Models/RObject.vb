
''' <summary>
''' Representation of a R object.
''' </summary>
Public Class RObject

    Public Property info As RObjectInfo
    Public Property value As Object
    Public Property attributes As RObject
    Public Property tag As RObject
    Public Property referenced_object As RObject

    Public Overrides Function ToString() As String
        Return MyBase.ToString()
    End Function
End Class
Imports int = System.Int32
Imports bool = System.Boolean

''' <summary>
''' R versions.
''' </summary>
Public Class RVersions

    Public format As int
    Public serialized As int
    Public minimum As int

End Class

''' <summary>
''' Extra information.
'''
''' Contains the Default encoding (only In version 3).
''' </summary>
Public Class RExtraInfo

    Public encoding As String = Nothing
End Class

''' <summary>
''' Internal attributes of a R object.
''' </summary>
Public Class RObjectInfo

    Public type As RObjectType
    Public [object] As bool
    Public attributes As bool
    Public tag As bool
    Public gp As int
    Public reference As int

End Class

''' <summary>
''' Representation of a R object.
''' </summary>
Public Class RObject

    Public info As RObjectInfo
    Public value As Object
    Public Property attributes As RObject
    Public tag As RObject
    Public referenced_object As RObject
End Class

''' <summary>
''' Data contained in a R file.
''' </summary>
Public Class RData
    Public versions As RVersions
    Public extra As RExtraInfo
    Public [object] As RObject
End Class

''' <summary>
''' Value of an environment.
''' </summary>
Public Class EnvironmentValue
    Public locked As bool
    Public enclosure As RObject
    Public frame As RObject
    Public hash_table As RObject
End Class
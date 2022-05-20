Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Public Class Item : Implements INamedValue

    Public Property name As String Implements IKeyedEntity(Of String).Key
    Public Property description As Doc

End Class

Public Class Enumerate

    Public Property items As Doc()

End Class

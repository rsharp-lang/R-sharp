Namespace Runtime.PrimitiveTypes

    Public Class list : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.list, GetType(Dictionary(Of [string], Object)).FullName)
        End Sub
    End Class
End Namespace
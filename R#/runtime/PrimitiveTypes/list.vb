Namespace Runtime.PrimitiveTypes

    Public Class list : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.list, GetType(Dictionary(Of String, Object)))
        End Sub

        Public Overrides Function ToString() As String
            Return "R# list"
        End Function
    End Class
End Namespace
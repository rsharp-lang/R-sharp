Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.boolean"/>
    ''' </summary>
    Public Class logical : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.boolean, GetType(Boolean))
        End Sub

        Public Overrides Function ToString() As String
            Return "R# logical"
        End Function
    End Class
End Namespace
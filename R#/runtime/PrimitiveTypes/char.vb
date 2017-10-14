Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.char"/>
    ''' </summary>
    Public Class character : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.char, GetType(Char))
        End Sub

        Public Overrides Function ToString() As String
            Return "R# char"
        End Function
    End Class
End Namespace
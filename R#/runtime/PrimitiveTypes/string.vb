Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.string"/>
    ''' </summary>
    Public Class [string] : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.string, GetType(String))
        End Sub

        Public Overrides Function ToString() As String
            Return "R# string"
        End Function
    End Class
End Namespace
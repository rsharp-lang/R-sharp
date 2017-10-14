Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.uinteger"/>
    ''' </summary>
    Public Class [uinteger] : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.uinteger, GetType(ULong))
        End Sub

        Public Overrides Function ToString() As String
            Return "R# uinteger"
        End Function
    End Class
End Namespace

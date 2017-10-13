Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.uinteger"/>
    ''' </summary>
    Public Class [uinteger] : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.uinteger, GetType(ULong).FullName)
        End Sub
    End Class
End Namespace

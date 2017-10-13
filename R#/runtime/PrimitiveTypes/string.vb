Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.string"/>
    ''' </summary>
    Public Class [string] : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.string, GetType(String).FullName)
        End Sub
    End Class
End Namespace
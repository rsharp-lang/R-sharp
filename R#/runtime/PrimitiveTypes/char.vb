Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.char"/>
    ''' </summary>
    Public Class character : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.char, GetType(Char))
        End Sub
    End Class
End Namespace
Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.boolean"/>
    ''' </summary>
    Public Class logical : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.boolean, GetType(Boolean).FullName)
        End Sub
    End Class
End Namespace
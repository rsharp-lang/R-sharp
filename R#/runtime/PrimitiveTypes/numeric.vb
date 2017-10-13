Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.double"/>
    ''' </summary>
    Public Class numeric : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.double, GetType(Double).FullName)
        End Sub
    End Class
End Namespace
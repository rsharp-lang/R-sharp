Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Runtime.PrimitiveTypes

    ''' <summary>
    ''' <see cref="TypeCodes.integer"/>
    ''' </summary>
    Public Class [integer] : Inherits RType

        Sub New()
            Call MyBase.New(TypeCodes.integer, GetType(Integer).FullName)
            Call MyBase.[New]()


        End Sub


    End Class
End Namespace
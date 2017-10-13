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

        Public Shared Function op_Add(x As Object, y As Object) As IEnumerable(Of Object)

        End Function
    End Class
End Namespace
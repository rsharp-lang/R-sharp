Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language

Public MustInherit Class RDocParser

    Protected text As Pointer(Of Char)
    Protected buffer As New List(Of Char)

    Protected MustOverride Sub walkChar(c As Char)

End Class
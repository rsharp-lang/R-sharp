Imports Microsoft.VisualBasic.Emit.Marshal

Public Class ContentParser : Inherits RDocParser

    Friend Sub New(text As Pointer(Of Char))
        Me.text = text
    End Sub

    Protected Overrides Sub walkChar(c As Char)
        Throw New NotImplementedException()
    End Sub

    ''' <summary>
    ''' 可能包含有纯文本以及以下的标签：``\code{}``,``\link{}``,``\enumerate{}``
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCurrentContent() As Doc
        Throw New NotImplementedException
    End Function
End Class

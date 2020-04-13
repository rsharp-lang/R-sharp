Namespace Runtime.Interop

    Public Class BinaryIndex

        ''' <summary>
        ''' the operator symbol text
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property symbol As String

        Sub New(symbol As String)
            Me.symbol = symbol
        End Sub
    End Class
End Namespace
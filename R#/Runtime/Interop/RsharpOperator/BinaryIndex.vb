Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Runtime.Interop

    Public Class BinaryIndex : Implements IReadOnlyId

        ''' <summary>
        ''' the operator symbol text
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property symbol As String Implements IReadOnlyId.Identity

        ReadOnly operators As New List(Of BinaryOperator)

        Sub New(symbol As String)
            Me.symbol = symbol
        End Sub

        Public Function Evaluate(left As Object, right As Object, env As Environment) As Object

        End Function
    End Class
End Namespace
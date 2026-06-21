Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Internal.Object

    Public Class PipeIterator(Of T) : Implements IEnumerable(Of T)

        Dim data As T()
        Dim err As Message

        Public ReadOnly Property isError As Boolean
            Get
                Return Not err Is Nothing
            End Get
        End Property

        Public ReadOnly Property length As Integer
            Get
                If isError Then
                    Return 0
                Else
                    Return data.Length
                End If
            End Get
        End Property

        Public Function getData() As T()
            Return data
        End Function

        Sub New(data As IEnumerable(Of T))
            Me.data = data.ToArray
        End Sub

        Sub New(err As Message)
            Me.err = err
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each item As T In data
                Yield item
            Next
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class
End Namespace
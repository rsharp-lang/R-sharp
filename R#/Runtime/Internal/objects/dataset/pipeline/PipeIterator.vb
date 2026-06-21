Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Internal.Object

    Public Class PipeIterator(Of T) : Implements IEnumerable(Of T)

        Dim data As T()
        Dim err As Message

        Public ReadOnly Property isError As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

        Public ReadOnly Property scalar As T
            Get
                If length > 0 Then
                    Return data(0)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getError() As Message
            Return err
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getData() As T()
            Return data
        End Function

        Sub New(data As IEnumerable(Of T))
            Me.data = data.ToArray
        End Sub

        Sub New(err As Message)
            Me.err = err
        End Sub

        Public Overrides Function ToString() As String
            If isError Then
                Return err.message.JoinBy(". ")
            ElseIf length = 0 Then
                Return "[]"
            ElseIf length = 1 Then
                Return $"scalar({GetType(T).Name} {scalar.ToString})"
            Else
                Return $"[{length}x{GetType(T).Name}] {data.Take(6).JoinBy(", ")}..."
            End If
        End Function

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
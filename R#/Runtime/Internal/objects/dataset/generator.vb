Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' A generator function wrapper
    ''' </summary>
    Public Class generator : Inherits RsharpDataObject

        Public Property fun As RFunction
        Public ReadOnly Property current As Object

        Dim generator As IEnumerator

        Public Function moveNext() As Boolean
            _current = generator.Current
            Return generator.MoveNext
        End Function

    End Class
End Namespace
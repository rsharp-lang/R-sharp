Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' The R# pipeline
    ''' </summary>
    Public Class pipeline

        ReadOnly pipeline As IEnumerable
        ReadOnly elementType As RType

        Sub New(input As IEnumerable, type As Type)
            pipeline = input
            elementType = RType.GetRSharpType(type)
        End Sub

        Public Function createVector() As vector
            Return New vector(elementType, pipeline)
        End Function

        Public Iterator Function populates(Of T)() As IEnumerable(Of T)
            For Each obj As Object In pipeline
                Yield DirectCast(obj, T)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"pipeline[{elementType.ToString}]"
        End Function
    End Class
End Namespace
Namespace Runtime.Vectorization

    ''' <summary>
    ''' Type cache module
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public NotInheritable Class typedefine(Of T)

        ''' <summary>
        ''' The vector based type
        ''' </summary>
        Public Shared ReadOnly baseType As Type
        ''' <summary>
        ''' The abstract vector type
        ''' </summary>
        Public Shared ReadOnly enumerable As Type

        Private Sub New()
        End Sub

        Shared Sub New()
            baseType = GetType(T)
            enumerable = GetType(IEnumerable(Of T))
        End Sub
    End Class
End Namespace
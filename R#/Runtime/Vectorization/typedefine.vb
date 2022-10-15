Namespace Runtime.Vectorization

    ''' <summary>
    ''' Type cache module
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public NotInheritable Class typedefine(Of T)

        ''' <summary>
        ''' The vector based type(type of scaler value)
        ''' </summary>
        Public Shared ReadOnly baseType As Type
        ''' <summary>
        ''' The abstract vector type(array, list, collection, etc)
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
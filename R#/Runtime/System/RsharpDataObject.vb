Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    Public MustInherit Class RsharpDataObject

        Protected m_type As RType

        Public Overridable Property elementType As RType
            Get
                Return m_type
            End Get
            Protected Set(value As RType)
                m_type = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return $"{MyClass.GetType.Name}<{elementType.ToString}>"
        End Function
    End Class
End Namespace
Namespace Runtime.Components

    Public Class RType

        Public ReadOnly Property fullName As String
            Get
                Return raw.FullName
            End Get
        End Property

        Public ReadOnly Property mode As TypeCodes
        Public ReadOnly Property isArray As Boolean
        Public ReadOnly Property raw As Type

        Sub New(raw As Type)
            Me.raw = raw
            Me.isArray = raw.IsInheritsFrom(GetType(Array))
        End Sub
    End Class
End Namespace
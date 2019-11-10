Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

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

        Public Overrides Function ToString() As String
            Return $"<{mode.Description}> {raw.Name}"
        End Function
    End Class
End Namespace
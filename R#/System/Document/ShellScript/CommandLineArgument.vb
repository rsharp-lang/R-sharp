Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development.CommandLine

    Friend Class ArgumentInfo

        Friend attrs As New Dictionary(Of String, String())
        Friend type As TypeCodes = TypeCodes.string

        Default Public ReadOnly Property Item(name As String) As String
            Get
                Return attrs.TryGetValue("info").JoinBy(";" & vbCrLf)
            End Get
        End Property

    End Class

    Friend Class CommandLineArgument

        Public Property name As String
        Public Property defaultValue As String
        Public Property type As String
        Public Property description As String

    End Class
End Namespace
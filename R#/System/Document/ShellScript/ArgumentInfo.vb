Imports Microsoft.VisualBasic.Language.Default
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development.CommandLine

    Friend Class ArgumentInfo

        Friend attrs As New Dictionary(Of String, String())

        ''' <summary>
        ''' argument value type in the commandline input.
        ''' </summary>
        ReadOnly type As TypeCodes?

        Default Public ReadOnly Property Item(name As String) As String
            Get
                Return attrs.TryGetValue("info").JoinBy(";" & vbCrLf)
            End Get
        End Property

        Sub New(type As TypeCodes)
            If (Not type = TypeCodes.generic) AndAlso (Not type = TypeCodes.NA) Then
                Me.type = type
            Else
                Me.type = Nothing
            End If
        End Sub

        Public Function [GetTypeCode]() As String
            Static stringType As [Default](Of String) = "string"

            If attrs.ContainsKey("type") Then
                Return attrs("type").FirstOrDefault Or stringType
            End If

            If type Is Nothing Then
                Return "string"
            Else
                Return CType(type, TypeCodes).ToString
            End If
        End Function

    End Class
End Namespace
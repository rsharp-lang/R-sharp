''' <summary>
''' R versions.
''' </summary>
Public Class RVersions

    Public format As Integer
    Public serialized As Integer
    Public minimum As Integer

    Public Overrides Function ToString() As String
        Return $"{format}.{serialized}.{minimum}"
    End Function

End Class



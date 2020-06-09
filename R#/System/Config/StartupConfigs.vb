Namespace System.Configuration

    Public Class StartupConfigs

        Public Property loadingPackages As String()

        Public Shared Function DefaultLoadingPackages() As String()
            Return {"base", "utils", "grDevices", "stats"}
        End Function

    End Class
End Namespace
Imports SMRUCC.Rsharp.Runtime

Namespace Development

    Public Class RFileSystem

        Public Shared Function ListPackageDir(env As Environment) As IEnumerable(Of String)
            Return GetPackageDir(env).ListDirectory
        End Function

        Public Shared Function GetPackageDir(env As Environment) As String
            Dim packageDir As String

            If App.IsMicrosoftPlatform Then
                packageDir = $"{env.globalEnvironment.options.lib_loc}/Library/"
            Else
                packageDir = env.globalEnvironment.options.lib_loc
            End If

            Return packageDir
        End Function

        Public Shared Function PackageInstalled(packageName As String, env As Environment) As Boolean
            Return $"{GetPackageDir(env)}/{packageName}".DirectoryExists
        End Function
    End Class
End Namespace
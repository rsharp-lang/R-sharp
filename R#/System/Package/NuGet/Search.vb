Namespace Development.Package.NuGet

    Public Class Search

        Public Property data As PackageData()
        Public Property totalHits As Integer

        Public Shared Narrowing Operator CType(search As Search) As PackageData()
            Return search.data
        End Operator

    End Class

    Public Class PackageData

        Public Property authors As String()
        Public Property description As String
        Public Property iconUrl As String
        Public Property id As String
        Public Property licenseUrl As String
        Public Property owners As String()
        Public Property packageTypes As PackageType()
        Public Property projectUrl As String
        Public Property registration As String
        Public Property summary As String
        Public Property tags As String()
        Public Property title As String
        Public Property totalDownloads As Integer
        Public Property verified As Boolean
        Public Property version As String
        Public Property versions As PackageVersion()

    End Class

    Public Class PackageVersion
        Public Property version As String
        Public Property downloads As Integer
    End Class

    Public Class PackageType

        Public Property name As String
    End Class
End Namespace
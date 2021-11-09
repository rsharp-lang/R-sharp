Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Development.Package.NuGet

    Public Enum NuGetMirrors
        nuget
        azuresearch
        azure_cn
    End Enum

    ''' <summary>
    ''' use nuget package repository system as R# package repository
    ''' </summary>
    Public Class WebRequest

        Public Shared ReadOnly mirrors As IReadOnlyDictionary(Of String, String) =
            New Dictionary(Of String, String) From {
                {"azuresearch", "https://azuresearch-usnc.nuget.org"}
            }

        Shared m_mirror As String

        Sub New()
            Call SetMirror(NuGetMirrors.azuresearch)
        End Sub

        Public Shared Sub SetMirror(mirror As String)
            WebRequest.m_mirror = mirror
        End Sub

        Public Shared Sub SetMirror(mirror As NuGetMirrors)
            WebRequest.m_mirror = mirrors(mirror.Description)
        End Sub

        Public Shared Function Query(term As String, Optional pre_release As Boolean = False) As PackageData()
            Dim url As String = $"{m_mirror}/query?q={term.UrlEncode}&prerelease={pre_release.ToString.ToLower}"
            Dim json As String = url.GET
            Dim data = json.LoadJSON(Of NuGet.Search)

            Return data
        End Function
    End Class
End Namespace
Imports Flute.Http.Configurations
Imports Flute.SessionManager
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Net.HTTP

Public Class AccessController

    ''' <summary>
    ''' key for check of the session file
    ''' </summary>
    ''' <returns></returns>
    Public Property status_key As String

    Public Property ignores As String()
        Get
            Return m_ignoreIndex.Objects
        End Get
        Set(value As String())
            m_ignoreIndex = value.Indexing
        End Set
    End Property

    Dim m_ignoreIndex As Index(Of String)

    Public Function CheckAccess(url As URL, ssid As String, config As Configuration) As Boolean
        If url.path Like m_ignoreIndex Then
            Return True
        End If

        Dim session As SessionFile = Flute.SessionManager.Open(ssid, config)
        Dim check As String = session.OpenKeyString(status_key)

        Return Not check.StringEmpty(testEmptyFactor:=True)
    End Function

End Class


Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("igraph")>
Public Module NetworkModule

    <ExportAPI("save.network")>
    Public Function SaveNetwork(g As Object, file$, Optional properties As String() = Nothing) As Boolean
        If g Is Nothing Then

        End If
    End Function
End Module

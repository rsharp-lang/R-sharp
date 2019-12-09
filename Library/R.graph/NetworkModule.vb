
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("igraph")>
Public Module NetworkModule

    <ExportAPI("save.network")>
    Public Function SaveNetwork(g As Object, file$, Optional properties As String() = Nothing) As Boolean
        If g Is Nothing Then
            Throw New ArgumentNullException("g")
        End If

        Dim tables As NetworkTables

        If g.GetType Is GetType(NetworkGraph) Then
            tables = DirectCast(g, NetworkGraph).Tabular(properties)
        ElseIf g.GetType Is GetType(NetworkTables) Then
            tables = g
        Else
            Throw New InvalidProgramException(g.GetType.FullName)
        End If

        Return tables.Save(file)
    End Function
End Module

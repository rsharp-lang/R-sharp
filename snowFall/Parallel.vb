Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Parallel

<Package("Parallel")>
Public Module Parallel

    ''' <summary>
    ''' run parallel client node task
    ''' </summary>
    ''' <param name="port"></param>
    ''' <returns></returns>
    <ExportAPI("snowFall")>
    Public Function snowFall(port As Integer) As Object
        Return New TaskBuilder(port).Run
    End Function
End Module

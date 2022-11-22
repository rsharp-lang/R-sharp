Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' backend for the http help documents
''' </summary>
<Package("help")>
Public Module httpHelp

    ''' <summary>
    ''' generate html page for keyword query
    ''' </summary>
    ''' <param name="term"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("search")>
    Public Function handleSearch(term As String, Optional env As Environment = Nothing) As Object
        Dim libpath As String = env.globalEnvironment.options.lib_loc

        Return libpath
    End Function

End Module

Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' backend for the http help documents
''' </summary>
<Package("help")>
Public Module httpHelp

    Dim lib_renv As String

    <ExportAPI("http_load")>
    Public Function load(Optional env As Environment = Nothing) As Object
        lib_renv = env.globalEnvironment.options.lib_loc.GetDirectoryFullPath

        Return True
    End Function

    ''' <summary>
    ''' generate html page for keyword query
    ''' </summary>
    ''' <param name="term"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("search")>
    Public Function handleSearch(term As String, Optional env As Environment = Nothing) As Object


        Return ""
    End Function

    <ExportAPI("index")>
    Public Function getHelpIndex(Optional env As Environment = Nothing) As Object
        Dim dirs As String() = lib_renv _
            .ListDirectory _
            .Select(Function(pkg) $"{pkg}\package\index.json") _
            .ToArray
        Dim html As New StringBuilder

        html.AppendLine($"<table>")

        For Each pkg As String In dirs
            Dim info As DESCRIPTION = pkg.LoadJsonFile(Of DESCRIPTION)
            Dim line As String = $"<tr>
<td>{info.Package}</td>
<td>{info.Title}</td>
</tr>"
            Call html.AppendLine(line)
        Next

        Return html.AppendLine("</table>").ToString
    End Function

End Module


Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Net

<Package("URL", Category:=APICategories.UtilityTools)>
Public Module URL

    <ExportAPI("url.get")>
    Public Function [get](url As String) As String
        Return url.GET
    End Function

    ''' <summary>
    ''' Do file download
    ''' </summary>
    ''' <param name="url$"></param>
    ''' <param name="saveAs$"></param>
    ''' <returns></returns>
    <ExportAPI("wget")>
    Public Function wget(url$, saveAs$) As Boolean
        Return Http.wget.Download(url, saveAs)
    End Function
End Module

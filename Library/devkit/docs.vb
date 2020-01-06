Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

''' <summary>
''' R# help document tools
''' </summary>
<Package("utils.docs", Category:=APICategories.SoftwareTools, Publisher:="I@xieguigang.me")>
Module docs

    ''' <summary>
    ''' Create html help document for the specific package module
    ''' </summary>
    ''' <param name="package$"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("makehtml.docs")>
    Public Function makeHtmlDocs(package$, env As Environment) As String

    End Function
End Module

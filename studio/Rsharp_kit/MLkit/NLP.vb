Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' NLP tools
''' </summary>
<Package("NLP")>
Module NLP

    <ExportAPI("segmentation")>
    Public Function Tokenice(text As String) As Paragraph()
        Return Paragraph.Segmentation(text).ToArray
    End Function

    <ExportAPI("article")>
    Public Function CrawlerText(html As String,
                                Optional depth As Integer = 6,
                                Optional limitCount As Integer = 180,
                                Optional appendMode As Boolean = False) As Article

        Return Article.ParseText(html, depth, limitCount, appendMode)
    End Function

End Module

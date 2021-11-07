Imports Microsoft.VisualBasic.CommandLine.Reflection
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

End Module

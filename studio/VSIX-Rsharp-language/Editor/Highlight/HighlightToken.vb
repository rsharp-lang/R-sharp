Imports Microsoft.VisualStudio.Text.Tagging

Public Class HighlightToken : Inherits TextMarkerTag

    Public Sub New()
        MyBase.New(HighlightWordFormatDefinition.FormatName)
    End Sub
End Class

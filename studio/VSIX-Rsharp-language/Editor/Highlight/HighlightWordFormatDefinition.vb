Imports System.ComponentModel.Composition
Imports System.Windows.Media
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

<Export(GetType(EditorFormatDefinition))>
<Name(HighlightWordFormatDefinition.FormatName)>
<UserVisible(True)>
Public Class HighlightWordFormatDefinition : Inherits MarkerFormatDefinition

    Public Const FormatName As String = "MarkerFormatDefinition/HighlightWordFormatDefinition"

    Sub New()
        BackgroundColor = Colors.LightBlue
        ForegroundColor = Colors.DarkBlue
        DisplayName = "keyword"
        ZOrder = 5
    End Sub
End Class

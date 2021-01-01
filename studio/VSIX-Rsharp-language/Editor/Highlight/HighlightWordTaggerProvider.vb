Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Editor
Imports Microsoft.VisualStudio.Text.Operations
Imports Microsoft.VisualStudio.Text.Tagging
Imports Microsoft.VisualStudio.Utilities

<Export(GetType(IViewTaggerProvider))>
<ContentType("text")>
<TagType(GetType(TextMarkerTag))>
Public Class HighlightWordTaggerProvider : Implements IViewTaggerProvider

    <Import>
    Friend Property TextSearchService As ITextSearchService

    <Import>
    Friend Property TextStructureNavigatorSelector As ITextStructureNavigatorSelectorService


    Public Function CreateTagger(Of T As ITag)(textView As ITextView, buffer As ITextBuffer) As ITagger(Of T) Implements IViewTaggerProvider.CreateTagger
        'provide highlighting only on the top buffer 
        If (textView.TextBuffer IsNot buffer) Then
            Return Nothing
        End If
        Dim textStructureNavigator As ITextStructureNavigator =
        TextStructureNavigatorSelector.GetTextStructureNavigator(buffer)

        Return New TokenTag(textView, buffer, TextSearchService, textStructureNavigator)
    End Function
End Class

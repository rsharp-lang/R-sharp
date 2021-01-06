#Region "Microsoft.VisualBasic::9686a8956d4320380046f60f5a5c6bcb, studio\VSIX-Rsharp-language\Editor\Highlight\HighlightWordTaggerProvider.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Class HighlightWordTaggerProvider
    ' 
    '     Properties: TextSearchService, TextStructureNavigatorSelector
    ' 
    '     Function: CreateTagger
    ' 
    ' /********************************************************************************/

#End Region

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


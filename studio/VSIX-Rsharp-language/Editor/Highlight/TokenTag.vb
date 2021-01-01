Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Editor
Imports Microsoft.VisualStudio.Text.Operations
Imports Microsoft.VisualStudio.Text.Tagging

Public Class TokenTag : Implements ITagger(Of HighlightToken)

    Public Event TagsChanged As EventHandler(Of SnapshotSpanEventArgs) Implements ITagger(Of HighlightToken).TagsChanged

    Public Property View As ITextView
    Public Property SourceBuffer As ITextBuffer
    Public Property TextSearchService As ITextSearchService
    Public Property TextStructureNavigator As ITextStructureNavigator
    Public Property WordSpans As NormalizedSnapshotSpanCollection
    Public Property CurrentWord As SnapshotSpan?
    Public Property RequestedPoint As SnapshotPoint
    Public Property updateLock As New Object

    Sub New(view As ITextView, sourceBuffer As ITextBuffer, textSearchService As ITextSearchService, textStructureNavigator As ITextStructureNavigator)
        Me.View = view
        Me.SourceBuffer = sourceBuffer
        Me.TextSearchService = textSearchService
        Me.TextStructureNavigator = textStructureNavigator
        Me.WordSpans = New NormalizedSnapshotSpanCollection()
        Me.CurrentWord = Nothing

        AddHandler Me.View.Caret.PositionChanged, AddressOf CaretPositionChanged
        AddHandler Me.View.LayoutChanged, AddressOf ViewLayoutChanged
    End Sub

    Sub ViewLayoutChanged(sender As Object, e As TextViewLayoutChangedEventArgs)

        ' If a New snapshot wasn't generated, then skip this layout 
        If (e.NewSnapshot IsNot e.OldSnapshot) Then
            UpdateAtCaretPosition(View.Caret.Position)
        End If
    End Sub


    Sub CaretPositionChanged(sender As Object, e As CaretPositionChangedEventArgs)

        UpdateAtCaretPosition(e.NewPosition)
    End Sub

    Sub UpdateAtCaretPosition(caretPosition As CaretPosition)

        Dim point As SnapshotPoint? = caretPosition.Point.GetPoint(SourceBuffer, caretPosition.Affinity)

        If (Not point.HasValue) Then
            Return
        End If

        ' If the New caret position Is still within the current word (And on the same snapshot), we don't need to check it 
        If (CurrentWord.HasValue AndAlso
            CurrentWord.Value.Snapshot Is View.TextSnapshot AndAlso
            (point.Value.CompareTo(CurrentWord.Value.Start) >= 0) AndAlso
           (point.Value.CompareTo(CurrentWord.Value.End) <= 0)) Then

            Return
        End If

        RequestedPoint = point.Value
        UpdateWordAdornments()
    End Sub

    Shared Function WordExtentIsValid(currentRequest As SnapshotPoint, word As TextExtent) As Boolean

        Return word.IsSignificant AndAlso currentRequest.Snapshot.GetText(word.Span).Any(AddressOf Char.IsLetter)
    End Function

    Sub UpdateWordAdornments()

        Dim currentRequest As SnapshotPoint = RequestedPoint
        Dim wordSpans As New List(Of SnapshotSpan)()
        ' Find all words in the buffer Like the one the caret Is on
        Dim word As TextExtent = TextStructureNavigator.GetExtentOfWord(currentRequest)
        Dim foundWord = True
        'If we've selected something not worth highlighting, we might have missed a "word" by a little bit
        If (Not WordExtentIsValid(currentRequest, word)) Then

            'Before we retry, make sure it Is worthwhile 
            If (word.Span.Start <> currentRequest OrElse currentRequest = currentRequest.GetContainingLine().Start OrElse Char.IsWhiteSpace((currentRequest - 1).GetChar())) Then

                foundWord = False

            Else

                '  Try again, one character previous.  
                ' If the caret Is at the end of a word, pick up the word.
                word = TextStructureNavigator.GetExtentOfWord(currentRequest - 1)

                ' If the word still isn't valid, we're done 
                If (Not WordExtentIsValid(currentRequest, word)) Then
                    foundWord = False
                End If
            End If
        End If

        If (Not foundWord) Then

            ' If we couldn't find a word, clear out the existing markers
            SynchronousUpdate(currentRequest, New NormalizedSnapshotSpanCollection(), Nothing)
            Return
        End If

        Dim currentWord As SnapshotSpan = word.Span
        ' If this Is the current word, And the caret moved within a word, we're done. 
        If (Me.CurrentWord.HasValue AndAlso Me.CurrentWord = currentWord) Then
            Return
        End If
        ' Find the New spans
        Dim findData As New FindData(currentWord.GetText(), currentWord.Snapshot)
        findData.FindOptions = FindOptions.WholeWord Or FindOptions.MatchCase

        wordSpans.AddRange(TextSearchService.FindAll(findData))

        ' If another change hasn't happened, do a real update 
        If (currentRequest = RequestedPoint) Then
            SynchronousUpdate(currentRequest, New NormalizedSnapshotSpanCollection(wordSpans), currentWord)
        End If
    End Sub

    Sub SynchronousUpdate(currentRequest As SnapshotPoint, newSpans As NormalizedSnapshotSpanCollection, newCurrentWord As SnapshotSpan?)

        SyncLock (updateLock)

            If (currentRequest <> RequestedPoint) Then
                Return
            End If
            WordSpans = newSpans
            CurrentWord = newCurrentWord

            RaiseEvent TagsChanged(Me, New SnapshotSpanEventArgs(New SnapshotSpan(SourceBuffer.CurrentSnapshot, 0, SourceBuffer.CurrentSnapshot.Length)))
        End SyncLock
    End Sub

    Public Iterator Function GetTags(spans As NormalizedSnapshotSpanCollection) As IEnumerable(Of ITagSpan(Of HighlightToken)) Implements ITagger(Of HighlightToken).GetTags
        If (Me.CurrentWord Is Nothing) Then
            Return
        End If
        ' Hold on to a "snapshot" of the word spans And current word, so that we maintain the same
        ' collection throughout
        Dim CurrentWord As SnapshotSpan = Me.CurrentWord.Value
        Dim WordSpans As NormalizedSnapshotSpanCollection = Me.WordSpans

        If (spans.Count = 0 OrElse WordSpans.Count = 0) Then
            Return
        End If

        ' If the requested snapshot isn't the same as the one our words are on, translate our spans to the expected snapshot 
        If (spans(0).Snapshot IsNot WordSpans(0).Snapshot) Then

            WordSpans = New NormalizedSnapshotSpanCollection(
            WordSpans.Select(Function(Span) Span.TranslateTo(spans(0).Snapshot, SpanTrackingMode.EdgeExclusive)))

            CurrentWord = CurrentWord.TranslateTo(spans(0).Snapshot, SpanTrackingMode.EdgeExclusive)
        End If

        ' First, yield back the word the cursor Is under (if it overlaps) 
        ' Note that we'll yield back the same word again in the wordspans collection; 
        ' the duplication here Is expected. 
        If (spans.OverlapsWith(New NormalizedSnapshotSpanCollection(CurrentWord))) Then
            Yield New TagSpan(Of HighlightToken)(CurrentWord, New HighlightToken())
        End If

        ' Second, yield all the other words in the file 
        For Each span As SnapshotSpan In NormalizedSnapshotSpanCollection.Overlap(spans, WordSpans)

            Yield New TagSpan(Of HighlightToken)(span, New HighlightToken())
        Next
    End Function
End Class

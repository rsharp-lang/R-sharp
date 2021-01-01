Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Classification

''' <summary>
''' Classifier that classifies all text as an instance of the "EditorClassifier1" classification type.
''' </summary>
Friend NotInheritable Class EditorClassifier1
    Implements IClassifier

    ''' <summary>
    ''' Classification type
    ''' </summary>
    Private ReadOnly classificationType As IClassificationType

    ''' <summary>
    ''' Initializes a new instance of the <see cref="EditorClassifier1"/> class.
    ''' </summary>
    ''' <param name="registry">Classification registry.</param>
    Friend Sub New(ByVal registry As IClassificationTypeRegistryService)

        Me.classificationType = registry.GetClassificationType("EditorClassifier1")

    End Sub

    ''' <summary>
    ''' An event that occurs when the classification of a span of text has changed.
    ''' </summary>
    ''' <remarks>
    ''' This event gets raised if a non-text change would affect the classification in some way,
    ''' for example typing /* would cause the classification to change in C# without directly
    ''' affecting the span.
    ''' </remarks>
    Public Event ClassificationChanged As EventHandler(Of ClassificationChangedEventArgs) Implements IClassifier.ClassificationChanged

    ''' <summary>
    ''' This method scans the given SnapshotSpan for potential matches for this classification.
    ''' In this instance, it classifies everything and returns each span as a new ClassificationSpan.
    ''' </summary>
    ''' <param name="span">The span currently being classified.</param>
    ''' <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
    Public Function GetClassificationSpans(ByVal span As SnapshotSpan) As IList(Of ClassificationSpan) Implements IClassifier.GetClassificationSpans

        ' Create a list to hold the results
        Dim result As New List(Of ClassificationSpan) From {
            New ClassificationSpan(New SnapshotSpan(span.Snapshot, New span(span.Start, span.Length)), Me.classificationType)
        }

        Return result

    End Function

End Class

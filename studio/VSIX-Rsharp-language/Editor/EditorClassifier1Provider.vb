Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

''' <summary>
''' Classifier provider. It adds the classifier to the set of classifiers.
''' </summary>
<Export(GetType(IClassifierProvider))>
<ContentType("text")> '' This classifier applies to all text files.
Friend NotInheritable Class EditorClassifier1Provider
    Implements IClassifierProvider

    ''' <summary>
    ''' Import the classification registry to be used for getting a reference
    ''' to the custom classification type later.
    ''' </summary>
    <Import()>
    Private classificationRegistry As IClassificationTypeRegistryService

    ''' <summary>
    ''' Gets a classifier for the given text buffer.
    ''' </summary>
    ''' <param name="buffer">The <see cref="ITextBuffer"/> to classify.</param>
    ''' <returns>A classifier for the text buffer, Or null if the provider cannot do so in its current state.</returns>
    Public Function GetClassifier(ByVal buffer As ITextBuffer) As IClassifier Implements IClassifierProvider.GetClassifier

        Return buffer.Properties.GetOrCreateSingletonProperty(Of EditorClassifier1)(Function() New EditorClassifier1(Me.classificationRegistry))

    End Function

End Class

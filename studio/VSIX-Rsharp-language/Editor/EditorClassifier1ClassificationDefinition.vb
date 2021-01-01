Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

''' <summary>
''' Classification type definition export for EditorClassifier1
''' </summary>
Friend Module EditorClassifier1ClassificationDefinition

    ''' <summary>
    ''' Defines the "EditorClassifier1" classification type.
    ''' </summary>
    <Export(GetType(ClassificationTypeDefinition))>
    <Name("EditorClassifier1")>
    Private EditorClassifier1Type As ClassificationTypeDefinition

End Module

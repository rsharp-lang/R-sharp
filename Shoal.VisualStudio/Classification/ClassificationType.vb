Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

Namespace OokLanguage

    Module OrdinaryClassificationDefinition

        ''' <summary>
        ''' Defines the "ordinary" classification type.
        ''' </summary>
        <Export(GetType(ClassificationTypeDefinition)), Name("ook!")>
        Public ookExclamation As ClassificationTypeDefinition = Nothing

        ''' <summary>
        ''' Defines the "ordinary" classification type.
        ''' </summary>
        <Export(GetType(ClassificationTypeDefinition)), Name("ook?")>
        Public ookQuestion As ClassificationTypeDefinition = Nothing

        ''' <summary>
        ''' Defines the "ordinary" classification type.
        ''' </summary>
        <Export(GetType(ClassificationTypeDefinition)), Name("ook.")>
        Public ookPeriod As ClassificationTypeDefinition = Nothing
    End Module
End Namespace

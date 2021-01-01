Imports System.ComponentModel.Composition
Imports System.Windows.Media
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

''' <summary>
''' Defines an editor format for our EditorClassifier1 type that has a purple background
''' and is underlined.
''' </summary>
<Export(GetType(EditorFormatDefinition))>
<ClassificationType(ClassificationTypeNames:="EditorClassifier1")>
<Name("EditorClassifier1")>
<UserVisible(True)>
<Order(After:=Priority.Default)>
Friend NotInheritable Class EditorClassifier1Format
    Inherits ClassificationFormatDefinition

    ''' <summary>
    ''' Initializes a new instance of the <see cref="EditorClassifier1Format"/> class.
    ''' </summary>
    Public Sub New()

        Me.DisplayName = "EditorClassifier1"
        Me.BackgroundColor = Colors.BlueViolet
        Me.TextDecorations = System.Windows.TextDecorations.Underline

    End Sub

End Class

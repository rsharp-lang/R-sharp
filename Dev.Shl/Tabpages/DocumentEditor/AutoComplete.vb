Imports Pavel.CodeEditor
Imports Microsoft.VisualBasic

Namespace DocumentEditor.Components

    Public Class AutoComplete

        Dim Editor As DocumentEditor
        Dim Popmenu As Pavel.CodeEditor.AutocompleteMenu

        Sub New(Editor As Global.Dev.Shl.DocumentEditor.DocumentEditor)
            Me.Editor = Editor
            Me.Popmenu = New Pavel.CodeEditor.AutocompleteMenu(Editor.DocumentEditor1)
            Me.Popmenu.SearchPattern = "[\w\.:=!<>]"
            BuildAutocompleteMenu()
        End Sub

        Private Sub BuildAutocompleteMenu()
            Dim items As New List(Of AutocompleteItem)()

            'For Each item As String In snippets
            '    items.Add(New SnippetAutocompleteItem(item) With {.ImageIndex = 1})
            'Next
            'For Each item As String In declarationSnippets
            '    items.Add(New DeclarationSnippet(item) With {.ImageIndex = 0})
            'Next
            For Each item As String In Program.ScriptEngine.ImportsAPI
                items.Add(New MethodAutocompleteItem(item)) ' With {.ImageIndex = 2})
            Next
            For Each item As String In Microsoft.VisualBasic.Scripting.ShoalShell.Interpreter.LDM.SyntaxModel.Keywords
                items.Add(New AutocompleteItem(item))
            Next

            'items.Add(New InsertSpaceSnippet())
            'items.Add(New InsertSpaceSnippet("^(\w+)([=<>!:]+)(\w+)$"))
            'items.Add(New InsertEnterSnippet())

            'set as autocomplete source
            Popmenu.Items.SetAutocompleteItems(items)
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("AutoComplete component for ""{0}""", Editor.ToString)
        End Function
    End Class
End Namespace
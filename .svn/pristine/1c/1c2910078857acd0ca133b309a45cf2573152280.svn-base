Imports Pavel.CodeEditor
Imports Microsoft.VisualBasic.Scripting.ShoalShell

Namespace DocumentEditor

    Public Class DocumentEditor

        Public Property DocumentFile As String

        Public Overrides Property Text As String
            Get
                Return DocumentEditor1.Text
            End Get
            Set(value As String)
                DocumentEditor1.Text = value
            End Set
        End Property

        Dim _realtimeCompiled As Scripting.ShoalShell.Runtime.ScriptEngine

        Public ReadOnly Property RealtimeCompiled As Runtime.ScriptEngine
            Get
                Return _realtimeCompiled
            End Get
        End Property

#Region "Components"

        Dim AutoComplete As Dev.Shl.DocumentEditor.Components.AutoComplete
        Dim Tooltips As Dev.Shl.DocumentEditor.Components.ToolTip
#End Region

        Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            Me.DocumentEditor1.OnTextChangedDelayed(Me.DocumentEditor1.Range)
        End Sub

        Private Sub DocumentEditor1_Load(sender As Object, e As EventArgs) Handles DocumentEditor1.Load
            DocumentEditor1.DescriptionFile = String.Format("{0}/styles/ShellScript.xml", My.Application.Info.DirectoryPath)
            AutoComplete = New Components.AutoComplete(Me)
            Tooltips = New Components.ToolTip
            DocumentEditor1.Text = ""
        End Sub

        Private Sub DocumentEditor1_TextChangedDelayed(sender As Object, e As Pavel.CodeEditor.TextChangedEventArgs) Handles DocumentEditor1.TextChangedDelayed
            Me.DocumentEditor1.Range.ClearFoldingMarkers()
            Dim currentIndent As Integer = 0
            Dim lastNonEmptyLine As Integer = 0
            For i As Integer = 0 To Me.DocumentEditor1.LinesCount - 1
                Dim line As Line = Me.DocumentEditor1(i)
                Dim spacesCount As Integer = line.StartSpacesCount
                If spacesCount <> line.Count Then
                    If currentIndent < spacesCount Then
                        Me.DocumentEditor1(lastNonEmptyLine).FoldingStartMarker = "m" + currentIndent.ToString()
                    Else
                        If currentIndent > spacesCount Then
                            Me.DocumentEditor1(lastNonEmptyLine).FoldingEndMarker = "m" + spacesCount.ToString()
                        End If
                    End If
                    currentIndent = spacesCount
                    lastNonEmptyLine = i
                End If
            Next
        End Sub

        Public Sub SaveFile(Optional path As String = "")
            If String.IsNullOrEmpty(path) Then
                path = DocumentFile
            End If

            Call FileIO.FileSystem.WriteAllText(path, DocumentEditor1.Text, False)
        End Sub

        Public Sub LoadFile(path As String)
            DocumentEditor1.Text = FileIO.FileSystem.ReadAllText(path)
            DocumentFile = path
        End Sub

        Private Sub DocumentEditor1_ToolTipNeeded(sender As Object, e As ToolTipNeededEventArgs) Handles DocumentEditor1.ToolTipNeeded
            If Not String.IsNullOrEmpty(e.HoveredWord) Then
                Dim Msg As String = Tooltips.GetTooltipInfo(e.HoveredWord, Me._realtimeCompiled)

                If String.IsNullOrEmpty(Msg) Then
                    Return
                End If

                e.ToolTipTitle = e.HoveredWord
                e.ToolTipText = Msg
            End If
        End Sub
    End Class
End Namespace
Imports Microsoft.VisualBasic.Scripting.ShoalShell.Interpreter.LDM.Expressions
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Tagging
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Scripting.ShoalShell.Interpreter.Parser.TextTokenliser
Imports Microsoft.VisualBasic.Linq

Namespace OokLanguage

    Public NotInheritable Class OokTokenTagger : Implements ITagger(Of OokTokenTag)

        ReadOnly _buffer As ITextBuffer
        ReadOnly _ookTypes As IDictionary(Of String, ExpressionTypes)

        Public Sub New(ByVal buffer As ITextBuffer)
            _buffer = buffer
            _ookTypes = New Dictionary(Of String, ExpressionTypes)
            _ookTypes("die") = ExpressionTypes.Die
            _ookTypes("call") = ExpressionTypes.FunctionCalls
            _ookTypes("goto") = ExpressionTypes.GoTo
            _ookTypes("var") = ExpressionTypes.VariableDeclaration
            _ookTypes("dim") = ExpressionTypes.VariableDeclaration
            _ookTypes("if") = ExpressionTypes.If
            _ookTypes("cd") = ExpressionTypes.CD
        End Sub

        Public Function GetTags(ByVal spans As NormalizedSnapshotSpanCollection) As IEnumerable(Of ITagSpan(Of OokTokenTag)) Implements ITagger(Of OokTokenTag).GetTags
            Dim tags As New List(Of TagSpan(Of OokTokenTag))

            For Each curSpan As SnapshotSpan In spans
                Dim containingLine As ITextSnapshotLine = curSpan.Start.GetContainingLine()
                Dim curLoc As Integer = containingLine.Start.Position
                Dim Parser = New MSLTokens().Parsing(containingLine.GetText())
                Dim tokens() As String = Parser.Tokens.ToArray(Function(x) x.GetTokenValue.ToLower)

                For Each ookToken As String In tokens
                    If _ookTypes.ContainsKey(ookToken) Then
                        Dim tokenSpan = New SnapshotSpan(curSpan.Snapshot, New Span(curLoc, ookToken.Length))
                        If tokenSpan.IntersectsWith(curSpan) Then
                            tags.Add(New TagSpan(Of OokTokenTag)(tokenSpan, New OokTokenTag(_ookTypes(ookToken))))
                        End If
                    End If
                    'add an extra char location because of the space
                    curLoc += ookToken.Length + 1
                Next ookToken
            Next curSpan

            Return tags
        End Function

        Public Event TagsChanged(ByVal sender As Object, ByVal e As SnapshotSpanEventArgs) Implements ITagger(Of OokTokenTag).TagsChanged

    End Class
End Namespace
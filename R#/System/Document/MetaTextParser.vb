Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text

Namespace Development

    Module MetaTextParser

        <Extension>
        Public Function ParseTagData(lines As IEnumerable(Of String), Optional strict As Boolean = True) As Dictionary(Of String, String)
            Dim lastTag As String = Nothing
            Dim index As New Dictionary(Of String, String)

            For Each line As String In lines
                Call ParserLoopStep(line, lastTag, index, strict)
            Next

            For Each name As String In index.Keys.ToArray
                index(name) = index(name).Trim(ASCII.CR, ASCII.LF)
            Next

            Return index
        End Function

        Private Sub ParserLoopStep(line As String, ByRef lastTag$, index As Dictionary(Of String, String), strict As Boolean)
            Dim tag As NamedValue(Of String) = line.GetTagValue(":", trim:=True)
            Dim continuteLine As String
            Dim valueStr As String

            If tag.Name.StringEmpty Then
                If lastTag.StringEmpty Then
                    If strict Then
                        Throw New SyntaxErrorException("invalid content format of the 'DESCRIPTION' meta data file!")
                    Else
                        lastTag = ""
                        GoTo Write
                    End If
                End If
Write:
                If index.ContainsKey(lastTag) Then
                    continuteLine = index(lastTag) & vbCrLf & line
                    index(lastTag) = continuteLine
                Else
                    valueStr = index.TryGetValue(lastTag)
                    continuteLine = valueStr & vbCrLf & line
                    index(lastTag) = continuteLine
                End If
            Else
                lastTag = tag.Name
                index(lastTag) = tag.Value
            End If
        End Sub
    End Module
End Namespace
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Runtime.Components

    ''' <summary>
    ''' An Rscript source wrapper
    ''' </summary>
    Public Class Rscript

        ''' <summary>
        ''' If the script is load from a text file, then property value of <see cref="source"/> is the file location
        ''' Otherwise this property is value nothing
        ''' </summary>
        ''' <returns></returns>
        Public Property source As String

        ''' <summary>
        ''' The script text
        ''' </summary>
        ''' <returns></returns>
        Public Property script As String

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Get language <see cref="Scanner"/> tokens
        ''' </summary>
        ''' <returns></returns>
        Public Function GetTokens() As Token()
            Return New Scanner(script).GetTokens.ToArray
        End Function

        Public Function GetSourceDirectory() As String
            If source.StringEmpty Then
                Return App.CurrentDirectory
            Else
                Return source.ParentPath
            End If
        End Function

        Public Shared Function FromFile(path As String) As Rscript
            Return New Rscript With {
                .source = path.GetFullPath,
                .script = .source.ReadAllText
            }
        End Function

        Public Shared Function FromText(text As String) As Rscript
            Return New Rscript With {
                .source = Nothing,
                .script = text
            }
        End Function

        Public Function GetRawText(tokenSpan As IEnumerable(Of Token)) As String
            With tokenSpan.OrderBy(Function(t) t.span.start).ToArray
                Dim left = .First.span.start
                Dim right = .Last.span.stops

                Return script.Substring(left, right - left)
            End With
        End Function

        Public Function GetRawText(span As IntRange) As String
            Return script.Substring(span.Min, span.Length)
        End Function

        Public Overrides Function ToString() As String
            If source.StringEmpty Then
                Return "<in_memory> " & script
            Else
                Return $"<{source.FileName}> " & script.LineTokens.First & "..."
            End If
        End Function
    End Class
End Namespace
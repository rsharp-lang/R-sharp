
Imports Microsoft.VisualBasic.Text

Namespace Runtime.Internal.Object.Utils

    ''' <summary>
    ''' table row object for save dataframe to file
    ''' </summary>
    Public Class TableRow

        Friend cells As String()

        Public Overrides Function ToString() As String
            Return ToString(",")
        End Function

        Public Overloads Function ToString(format As String) As String
            Return cells _
                .Select(Function(str)
                            If str.IndexOfAny({","c, ASCII.TAB}) > -1 Then
                                Return $"""{str}"""
                            Else
                                Return str
                            End If
                        End Function) _
                .JoinBy(format)
        End Function
    End Class
End Namespace
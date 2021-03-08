Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    Public Class RuntimeError : Inherits VisualBasicAppException

        Public Sub New(message As Message)
            MyBase.New(GetMessages(message).JoinBy("; "), caller:=SafeGetSource(message))
        End Sub

        Private Shared Function SafeGetSource(msg As Message) As String
            If msg.source Is Nothing Then
                Return "<Unknown>"
            Else
                Return msg.source.ToString
            End If
        End Function

        Private Shared Iterator Function GetMessages(msg As Message) As IEnumerable(Of String)
            Dim i As i32 = 1

            For Each line As String In msg
                Yield $"{++i}. {line}"
            Next
        End Function
    End Class
End Namespace
Namespace Runtime

    ''' <summary>
    ''' A signal object for make R# exit the script executation
    ''' </summary>
    Public Class RExit

        ''' <summary>
        ''' the program exit status code
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property exit_code As Integer

        Sub New(status As Integer)
            exit_code = status
        End Sub

        Public Overrides Function ToString() As String
            Return $"exit({exit_code});"
        End Function

    End Class
End Namespace
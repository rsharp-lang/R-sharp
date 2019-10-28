Namespace Runtime

    Public Class RMethodInfo

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String

        Public Function Invoke(envir As Environment, ParamArray arguments As Object()) As Object
            envir = New Environment(envir, stackTag:=Me.name)
            Throw New NotImplementedException
        End Function
    End Class
End Namespace
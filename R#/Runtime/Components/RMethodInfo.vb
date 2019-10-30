Namespace Runtime

    Public Class RMethodInfo : Implements RFunction

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String Implements RFunction.name
        Public ReadOnly Property method As [Delegate]

        Sub New(name$, closure As [Delegate])
            Me.name = name
            Me.method = closure
        End Sub

        Public Function Invoke(envir As Environment, arguments As Object()) As Object Implements RFunction.Invoke
            envir = New Environment(envir, stackTag:=Me.name)
            Throw New NotImplementedException
        End Function
    End Class

    Public Interface RFunction

        ReadOnly Property name As String

        Function Invoke(envir As Environment, arguments As Object()) As Object

    End Interface
End Namespace
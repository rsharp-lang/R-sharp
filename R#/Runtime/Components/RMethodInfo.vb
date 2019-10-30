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
            Dim result As Object

            envir = New Environment(envir, stackTag:=Me.name)
            result = method.Method.Invoke(method.Target, arguments)

            Return result
        End Function
    End Class

    Public Interface RFunction

        ''' <summary>
        ''' 函数名
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property name As String

        ''' <summary>
        ''' 执行当前的这个函数对象然后获取得到结果值
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        Function Invoke(envir As Environment, arguments As Object()) As Object

    End Interface
End Namespace
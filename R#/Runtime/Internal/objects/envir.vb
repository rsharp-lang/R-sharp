Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Internal

    ''' <summary>
    ''' 主要是针对<see cref="ClosureExpression"/>
    ''' </summary>
    Public Class envir

        Public ReadOnly Property envir As Environment
        Public ReadOnly Property closure As ClosureExpression

        Sub New(envir As Environment, closure As ClosureExpression)
            Me.envir = envir
            Me.closure = closure
        End Sub

        Public Function Invoke() As Object
            Return closure.Evaluate(envir)
        End Function

    End Class
End Namespace
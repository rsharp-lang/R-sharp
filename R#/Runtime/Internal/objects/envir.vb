Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Runtime.Internal

    ''' <summary>
    ''' 主要是针对<see cref="ClosureExpression"/>
    ''' </summary>
    Public Class envir

        Public ReadOnly Property envir As Environment
        Public ReadOnly Property closure As ClosureExpression
        Public ReadOnly Property [declare] As Expression

        Sub New(envir As Environment, closure As ClosureExpression, [declare] As Expression)
            Me.envir = envir
            Me.closure = closure
            Me.declare = [declare]
        End Sub

        Public Function Invoke() As Object
            Return closure.Evaluate(envir)
        End Function

        Public Overrides Function ToString() As String
            Return [declare].ToString
        End Function

    End Class
End Namespace
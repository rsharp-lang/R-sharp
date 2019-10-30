Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' 其实就是一段拥有自己的<see cref="Environment"/>的没有名称的匿名函数
    ''' </summary>
    Public Class ClosureExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Sub New()

        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
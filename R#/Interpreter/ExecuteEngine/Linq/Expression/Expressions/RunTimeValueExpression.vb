Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' 这个是为了解决R#脚本中的表达式对象与Linq脚本中的表达式对象的不兼容问题创建的
    ''' </summary>
    Public Class RunTimeValueExpression : Inherits Expression

        Friend ReadOnly R As RExpression

        Public Overrides ReadOnly Property name As String
            Get
                Return $"R#: {R}"
            End Get
        End Property

        Sub New(value As RExpression)
            R = value
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Return R.Evaluate(context)
        End Function

        Public Overrides Function ToString() As String
            Return R.ToString
        End Function
    End Class
End Namespace
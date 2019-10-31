Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' 只允许简单的表达式出现在这里
    ''' 并且参数也只允许出现一个
    ''' </summary>
    Public Class DeclareLambdaFunction : Inherits Expression
        Implements RFunction

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public ReadOnly Property name As String Implements RFunction.name

        Dim parameter As DeclareNewVariable
        Dim closure As ClosureExpression

        ''' <summary>
        ''' 只允许拥有一个参数，并且只允许出现一行代码
        ''' </summary>
        ''' <param name="tokens"></param>
        Sub New(tokens As IEnumerable(Of Token))
            With tokens.ToArray
                name = "[lambda: " & .Select(Function(t) t.text).JoinBy(" ") & "]"
                parameter = New DeclareNewVariable(tokens(Scan0))
                closure = ClosureExpression.ParseExpressionTree(.Skip(2))
            End With
        End Sub

        ''' <summary>
        ''' 返回的是一个<see cref="RFunction"/>
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Me
        End Function

        Public Function Invoke(parent As Environment, arguments() As Object) As Object Implements RFunction.Invoke
            Using envir As New Environment(parent, name)

            End Using
        End Function
    End Class
End Namespace
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.Expression

Namespace Runtime.CodeDOM

    ''' <summary>
    ''' The very base expression in the R# language
    ''' </summary>
    Public Class PrimitiveExpression

        Public Overridable Function Evaluate(envir As Environment) As (value As Object, Type As TypeCodes)

        End Function
    End Class

    ''' <summary>
    ''' The expression which can produce the values
    ''' </summary>
    Public Class ValueExpression : Inherits PrimitiveExpression

        ReadOnly tree As Func(Of Environment, SimpleExpression)

        Sub New(tokens As IEnumerable(Of Token(Of LanguageTokens)))
            tree = New Pointer(Of Token(Of LanguageTokens))(tokens).TryParse
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As (value As Object, Type As TypeCodes)
            Dim out = tree(envir).Evaluate(envir)
            Return out
        End Function
    End Class
End Namespace
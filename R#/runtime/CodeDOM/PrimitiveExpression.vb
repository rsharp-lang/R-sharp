Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp

''' <summary>
''' The very base expression in the R# language
''' </summary>
Public Class PrimitiveExpression

    Public Overridable Function Evaluate(envir As Environment) As Object

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

    Public Overrides Function Evaluate(envir As Environment) As Object
        With tree(envir)
            Dim value As Object = .Evaluate(envir)
            Return value
        End With
    End Function
End Class
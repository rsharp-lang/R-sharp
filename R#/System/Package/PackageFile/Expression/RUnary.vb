Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File.Expressions

    Public Class RUnary : Inherits RExpression

        Public Property [operator] As String
        Public Property expression As JSONNode

        Public Sub New()
        End Sub

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            If [operator] = "!" Then
                Return New UnaryNot(expression.GetExpression(desc))
            Else
                Throw New NotImplementedException
            End If
        End Function
    End Class
End Namespace
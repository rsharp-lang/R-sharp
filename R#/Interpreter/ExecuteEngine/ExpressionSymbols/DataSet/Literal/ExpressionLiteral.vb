Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class ExpressionLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.formula
            End Get
        End Property

        Dim expression As Expression

        Sub New(expression As Expression)
            Me.expression = expression
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Expression
        End Function

        Public Overrides Function ToString() As String
            Return $"~<{expression}>"
        End Function
    End Class
End Namespace
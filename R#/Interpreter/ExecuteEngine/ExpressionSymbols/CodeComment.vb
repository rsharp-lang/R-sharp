Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class CodeComment : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.NA
            End Get
        End Property

        Dim comment$

        Sub New(comment As Token)
            Me.comment = comment.text
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return comment
        End Function
    End Class
End Namespace
Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Inspector

    Public Class ExpressionFormatter

        Dim text As TextWriter
        Dim indent As Integer = 0

        Sub New(text As TextWriter)
            Me.text = text
        End Sub

        Public Sub WriteScript(expr As Expression)
            If TypeOf expr Is ValueAssignExpression Then

            End If

        End Sub



        Public Shared Sub WriteScript(expr As Expression, text As TextWriter)
            Call New ExpressionFormatter(text).WriteScript(expr)
        End Sub

    End Class
End Namespace
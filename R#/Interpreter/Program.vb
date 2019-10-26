Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    Public Class Program

        Public Property expressionQueue As Expression()

        Sub New()
        End Sub

        Public Function Execute(envir As Environment) As Object
            Dim last As Object = Nothing

            For Each expression As Expression In expressionQueue
                last = expression.Evaluate(envir)
            Next

            Return last
        End Function
    End Class
End Namespace
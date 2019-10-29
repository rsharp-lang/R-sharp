Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    Public Class Program

        Public Property execQueue As Expression()

        Sub New()
        End Sub

        Public Function Execute(envir As Environment) As Object
            Dim last As Object = Nothing

            For Each expression As Expression In execQueue
                last = expression.Evaluate(envir)

                ' return keyword will break the function
                If TypeOf expression Is ReturnValue Then
                    Exit For
                End If
            Next

            Return last
        End Function
    End Class
End Namespace
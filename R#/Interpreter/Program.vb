Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
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

                If last.GetType Is GetType(Message) Then
                    If DirectCast(last, Message).MessageLevel = MSG_TYPES.ERR Then
                        ' how to throw error?
                    ElseIf DirectCast(last, Message).MessageLevel = MSG_TYPES.DEBUG Then
                    ElseIf DirectCast(last, Message).MessageLevel = MSG_TYPES.WRN Then
                    Else

                    End If
                ElseIf TypeOf expression Is ReturnValue Then
                    ' return keyword will break the function
                    Exit For
                End If
            Next

            Return last
        End Function
    End Class
End Namespace
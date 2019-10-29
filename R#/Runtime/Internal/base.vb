Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language

Namespace Runtime.Internal

    Public Module base

        Public Function [stop](message$(), envir As Environment) As Object
            Dim frames As New List(Of StackFrame)
            Dim parent As Environment = envir

            Do While Not parent Is Nothing
                frames += New StackFrame With {
                    .Method = New Method With {
                        .Method = parent.stackTag
                    }
                }
                parent = parent.parent
            Loop

            Return New Message With {
                .Message = message,
                .MessageLevel = MSG_TYPES.ERR,
                .StackTrace = frames
            }
        End Function

        Public Function print(x As Object) As Object
            Call base.printInternal(x, "")
            Return x
        End Function

        Private Sub printInternal(x As Object, listPrefix$)
            Dim valueType As Type = x.GetType
            Dim isString As Boolean = valueType Is GetType(String) OrElse valueType Is GetType(String())
            Dim toString = Function(o As Object) As String
                               If isString Then
                                   Return $"""{Scripting.ToString(o, "NULL")}"""
                               Else
                                   Return Scripting.ToString(o, "NULL")
                               End If
                           End Function

            If x.GetType.IsInheritsFrom(GetType(Array)) Then
                Dim xVec As Array = DirectCast(x, Array)
                Dim stringVec = From element As Object In xVec.AsQueryable Select toString(element)

                Call Console.WriteLine($"[{xVec.Length}] " & stringVec.JoinBy(vbTab))
            ElseIf x.GetType Is GetType(Dictionary(Of String, Object)) Then
                For Each slot In DirectCast(x, Dictionary(Of String, Object))
                    Dim key$ = slot.Key

                    If key.IsPattern("\d+") Then
                        key = $"{listPrefix}[[{slot.Key}]]"
                    Else
                        key = $"{listPrefix}${slot.Key}"
                    End If

                    Call Console.WriteLine(key)
                    Call base.printInternal(slot.Value, key & "$")
                    Call Console.WriteLine()
                Next
            Else
                Call Console.WriteLine("[1] " & toString(x))
            End If
        End Sub
    End Module
End Namespace
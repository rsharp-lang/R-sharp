Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime.Internal

    ''' <summary>
    ''' 在这个模块之中仅包含有最基本的数据操作函数
    ''' </summary>
    Public Module base

        Public Function names([object] As Object, namelist As Object) As Object
            If namelist Is Nothing OrElse Runtime.asVector(Of Object)(namelist).Length = 0 Then
                ' get names
            Else
                ' set names

            End If
        End Function

        Public Function [stop](message As Object, envir As Environment) As Message
            Return createMessageInternal(message, envir, level:=MSG_TYPES.ERR)
        End Function

        Private Function createMessageInternal(messages As Object, envir As Environment, level As MSG_TYPES) As Message
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
                .Message = Runtime.asVector(Of Object)(messages) _
                    .AsObjectEnumerator _
                    .SafeQuery _
                    .Select(Function(o) Scripting.ToString(o, "NULL")) _
                    .ToArray,
                .MessageLevel = level,
                .EnvironmentStack = frames,
                .Trace = devtools.ExceptionData.GetCurrentStackTrace
            }
        End Function

        Public Function warning(message As Object, envir As Environment) As Message
            Return createMessageInternal(message, envir, level:=MSG_TYPES.WRN)
        End Function

        Public Function cat(values As Object, file As String, sep As String) As Object
            Dim strs = Runtime.asVector(Of Object)(values) _
                .AsObjectEnumerator _
                .Select(Function(o) Scripting.ToString(o, "")) _
                .JoinBy(sep) _
                .DoCall(AddressOf sprintf)

            If Not file.StringEmpty Then
                Call strs.SaveTo(file)
            Else
                Call Console.Write(strs)
            End If

            Return strs
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
                    Call base.printInternal(slot.Value, key)
                    Call Console.WriteLine()
                Next
            Else
                Call Console.WriteLine("[1] " & toString(x))
            End If
        End Sub

        Public Function lapply(sequence As Object, apply As RFunction, envir As Environment) As Object
            If sequence.GetType Is GetType(Dictionary(Of String, Object)) Then
                Return DirectCast(sequence, Dictionary(Of String, Object)) _
                    .ToDictionary(Function(d) d.Key,
                                  Function(d)
                                      Return apply.Invoke(envir, {d.Value})
                                  End Function)
            Else
                Return Runtime.asVector(Of Object)(sequence) _
                    .AsObjectEnumerator _
                    .SeqIterator _
                    .ToDictionary(Function(i) $"[[{i.i}]]",
                                  Function(d)
                                      Return apply.Invoke(envir, {d.value})
                                  End Function)
            End If
        End Function

        Public Function sapply(sequence As Object, apply As RFunction, envir As Environment) As Object
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter

    Public Class Program : Implements IEnumerable(Of Expression)

        Friend execQueue As Expression()

        Sub New()
        End Sub

        Public Function Execute(envir As Environment) As Object
            Dim last As Object = Nothing

            For Each expression As Expression In execQueue
                last = expression.Evaluate(envir)

                If TypeOf expression Is ReturnValue Then
                    ' return keyword will break the function
                    last = New ReturnValue(New Literal With {.value = last})
                End If

                If Not last Is Nothing Then
                    If last.GetType Is GetType(Message) Then
                        If DirectCast(last, Message).MessageLevel = MSG_TYPES.ERR Then
                            ' how to throw error?
                            Return last
                        ElseIf DirectCast(last, Message).MessageLevel = MSG_TYPES.DEBUG Then
                        ElseIf DirectCast(last, Message).MessageLevel = MSG_TYPES.WRN Then
                        Else

                        End If
                    ElseIf last.GetType Is GetType(IfBranch.IfPromise) Then
                        envir.ifPromise.Add(last)
                        last = DirectCast(last, IfBranch.IfPromise).Value

                        If envir.ifPromise.Last.Result Then
                            If Not last Is Nothing AndAlso last.GetType Is GetType(ReturnValue) Then
                                Exit For
                            End If
                        End If
                    End If
                End If
            Next

            Return last
        End Function

        Public Overrides Function ToString() As String
            Return execQueue.Select(Function(exp) exp.ToString & ";").JoinBy(vbCrLf)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function CreateProgram(tokens As IEnumerable(Of Token)) As Program
            Return New Program With {
                .execQueue = tokens.ToArray _
                    .GetExpressions _
                    .ToArray
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function isException(result As Object) As Boolean
            If result Is Nothing Then
                Return False
            ElseIf result.GetType Is GetType(Message) Then
                Return DirectCast(result, Message).MessageLevel = MSG_TYPES.ERR
            Else
                Return False
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function BuildProgram(scriptText As String) As Program
            Return New Scanner(scriptText) _
                .GetTokens _
                .DoCall(AddressOf CreateProgram)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Expression) Implements IEnumerable(Of Expression).GetEnumerator
            For Each line As Expression In execQueue
                Yield line
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
Imports System.Runtime.CompilerServices
Imports System.Threading

Module ScriptTask

    Private Structure VoidTypeStruct
    End Structure

    <Extension>
    Public Function TimeoutAfter(task As Task, millisecondsTimeout As Integer) As Task
        If task.IsCompleted OrElse (millisecondsTimeout = Timeout.Infinite) Then
            Return task
        End If

        Dim tcs As New TaskCompletionSource(Of VoidTypeStruct)()

        If millisecondsTimeout = 0 Then
            tcs.SetException(New TimeoutException())
            Return tcs.Task
        End If

        Dim timer As New Timer(AddressOf [Throw], tcs, millisecondsTimeout, Timeout.Infinite)
        Dim state = Tuple.Create(timer, tcs)

        Call task.ContinueWith(
            continuationAction:=AddressOf ContinuteTask,
            state:=state,
            cancellationToken:=CancellationToken.None,
            continuationOptions:=TaskContinuationOptions.ExecuteSynchronously,
            scheduler:=TaskScheduler.[Default]
        )

        Return tcs.Task
    End Function

    Private Sub ContinuteTask(antecedent As Task, state As Object)
        Dim tuple = CType(state, Tuple(Of Timer, TaskCompletionSource(Of VoidTypeStruct)))
        Dim timer As Timer = tuple.Item1

        Call timer.Dispose()
        Call antecedent.MarshalTaskResults(tuple.Item2)
    End Sub

    <Extension>
    Private Sub MarshalTaskResults(Of TResult)(source As Task, proxy As TaskCompletionSource(Of TResult))
        Select Case source.Status
            Case TaskStatus.Faulted
                proxy.TrySetException(source.Exception)
            Case TaskStatus.Canceled
                proxy.TrySetCanceled()
            Case TaskStatus.RanToCompletion
                Dim castedSource As Task(Of TResult) = TryCast(source, Task(Of TResult))

                If castedSource Is Nothing Then
                    proxy.TrySetResult(Nothing)
                Else
                    proxy.TrySetResult(castedSource.Result)
                End If
        End Select
    End Sub

    Private Sub [Throw](state As Object)
        CType(state, TaskCompletionSource(Of VoidTypeStruct)).TrySetException(New TimeoutException())
    End Sub
End Module

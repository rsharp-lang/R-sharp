Imports System
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' https://github.com/alicommit-malp/easy-async-cancel
''' </summary>
Friend Module TaskCancellationInternals
    Public Async Function CancelWithInternal(Of T)(task As Task(Of T), cancellationToken As CancellationToken, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Dim tcs = New TaskCompletionSource(Of Boolean)()
        Using cancellationToken.Register(Sub(s) CType(s, TaskCompletionSource(Of Boolean)).TrySetResult(True), tcs)
            If task IsNot Await Tasks.Task.WhenAny(task, tcs.Task) Then
                If Not swallowCancellationException Then
                    Throw New OperationCanceledException(cancellationToken)
                Else
                    Return Nothing
                End If
            End If
        End Using
        Return Await task
    End Function

    Public Async Function CancelWithInternal(Of T)(task As Task(Of T), cancellationToken As CancellationToken, message As String, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Dim tcs = New TaskCompletionSource(Of Boolean)()
        Using cancellationToken.Register(Sub(s) CType(s, TaskCompletionSource(Of Boolean)).TrySetResult(True), tcs)
            If task IsNot Await Tasks.Task.WhenAny(task, tcs.Task) Then
                If Not swallowCancellationException Then
                    Throw New OperationCanceledException(message, cancellationToken)
                Else
                    Return Nothing
                End If
            End If
        End Using
        Return Await task
    End Function


    Public Async Function CancelWithInternal(task As Task, cancellationToken As CancellationToken, Optional swallowCancellationException As Boolean = False) As Task
        Dim tcs = New TaskCompletionSource(Of Boolean)()
        Using cancellationToken.Register(Sub(s) CType(s, TaskCompletionSource(Of Boolean)).TrySetResult(True), tcs)
            If task IsNot Await Task.WhenAny(task, tcs.Task) Then
                If Not swallowCancellationException Then
                    Throw New OperationCanceledException(cancellationToken)
                Else
                    Return
                End If
            End If
        End Using
        Await task
    End Function


    Public Async Function CancelWithInternal(task As Task, cancellationToken As CancellationToken, message As String, Optional swallowCancellationException As Boolean = False) As Task
        Dim tcs = New TaskCompletionSource(Of Boolean)()
        Using cancellationToken.Register(Sub(s) CType(s, TaskCompletionSource(Of Boolean)).TrySetResult(True), tcs)
            If task IsNot Await Task.WhenAny(task, tcs.Task) Then
                If Not swallowCancellationException Then
                    Throw New OperationCanceledException(message, cancellationToken)
                Else
                    Return
                End If
            End If
        End Using
        Await task
    End Function
End Module

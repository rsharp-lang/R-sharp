Imports System
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Runtime.CompilerServices

Public Module TaskCancellationExtension
    ''' <summary>
    ''' Add cancellation functionality to <see cref="Task"/> of <typeparamref name="T"/>
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="cancellationToken"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <typeparam name="T"></typeparam>
    ''' <returns><see cref="Task"/></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    <Obsolete("User CancelWith instead")>
    Public Function CancelAfter(Of T)(task As Task(Of T), cancellationToken As CancellationToken, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Return CancelWithInternal(task, cancellationToken, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Task T 
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="cancellationToken"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelWith(Of T)(task As Task(Of T), cancellationToken As CancellationToken, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Return CancelWithInternal(task, cancellationToken, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Task T with exception message 
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="cancellationToken"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <param name="message"></param>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    <Obsolete("User CancelWith instead")>
    Public Function CancelAfter(Of T)(task As Task(Of T), cancellationToken As CancellationToken, message As String, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Return CancelWithInternal(task, cancellationToken, message, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Task T with exception message 
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="cancellationToken"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <param name="message"></param>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelWith(Of T)(task As Task(Of T), cancellationToken As CancellationToken, message As String, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Return CancelWithInternal(task, cancellationToken, message, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Tasks 
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="cancellationToken"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    <Obsolete("User CancelWith instead")>
    Public Function CancelAfter(task As Task, cancellationToken As CancellationToken, Optional swallowCancellationException As Boolean = False) As Task
        Return CancelWithInternal(task, cancellationToken, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Tasks 
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="cancellationToken"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelWith(task As Task, cancellationToken As CancellationToken, Optional swallowCancellationException As Boolean = False) As Task
        Return CancelWithInternal(task, cancellationToken, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Tasks with exception message 
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="cancellationToken"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <param name="message"></param>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    <Obsolete("User CancelWith instead")>
    Public Function CancelAfter(task As Task, cancellationToken As CancellationToken, message As String, Optional swallowCancellationException As Boolean = False) As Task
        Return CancelWithInternal(task, cancellationToken, message, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Tasks with exception message 
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="cancellationToken"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <param name="message"></param>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelWith(task As Task, cancellationToken As CancellationToken, message As String, Optional swallowCancellationException As Boolean = False) As Task
        Return CancelWithInternal(task, cancellationToken, message, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Task T
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="milliseconds"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelAfter(Of T)(task As Task(Of T), milliseconds As Integer, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Dim cts = New CancellationTokenSource()
        cts.CancelAfter(milliseconds)
        Return CancelWithInternal(task, cts.Token, swallowCancellationException)
    End Function


    ''' <summary>
    ''' add cancellation functionality to Task T with exception message
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="milliseconds"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <param name="message"></param>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelAfter(Of T)(task As Task(Of T), milliseconds As Integer, message As String, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Dim cts = New CancellationTokenSource()
        cts.CancelAfter(milliseconds)
        Return CancelWithInternal(task, cts.Token, message, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Task
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="milliseconds"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelAfter(task As Task, milliseconds As Integer, Optional swallowCancellationException As Boolean = False) As Task
        Dim cts = New CancellationTokenSource()
        cts.CancelAfter(milliseconds)
        Return CancelWithInternal(task, cts.Token, swallowCancellationException)
    End Function


    ''' <summary>
    ''' add cancellation functionality to Task with exception message
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="milliseconds"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <param name="message"></param>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelAfter(task As Task, milliseconds As Integer, message As String, Optional swallowCancellationException As Boolean = False) As Task
        Dim cts = New CancellationTokenSource()
        cts.CancelAfter(milliseconds)
        Return CancelWithInternal(task, cts.Token, message, swallowCancellationException)
    End Function


    ''' <summary>
    ''' add cancellation functionality to Task T
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="timeSpan"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelAfter(Of T)(task As Task(Of T), timeSpan As TimeSpan, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Dim cts = New CancellationTokenSource()
        cts.CancelAfter(timeSpan)
        Return CancelWithInternal(task, cts.Token, swallowCancellationException)
    End Function


    ''' <summary>
    ''' add cancellation functionality to Task T with exception message
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="timeSpan"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <param name="message"></param>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelAfter(Of T)(task As Task(Of T), timeSpan As TimeSpan, message As String, Optional swallowCancellationException As Boolean = False) As Task(Of T)
        Dim cts = New CancellationTokenSource()
        cts.CancelAfter(timeSpan)
        Return CancelWithInternal(task, cts.Token, message, swallowCancellationException)
    End Function

    ''' <summary>
    ''' add cancellation functionality to Task
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="timeSpan"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelAfter(task As Task, timeSpan As TimeSpan, Optional swallowCancellationException As Boolean = False) As Task
        Dim cts = New CancellationTokenSource()
        cts.CancelAfter(timeSpan)
        Return CancelWithInternal(task, cts.Token, swallowCancellationException)
    End Function


    ''' <summary>
    ''' add cancellation functionality to Task with exception message
    ''' </summary>
    ''' <param name="task"></param>
    ''' <param name="timeSpan"></param>
    ''' <param name="swallowCancellationException">If True the <see cref="OperationCanceledException"/> will be swallowed</param>
    ''' <param name="message"></param>
    ''' <returns></returns>
    ''' <exception cref="OperationCanceledException"></exception>
    <Extension()>
    Public Function CancelAfter(task As Task, timeSpan As TimeSpan, message As String, Optional swallowCancellationException As Boolean = False) As Task
        Dim cts = New CancellationTokenSource()
        cts.CancelAfter(timeSpan)
        Return CancelWithInternal(task, cts.Token, message, swallowCancellationException)
    End Function
End Module

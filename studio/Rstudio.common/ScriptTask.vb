#Region "Microsoft.VisualBasic::93b55eb266d713a58c7e757bb9673baa, R-sharp\studio\Rstudio.common\ScriptTask.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


     Code Statistics:

        Total Lines:   65
        Code Lines:    52
        Comment Lines: 0
        Blank Lines:   13
        File Size:     2.21 KB


    ' Module ScriptTask
    ' 
    '     Function: TimeoutAfter
    ' 
    '     Sub: [Throw], ContinuteTask, MarshalTaskResults
    '     Structure VoidTypeStruct
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

#Region "Microsoft.VisualBasic::7ef89a8ea7e5ccc11d4e2e2045794930, R-sharp\R#\Runtime\Internal\internalInvokes\Linq\parallelApplys.vb"

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


' Code Statistics:

'   Total Lines: 206
'    Code Lines: 150
' Comment Lines: 26
'   Blank Lines: 30
'     File Size: 8.81 KB


'     Module parallelApplys
' 
'         Function: deepCloneContext, parallelList
' 
'         Sub: pushGroupParallelTask, pushParallelTask
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Threads
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports anyObj = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes.LinqPipeline

    Module parallelApplys

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="apply"></param>
        ''' <param name="group"></param>
        ''' <param name="n_threads"></param>
        ''' <param name="verbose"></param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' the returns result value sequence keeps the 
        ''' same order with the input sequence.
        ''' </returns>
        <Extension>
        Friend Function parallelList(list As IDictionary,
                                     apply As RFunction,
                                     group As Integer,
                                     n_threads As Integer,
                                     verbose As Boolean,
                                     envir As Environment) As (names As List(Of String), objects As List(Of Object))

            Dim values As New List(Of (i%, key$, value As Object))
            Dim task_threads As Integer = If(n_threads <= 0, App.CPUCoreNumbers, n_threads)
            Dim println As Action(Of Object) = envir.WriteLineHandler

            If task_threads > App.CPUCoreNumbers Then
                task_threads = App.CPUCoreNumbers
                println($"[warning] the given task threads number({n_threads}) is greater than the CPU core thread number({App.CPUCoreNumbers}), set task threads number to {task_threads}!")
            End If

            Dim host As New ThreadPool(
                maxThread:=task_threads,
                maxQueueSize:=1,
                exceptionCallback:=
                    Sub(name, ex)
                        Call values.Add((0, name, Internal.debug.stop(ex, envir, suppress:=True)))
                    End Sub)

            If group > 1 Then
                Call host.pushGroupParallelTask(list, apply, envir, group, verbose, values)
            Else
                Call host.pushParallelTask(list, apply, envir, verbose, values)
            End If

            Call host.Start()
            Call host.WaitAll(verbose)
            Call host.Dispose()

            If verbose Then
                Call println("all job done!")
            End If

            Dim seq As New List(Of Object)
            Dim names As New List(Of String)

            For Each tuple As (i As Integer, key As String, value As Object) In values _
                .OrderBy(Function(a)
                             ' 20221023 re-order data by index try to make the
                             ' result sequence keeps the same order with the 
                             ' original input sequence
                             Return a.i
                         End Function)

                Call seq.Add(REnv.single(tuple.value))
                Call names.Add(tuple.key)
            Next

            Return (names, seq)
        End Function

        ''' <summary>
        ''' try to make a deep clone of the environment context for run parallel code.
        ''' </summary>
        ''' <param name="env">
        ''' due to the reason of make deep copy of this environment context, 
        ''' so that the global variable value update in the parallel code
        ''' may not effect the environment context in the main thread.
        ''' 
        ''' in this point of view, keep less global variable reference will
        ''' be better in parallel
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function deepCloneContext(env As Environment, tag As String) As Environment
            Dim symbols As New Dictionary(Of String, Symbol)
            Dim funcs As New Dictionary(Of String, Symbol)
            Dim empty As Environment = env.globalEnvironment

            ' modification of the symbol value
            ' may not affect the parent environment context
            '
            ' the later order of the symbol it is, the more close to the root
            ' environment context it does. the lower level symbol with the same
            ' name could overloads the symbol in top level.
            '
            ' so we skip the symbols which we found that there is
            ' already a symbol with the exactly same name in the hash
            ' table
            For Each symbol As Symbol In env.EnumerateAllSymbols
                If Not symbols.ContainsKey(symbol.name) Then
                    Call symbols.Add(symbol.name, symbol)
                End If
            Next
            For Each symbol As Symbol In env.EnumerateAllFunctions
                If Not funcs.ContainsKey(symbol.name) Then
                    Call funcs.Add(symbol.name, symbol)
                End If
            Next

            ' create empty stack to break the 
            ' parallel sync lock in environment
            ' join for initialize the function 
            ' call context
            For Each frame As StackFrame In env.stackTrace.Reverse
                empty = New Environment(empty, frame, isInherits:=False)
            Next

            Dim context As New Environment(
                parent:=empty,
                stackName:=$"parallel_task[{MethodBase.GetCurrentMethod.Name} ~ {tag}]",
                symbols:=symbols.Values,
                funcs:=funcs.Values
            )

            Return context
        End Function

        <Extension>
        Private Sub pushGroupParallelTask(host As ThreadPool,
                                          list As IDictionary,
                                          apply As RFunction,
                                          envir As Environment,
                                          group As Integer,
                                          verbose As Boolean,
                                          values As List(Of (i%, key$, value As Object)))
            Dim key_groups = list.Keys _
                .ToArray(Of Object) _
                .SeqIterator _
                .Split(partitionSize:=group)
            Dim i As i32 = 1

            Call println($"create {key_groups.Length} task groups based on {list.Count} data inputs!")

            ' for each key group
            For Each keys As SeqValue(Of Object)() In key_groups
                ' get task group of current key group
                Dim value_group As SeqValue(Of (Object, Object))() = keys _
                    .Select(Function(xi)
                                Return New SeqValue(Of (Object, Object))(xi.i, (xi.value, list(xi.value)))
                            End Function) _
                    .ToArray
                Dim task_id As Integer = ++i
                Dim env As Environment = envir.deepCloneContext("parallel_task_group_" & task_id)
                Dim queueTaskName As String = "task_group~" & keys.Select(Function(c) anyObj.ToString(c.value)).JoinBy("+")

                If verbose Then
                    Call println($"[task_queue] queue {task_id}...")
                End If

                Call host.RunTask(
                    Sub()
                        If verbose Then
                            Call println($"[task_queue] run {task_id}...")
                        End If

                        Dim result = value_group _
                            .Select(Function(xi)
                                        Return (
                                            i:=xi.i,
                                            key:=anyObj.ToString(xi.value.Item1),
                                            value:=apply.Invoke(env, invokeArgument(xi.value.Item2, xi.i))
                                        )
                                    End Function) _
                            .ToArray

                        If verbose Then
                            Call println($"[task_queue] append to result {task_id}!")
                        End If

                        SyncLock values
                            For Each pop In result
                                Call values.Add(pop)
                            Next
                        End SyncLock

                        If verbose Then
                            Call println($"[task_queue] finish {task_id}!")
                        End If
                    End Sub, name:=queueTaskName)
            Next
        End Sub

        <Extension>
        Private Sub pushParallelTask(host As ThreadPool,
                                     list As IDictionary,
                                     apply As RFunction,
                                     envir As Environment,
                                     verbose As Boolean,
                                     values As List(Of (i%, key$, value As Object)))
            Dim i As i32 = 1
            Dim println = envir.WriteLineHandler
            Dim sizeAll = list.Count

            For Each key As Object In list.Keys
                Dim value As Object = list(key)
                Dim index As Integer = ++i
                Dim keyName As String = anyObj.ToString(key)
                Dim env As Environment = envir.deepCloneContext(keyName)

                Call host.RunTask(
                    Sub()
                        Dim result = (
                            i:=index,
                            key:=keyName,
                            value:=apply.Invoke(env, invokeArgument(value, index))
                        )

                        SyncLock values
                            Call values.Add(result)

                            If verbose Then
                                Call println($"[{(values.Count / sizeAll * 100).ToString("F2")}%] {keyName}, job done!")
                            End If
                        End SyncLock
                    End Sub, name:=keyName)
            Next
        End Sub
    End Module
End Namespace

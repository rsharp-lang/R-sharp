#Region "Microsoft.VisualBasic::cf66410ece61b7af8eaf857ec289c01d, R-sharp\R#\Runtime\Internal\internalInvokes\applys.vb"

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

'   Total Lines: 493
'    Code Lines: 330
' Comment Lines: 88
'   Blank Lines: 75
'     File Size: 20.26 KB


'     Module applys
' 
'         Function: apply, checkInternal, (+2 Overloads) keyNameAuto, lapply, parallelList
'                   parLapply, parSapply, sapply
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Threads
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RObj = SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal.Invokes

    <Package("applys")>
    Module applys

        ''' <summary>
        ''' ### Apply Functions Over Array Margins
        ''' 
        ''' Returns a vector or array or list of values obtained by applying 
        ''' a function to margins of an array or matrix.
        ''' </summary>
        ''' <param name="x">an array, including a matrix.</param>
        ''' <param name="margin">a vector giving the subscripts which the 
        ''' function will be applied over. E.g., for a matrix 1 indicates rows, 
        ''' 2 indicates columns, c(1, 2) indicates rows and columns. Where X has 
        ''' named dimnames, it can be a character vector selecting dimension 
        ''' names.</param>
        ''' <param name="FUN">
        ''' the function to be applied: see ‘Details’. In the case of functions 
        ''' like +, %*%, etc., the function name must be backquoted or quoted.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>If each call to FUN returns a vector of length n, then apply 
        ''' returns an array of dimension c(n, dim(X)[MARGIN]) if ``n &gt; 1``. 
        ''' If n equals 1, apply returns a vector if MARGIN has length 1 and an array 
        ''' of dimension dim(X)[MARGIN] otherwise. If n is 0, the result has length 
        ''' 0 but not necessarily the ‘correct’ dimension.
        '''
        ''' If the calls To FUN Return vectors Of different lengths, apply returns 
        ''' a list Of length prod(Dim(X)[MARGIN]) With Dim Set To MARGIN If this has 
        ''' length greater than one.
        '''
        ''' In all cases the result Is coerced by as.vector to one of the basic 
        ''' vector types before the dimensions are set, so that (for example) factor 
        ''' results will be coerced to a character array.</returns>
        ''' <remarks>
        ''' If X is not an array but an object of a class with a non-null dim value 
        ''' (such as a data frame), apply attempts to coerce it to an array via 
        ''' ``as.matrix`` if it is two-dimensional (e.g., a data frame) or via 
        ''' ``as.array``.
        '''
        ''' FUN Is found by a call to match.fun And typically Is either a function 
        ''' Or a symbol (e.g., a backquoted name) Or a character string specifying 
        ''' a function to be searched for from the environment of the call to apply.
        '''
        ''' Arguments in ``...`` cannot have the same name as any of the other 
        ''' arguments, And care may be needed to avoid partial matching to MARGIN Or 
        ''' FUN. In general-purpose code it Is good practice to name the first three 
        ''' arguments if ... Is passed through: this both avoids Partial matching To 
        ''' MARGIN Or FUN And ensures that a sensible Error message Is given If 
        ''' arguments named X, MARGIN Or FUN are passed through ``...``.
        ''' </remarks>
        <ExportAPI("apply")>
        Public Function apply(x As Object, margin As margins, FUN As Object, Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return New Object() {}
            ElseIf TypeOf x Is dataframe Then
                Return doApply.apply(DirectCast(x, dataframe), margin, FUN, env)
            Else
                Return debug.stop(New InvalidProgramException, env)
            End If
        End Function

        <ExportAPI("parLapply")>
        Public Function parLapply(x As list, FUN As Object,
                                  Optional group As Integer = -1,
                                  Optional n_threads As Integer = -1,
                                  Optional verbose As Boolean? = Nothing,
                                  Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return Nothing
            End If

            Dim check = checkInternal(x, FUN, env)

            If Not TypeOf check Is Boolean Then
                Return check
            End If

            Dim seq As List(Of Object)
            Dim names As List(Of String)
            Dim apply As RFunction = FUN
            Dim result = x.slots.parallelList(apply, group, n_threads, env.verboseOption(opt:=verbose), env)

            seq = result.objects
            names = result.names
            x = New list With {.slots = New Dictionary(Of String, Object)}

            For i As Integer = 0 To names.Count - 1
                If Program.isException(seq(i)) Then
                    Return seq(i)
                Else
                    Call x.add(names(i), seq(i))
                End If
            Next

            Return x
        End Function

        <Extension>
        Private Function parallelList(list As IDictionary,
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

            Dim host As New ThreadPool(task_threads)
            Dim i As i32 = 1

            Call host.Start()

            If group > 1 Then
                Dim key_groups = list.Keys _
                    .ToArray(Of Object) _
                    .SeqIterator _
                    .Split(partitionSize:=group)

                Call println($"create {key_groups.Length} task groups based on {list.Count} data inputs!")

                For Each keys As SeqValue(Of Object)() In key_groups
                    Dim value_group As SeqValue(Of (Object, Object))() = keys _
                        .Select(Function(xi)
                                    Return New SeqValue(Of (Object, Object))(xi.i, (xi.value, list(xi.value)))
                                End Function) _
                        .ToArray
                    Dim task_id As Integer = ++i

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
                                                key:=any.ToString(xi.value.Item1),
                                                value:=apply.Invoke(envir, invokeArgument(xi.value.Item2, xi.i))
                                            )
                                        End Function) _
                                .ToArray

                            SyncLock values
                                For Each pop In result
                                    Call values.Add(pop)
                                Next
                            End SyncLock

                            If verbose Then
                                Call println($"[task_queue] finish {task_id}!")
                            End If
                        End Sub)
                Next
            Else
                For Each key As Object In list.Keys
                    Dim value As Object = list(key)
                    Dim index As Integer = ++i

                    Call host.RunTask(
                        Sub()
                            Dim result = (
                                i:=index,
                                key:=any.ToString(key),
                                value:=apply.Invoke(envir, invokeArgument(value, index))
                            )

                            SyncLock values
                                Call values.Add(result)
                            End SyncLock
                        End Sub)
                Next
            End If

            Call host.WaitAll()
            Call host.Dispose()

            If verbose Then
                Call println("all job done!")
            End If

            Dim seq As New List(Of Object)
            Dim names As New List(Of String)

            For Each tuple As (i As Integer, key As String, value As Object) In values.OrderBy(Function(a) a.i)
                Call seq.Add(REnv.single(tuple.value))
                Call names.Add(tuple.key)
            Next

            Return (names, seq)
        End Function

        ''' <summary>
        ''' parallel sapply
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("parSapply")>
        Public Function parSapply(<RRawVectorArgument>
                                  X As Object,
                                  FUN As Object,
                                  Optional group As Integer = -1,
                                  Optional n_threads As Integer = -1,
                                  Optional verbose As Boolean? = Nothing,
                                  Optional envir As Environment = Nothing) As Object
            If X Is Nothing Then
                Return New Object() {}
            End If

            Dim check = checkInternal(X, FUN, envir)

            If Not TypeOf check Is Boolean Then
                Return check
            End If

            Dim seq As New List(Of Object)
            Dim names As New List(Of String)
            Dim apply As RFunction = FUN

            If X.GetType Is GetType(list) Then
                X = DirectCast(X, list).slots
            End If

            If X.GetType.ImplementInterface(GetType(IDictionary)) Then
                Dim list As IDictionary = DirectCast(X, IDictionary)
                Dim result = list.parallelList(apply, group, n_threads, envir.verboseOption(opt:=verbose), envir)

                names = result.names
                seq = result.objects

                For Each i In seq
                    If Program.isException(i) Then
                        Return i
                    End If
                Next
            Else
                Dim values As IEnumerable(Of Object) = REnv.asVector(Of Object)(X) _
                    .AsObjectEnumerator _
                    .SeqIterator _
                    .AsParallel _
                    .Select(Function(d)
                                Return (d.i, value:=apply.Invoke(envir, invokeArgument(d.value)))
                            End Function) _
                    .OrderBy(Function(i) i.i) _
                    .Select(Function(i)
                                Return i.value
                            End Function)

                For Each value As Object In values
                    If Program.isException(value) Then
                        Return value
                    Else
                        seq.Add(REnv.single(value))
                    End If
                Next
            End If

            Return New RObj.vector(names, seq.ToArray, envir)
        End Function

        Private Function checkInternal(X As Object, FUN As Object, env As Environment) As Object
            If FUN Is Nothing Then
                Return Internal.debug.stop({"Missing apply function!"}, env)
            ElseIf Not FUN.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.debug.stop({"Target is not a function!"}, env)
            End If

            If Program.isException(X) Then
                Return X
            ElseIf Program.isException(FUN) Then
                Return FUN
            ElseIf X Is Nothing Then
                Return Nothing
            Else
                Return True
            End If
        End Function

        ''' <summary>
        ''' # Apply a Function over a List or Vector
        ''' 
        ''' sapply is a user-friendly version and wrapper of lapply by default 
        ''' returning a vector, matrix or, if simplify = "array", an array 
        ''' if appropriate, by applying simplify2array(). sapply(x, f, simplify 
        ''' = FALSE, USE.NAMES = FALSE) is the same as lapply(x, f).
        ''' </summary>
        ''' <param name="X">
        ''' a vector (atomic or list) or an expression object. Other objects 
        ''' (including classed objects) will be coerced by ``base::as.list``.
        ''' </param>
        ''' <param name="FUN">
        ''' the Function to be applied To Each element Of X: see 'Details’. 
        ''' In the case of functions like +, %*%, the function name must be 
        ''' backquoted or quoted.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("sapply")>
        <RApiReturn(GetType(vector))>
        Public Function sapply(<RRawVectorArgument> X As Object, FUN As Object, envir As Environment) As Object
            If X Is Nothing Then
                Return New Object() {}
            End If

            Dim check = checkInternal(X, FUN, envir)
            Dim nameVec As String() = Nothing
            Dim arrayVec As Array

            If Not TypeOf check Is Boolean Then
                Return check
            End If

            Dim apply As RFunction = FUN

            If X.GetType Is GetType(list) Then
                X = DirectCast(X, list).slots
            End If

            If X.GetType.ImplementInterface(GetType(IDictionary)) Then
                Dim list = DirectCast(X, IDictionary)
                Dim seq As New List(Of Object)
                Dim names As New List(Of String)
                Dim value As Object
                Dim i As i32 = 1

                For Each key As Object In list.Keys
                    value = apply.Invoke(envir, InvokeParameter.CreateLiterals(list(key), ++i))

                    If Program.isException(value) Then
                        Return value
                    End If

                    seq.Add(REnv.single(value))
                    names.Add(any.ToString(key))
                Next

                Dim a As Object = TryCastGenericArray(seq.ToArray, envir)
                Dim type As Type

                If TypeOf a Is Message Then
                    type = GetType(Object)
                Else
                    type = a.GetType.GetElementType
                End If

                nameVec = names.ToArray
                arrayVec = DirectCast(a, Array)
            Else
                Dim seq As New List(Of Object)
                Dim value As Object
                Dim argsPreviews As InvokeParameter()
                Dim i As i32 = 1

                For Each d In REnv.asVector(Of Object)(X) _
                    .AsObjectEnumerator

                    argsPreviews = invokeArgument(d, ++i)
                    value = apply.Invoke(envir, argsPreviews)

                    If Program.isException(value) Then
                        Return value
                    Else
                        seq.Add(REnv.single(value))
                    End If
                Next

                Dim a As Object = TryCastGenericArray(seq.ToArray, envir)
                Dim type As Type

                If TypeOf a Is Message Then
                    type = GetType(Object)
                Else
                    type = a.GetType.GetElementType
                End If

                arrayVec = DirectCast(a, Array)
            End If

            arrayVec = REnv.TryCastGenericArray(arrayVec, envir)

            If nameVec.IsNullOrEmpty Then
                Return New RObj.vector(arrayVec, RType.GetRSharpType(arrayVec.GetType.GetElementType))
            Else
                Return New RObj.vector(nameVec, arrayVec, RType.GetRSharpType(arrayVec.GetType.GetElementType), envir)
            End If
        End Function

        ''' <summary>
        ''' # Apply a Function over a List or Vector
        ''' 
        ''' lapply returns a list of the same length as X, each element of 
        ''' which is the result of applying FUN to the corresponding 
        ''' element of X.
        ''' </summary>
        ''' <param name="X">
        ''' a vector (atomic or list) or an expression object. Other objects 
        ''' (including classed objects) will be coerced by ``base::as.list``.
        ''' </param>
        ''' <param name="FUN">
        ''' the Function to be applied To Each element Of X: see 'Details’. 
        ''' In the case of functions like +, %*%, the function name must be 
        ''' backquoted or quoted.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("lapply")>
        Public Function lapply(<RRawVectorArgument> X As Object, FUN As Object,
                               <RRawVectorArgument>
                               Optional names As Object = Nothing,
                               Optional envir As Environment = Nothing) As Object

            If X Is Nothing Then
                Return New list With {.slots = New Dictionary(Of String, Object)}
            End If

            Dim check = checkInternal(X, FUN, envir)

            If Not TypeOf check Is Boolean Then
                Return check
            ElseIf X.GetType Is GetType(list) Then
                X = DirectCast(X, list).slots
            End If

            Dim apply As RFunction = FUN
            Dim list As New Dictionary(Of String, Object)
            Dim getName As Func(Of SeqValue(Of Object), String) = keyNameAuto(names, envir)
            Dim value As Object
            Dim keyName$
            Dim idx As i32 = 1

            If X.GetType.ImplementInterface(Of IDictionary) Then
                Dim i As i32 = Scan0
                Dim dict As IDictionary = DirectCast(X, IDictionary)

                For Each d As Object In dict.Keys
                    value = dict(d)

                    If names Is Nothing Then
                        keyName = any.ToString(d)
                    Else
                        keyName = getName(New SeqValue(Of Object)(++i, value))
                    End If

                    list(keyName) = apply.Invoke(envir, invokeArgument(value, ++idx))

                    If Program.isException(list(keyName)) Then
                        Return list(keyName)
                    End If
                Next
            ElseIf TypeOf X Is pipeline Then
                For Each obj As SeqValue(Of Object) In DirectCast(X, pipeline) _
                    .populates(Of Object)(envir) _
                    .SeqIterator

                    keyName = getName(obj)
                    value = apply.Invoke(envir, invokeArgument(obj.value, ++idx))

                    If Program.isException(value) Then
                        Return value
                    Else
                        list(keyName) = value
                    End If
                Next
            Else
                For Each d As SeqValue(Of Object) In REnv.asVector(Of Object)(X) _
                    .AsObjectEnumerator _
                    .SeqIterator

                    keyName = getName(d)
                    value = apply.Invoke(envir, invokeArgument(d.value, ++idx))

                    If Program.isException(value) Then
                        Return value
                    Else
                        list(keyName) = value
                    End If
                Next
            End If

            Return New list With {.slots = list}
        End Function

        Private Function keyNameAuto(type As Type) As Func(Of Object, String)
            Static cache As New Dictionary(Of Type, Func(Of Object, String))

            Return cache.ComputeIfAbsent(
                key:=type,
                lazyValue:=Function(key As Type)
                               If key.ImplementInterface(GetType(INamedValue)) Then
                                   Return Function(a) DirectCast(a, INamedValue).Key
                               ElseIf key.ImplementInterface(GetType(IReadOnlyId)) Then
                                   Return Function(a) DirectCast(a, IReadOnlyId).Identity
                               ElseIf key.ImplementInterface(GetType(IKeyedEntity(Of String))) Then
                                   Return Function(a) DirectCast(a, IKeyedEntity(Of String)).Key
                               ElseIf key Is GetType(Group) Then
                                   Return Function(g) any.ToString(DirectCast(g, Group).key)
                               Else
                                   Return Function() Nothing
                               End If
                           End Function)
        End Function

        Public Function keyNameAuto(names As Object, env As Environment) As Func(Of SeqValue(Of Object), String)
            If names Is Nothing Then
                Return Function(i)
                           Dim name As String = Nothing

                           If Not i.value Is Nothing Then
                               name = keyNameAuto(i.value.GetType)(i.value)
                           End If

                           If name Is Nothing Then
                               name = $"[[{i.i + 1}]]"
                           End If

                           Return name
                       End Function
            ElseIf names.GetType.ImplementInterface(Of RFunction) Then
                Dim func As RFunction = DirectCast(names, RFunction)

                Return Function(i)
                           Dim nameVals = func.Invoke(env, invokeArgument(i.value))
                           Dim namesVec = RConversion.asCharacters(nameVals)

                           Return getFirst(namesVec)
                       End Function
            Else
                Dim vec As String() = REnv.asVector(Of String)(names)

                Return Function(i)
                           Return vec(i)
                       End Function
            End If
        End Function
    End Module
End Namespace

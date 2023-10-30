#Region "Microsoft.VisualBasic::041bcebfce44c5eac307081a75fd84df, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/applys.vb"

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

'   Total Lines: 587
'    Code Lines: 395
' Comment Lines: 113
'   Blank Lines: 79
'     File Size: 24.68 KB


'     Module applys
' 
'         Function: apply, checkInternal, lapply, lapplyGeneralIDictionary, lapplyGeneralSequence
'                   lapplyPipelineStream, lapplyRNameIndex, parLapply, parSapply, sapply
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RObj = SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal.Invokes

    <Package("applys")>
    Public Module applys

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
            Dim check = checkInternal(x, FUN, env)

            If Not TypeOf check Is Boolean Then
                Return check
            End If

            If x Is Nothing Then
                Return New Object() {}
            ElseIf TypeOf x Is dataframe Then
                Return doApply.apply(DirectCast(x, dataframe), margin, FUN, env)
            Else
                Return debug.stop(New InvalidProgramException, env)
            End If
        End Function

        ''' <summary>
        ''' Parallel version of ``lapply``.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="FUN"></param>
        ''' <param name="group"></param>
        ''' <param name="n_threads"></param>
        ''' <param name="verbose"></param>
        ''' <param name="names">
        ''' Set new names to the result list, the size of this parameter
        ''' should be equals to the size of the original input sequence 
        ''' size.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("parLapply")>
        Public Function parLapply(<RRawVectorArgument> x As list, FUN As Object,
                                  Optional group As Integer = -1,
                                  Optional n_threads As Integer = -1,
                                  Optional verbose As Boolean? = Nothing,
                                  Optional names As String() = Nothing,
                                  Optional env As Environment = Nothing) As Object
            If x Is Nothing Then
                Return Nothing
            End If

            Dim check = checkInternal(x, FUN, env)

            If Not TypeOf check Is Boolean Then
                Return check
            End If

            Dim seq As List(Of Object)
            Dim apply As RFunction = FUN
            Dim result = x.slots.parallelList(apply, group, n_threads, env.verboseOption(opt:=verbose), env)
            Dim rawSize As Integer = x.length

            If names.IsNullOrEmpty Then
                names = result.names.ToArray
            ElseIf names.Length = x.length Then
                ' do nothing, use the parameter user input(assign new names)
            Else
                ' the size of the names that user input from the parameter
                ' is not matched with the sequence size
                Return Internal.debug.stop({
                    $"the size of the names input({names.Length}) is not matched with the size of the input sequence({x.length})!",
                    $"sizeof_names: {names.Length}",
                    $"sizeof_sequence: {x.length}"
                }, env)
            End If

            ' the result data set still keeps the same order with the
            ' original input sequence
            seq = result.objects
            x = New list With {.slots = New Dictionary(Of String, Object)}

            If seq.Count <> rawSize Then
                Return Internal.debug.stop({
                    $"the size of the raw sequence input({rawSize}) is not matched with the size of the result sequence({seq.Count})!",
                    $"sizeof_rawSeq: {rawSize}",
                    $"sizeof_result: {seq.Count}"
                }, env)
            End If

            For i As Integer = 0 To names.Length - 1
                If Program.isException(seq(i)) Then
                    Return seq(i)
                Else
                    Call x.add(names(i), seq(i))
                End If
            Next

            Return x
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

        ''' <summary>
        ''' check for the argument error
        ''' </summary>
        ''' <param name="X">check for the input argument, should be nothing if no needed</param>
        ''' <param name="FUN">should be implements a R# function interface.</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' this function returns nothing if no errors, otherwise a
        ''' error message will be generated if:
        ''' 
        ''' 1. the input <paramref name="X"/> may be a error message, or
        ''' 2. the given <paramref name="FUN"/> is not implements the <see cref="RFunction"/> interface.
        ''' </returns>
        ''' <remarks>this function will convert the <paramref name="FUN"/> 
        ''' from a clr <see cref="MethodInfo"/> pointer to <see cref="RMethodInfo"/> 
        ''' automatically.</remarks>
        Public Function checkInternal(X As Object, ByRef FUN As Object, env As Environment) As Object
            If TypeOf FUN Is MethodInfo Then
                FUN = New RMethodInfo("call_clr", DirectCast(FUN, MethodInfo), Nothing)
            End If

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

                    If TypeOf value Is ReturnValue Then
                        value = DirectCast(value, ReturnValue).value.Evaluate(envir)
                    End If
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

                For Each d As Object In REnv.asVector(Of Object)(X).AsObjectEnumerator
                    argsPreviews = invokeArgument(d, ++i)
                    value = apply.Invoke(envir, argsPreviews)

                    If TypeOf value Is ReturnValue Then
                        value = DirectCast(value, ReturnValue).value.Evaluate(envir)
                    End If

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

        <Extension>
        Private Function lapplyGeneralIDictionary(dict As IDictionary,
                                                  apply As RFunction,
                                                  getName As Func(Of SeqValue(Of Object), [Variant](Of String, Message)),
                                                  hasName As Boolean,
                                                  env As Environment) As Object
            Dim i As i32 = Scan0
            Dim value As Object
            Dim keyName As [Variant](Of String, Message)
            Dim list As New Dictionary(Of String, Object)
            Dim idx As i32 = 1
            Dim asmFile As String = GetType(applys).Assembly.FullName
            Dim methodTrace As New Method With {
                .Method = NameOf(lapplyGeneralIDictionary),
                .[Module] = NameOf(applys),
                .[Namespace] = "Rsharp_clr_interop"
            }

            For Each d As Object In dict.Keys
                value = dict(d)

                ' user didn't specific the names function or value
                ' use the key in the dictionary as the new list name
                ' by default
                If Not hasName Then
                    keyName = any.ToString(d)
                Else
                    ' use the index key name from the user parameter
                    keyName = getName(New SeqValue(Of Object)(++i, value))

                    If keyName Like GetType(Message) Then
                        Return keyName.TryCast(Of Message)
                    End If
                End If

                env.setStackInfo(New StackFrame With {.File = asmFile, .Line = keyName, .Method = methodTrace})
                value = apply.Invoke(env, invokeArgument(value, ++idx))

                If TypeOf value Is ReturnValue Then
                    value = DirectCast(value, ReturnValue).Evaluate(env)
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    list(keyName.VA) = value
                End If
            Next

            Return New list With {.slots = list}
        End Function

        <Extension>
        Private Function lapplyPipelineStream(pip As pipeline,
                                              apply As RFunction,
                                              getName As Func(Of SeqValue(Of Object), [Variant](Of String, Message)),
                                              env As Environment) As Object

            Dim pops = pip.populates(Of Object)(env)
            Dim i As i32 = Scan0
            Dim value As Object
            Dim keyName As [Variant](Of String, Message)
            Dim list As New Dictionary(Of String, Object)
            Dim idx As i32 = 1

            For Each obj As SeqValue(Of Object) In pops.SeqIterator
                keyName = getName(obj)

                If keyName Like GetType(Message) Then
                    Return keyName.TryCast(Of Message)
                Else
                    value = apply.Invoke(env, invokeArgument(obj.value, ++idx))
                End If

                If TypeOf value Is ReturnValue Then
                    value = DirectCast(value, ReturnValue).Evaluate(env)
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    list(keyName.VA) = value
                End If
            Next

            Return New list With {.slots = list}
        End Function

        <Extension>
        Private Function lapplyRNameIndex(vec As RIndex,
                                          apply As RFunction,
                                          getName As Func(Of SeqValue(Of Object), [Variant](Of String, Message)),
                                          env As Environment) As Object

            Dim size As Integer = vec.length
            Dim d As Object
            Dim value As Object
            Dim keyName As [Variant](Of String, Message)
            Dim list As New Dictionary(Of String, Object)
            Dim idx As i32 = 1

            For i As Integer = 1 To size
                d = vec.getByIndex(i)
                keyName = getName(New SeqValue(Of Object)(i - 1, d))

                If keyName Like GetType(Message) Then
                    Return keyName.TryCast(Of Message)
                Else
                    value = apply.Invoke(env, invokeArgument(d, i - 1))
                End If

                If TypeOf value Is ReturnValue Then
                    value = DirectCast(value, ReturnValue).Evaluate(env)
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    list(keyName.VA) = value
                End If
            Next

            Return New list With {.slots = list}
        End Function

        <Extension>
        Private Function lapplyGeneralSequence(seq As IEnumerable(Of Object),
                                               apply As RFunction,
                                               getName As Func(Of SeqValue(Of Object), [Variant](Of String, Message)),
                                               env As Environment) As Object

            Dim list As New Dictionary(Of String, Object)
            Dim value As Object
            Dim keyName As [Variant](Of String, Message)
            Dim idx As i32 = 1

            For Each d As SeqValue(Of Object) In seq.SeqIterator
                keyName = getName(d)

                If keyName Like GetType(Message) Then
                    Return keyName.TryCast(Of Message)
                Else
                    value = apply.Invoke(env, invokeArgument(d.value, ++idx))
                End If

                If TypeOf value Is ReturnValue Then
                    value = DirectCast(value, ReturnValue).Evaluate(env)
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    list(keyName.VA) = value
                End If
            Next

            Return New list With {.slots = list}
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
                ' digest the R# tuple list to .net clr dictionary
                X = DirectCast(X, list).slots
            End If

            Dim apply As RFunction = FUN
            Dim getName As Func(Of SeqValue(Of Object), [Variant](Of String, Message)) = keyNameAuto(names, envir)

            envir = New Environment(envir, "lapply_internal_loop", isInherits:=True)

            If X.GetType.ImplementInterface(Of IDictionary) Then
                Return DirectCast(X, IDictionary).lapplyGeneralIDictionary(
                    apply:=apply,
                    getName:=getName,
                    hasName:=Not names Is Nothing,
                    env:=envir
                )
            ElseIf TypeOf X Is pipeline Then
                Return DirectCast(X, pipeline).lapplyPipelineStream(
                    apply:=apply,
                    getName:=getName,
                    env:=envir
                )
            ElseIf X.GetType.ImplementInterface(Of RIndex) Then
                Return DirectCast(X, RIndex).lapplyRNameIndex(
                    apply:=apply,
                    getName:=getName,
                    env:=envir
                )
            Else
                Return REnv.asVector(Of Object)(X) _
                    .AsObjectEnumerator _
                    .lapplyGeneralSequence(
                        apply:=apply,
                        getName:=getName,
                        env:=envir
                    )
            End If
        End Function
    End Module
End Namespace

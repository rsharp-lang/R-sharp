﻿#Region "Microsoft.VisualBasic::d0661b750f68187209e6310ae4db9e64, R#\Runtime\Internal\internalInvokes\Linq\linq.vb"

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

    '   Total Lines: 1670
    '    Code Lines: 947 (56.71%)
    ' Comment Lines: 538 (32.22%)
    '    - Xml Docs: 89.03%
    ' 
    '   Blank Lines: 185 (11.08%)
    '     File Size: 74.88 KB


    '     Module linq
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [select], all, any, booleanFilter, doWhile
    '                   fastIndexing, first, getPredicate, groupBy, groupDataframeRows
    '                   groupsSummary, groupSummary, last, left_join, match
    '                   objectPopulator, orderBy, produceKeyedSequence, progress, projectAs
    '                   reverse, rotate_left, rotate_right, runFilterPipeline, runWhichFilter
    '                   skip, sort, sortByKeyFunction, sortByKeyValue, split
    '                   splitByPartitionSize, splitCollection, splitList, splitVector, take
    '                   tryKeyBy, unique, where, whichGenericClrSet, whichMax
    '                   whichMin
    '         Class SplitPredicateFunction
    ' 
    '             Function: AssertEquals, AssertThat, GetPredicate
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Parallel.Tasks.TaskQueue(Of Long)
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Converts
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Linq
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports obj = Microsoft.VisualBasic.Scripting
Imports ObjectSet = SMRUCC.Rsharp.Runtime.Vectorization.ObjectSet
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes.LinqPipeline

    ''' <summary>
    ''' Provides a set of static (Shared in Visual Basic) methods for querying objects
    ''' that implement System.Collections.Generic.IEnumerable`1.
    ''' </summary>
    <Package("linq", Category:=APICategories.SoftwareTools, Publisher:="xie.guigang@live.com")>
    Module linq

        Sub New()
            Call generic.add(NameOf(base.summary), GetType(Group), AddressOf groupSummary)
            Call generic.add(NameOf(base.summary), GetType(Group()), AddressOf groupsSummary)
        End Sub

        Private Function groupSummary(x As Group, args As list, env As Environment) As Object
            Return $" '{x.length}' elements with key: " & printer.ValueToString(x.key, env.globalEnvironment)
        End Function

        Private Function groupsSummary(groups As Group(), args As list, env As Environment) As Object
            Dim summary As New list With {.slots = New Dictionary(Of String, Object)}

            For Each item As Group In groups
                summary.slots.Add(Scripting.ToString(item.key), item.length)
            Next

            Return summary
        End Function

        ''' <summary>
        ''' A left join is a type of relational join operation that combines 
        ''' two datasets based on a common column or variable. The result of 
        ''' a left join includes all the rows from the left dataset and any 
        ''' matching rows from the right dataset.
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="by">the field name that used for join two data table, if the field name that 
        ''' specific by this parameter is existsed in both <paramref name="left"/> and 
        ''' <paramref name="right"/>.</param>
        ''' <param name="grep">
        ''' text grep expression for the index key string value, see ``text_grep``.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("left_join")>
        Public Function left_join(left As dataframe, right As dataframe,
                                  Optional by_x As String = Nothing,
                                  Optional by_y As String = Nothing,
                                  Optional [by] As String = Nothing,
                                  Optional grep As Object = Nothing,
                                  Optional env As Environment = Nothing) As Object

            If left Is Nothing Then
                Return right
            ElseIf right Is Nothing Then
                Return left
            End If

            Dim keyX As String()
            Dim keyY As String()

            If Not by Is Nothing Then
                keyX = left.getColumnVector(by)
                keyY = right.getColumnVector(by)
            ElseIf by_x Is Nothing OrElse by_y Is Nothing Then
                Return Internal.debug.stop({
                    "missing primary key for join two dataframe!",
                    "you should set parameter 'by' for specific field name that bot existed in two given dataset, or by.x and by.y if the index field its field name is different between two dataset."
                }, env)
            Else
                keyX = left.getColumnVector(by_x)
                keyY = right.getColumnVector(by_y)
            End If

            If Not grep Is Nothing Then
                If TypeOf grep Is String Then
                    grep = stringr.text_grep(grep)
                End If
                If TypeOf grep Is Message Then
                    Return grep
                End If
                If Not TypeOf grep Is TextGrepLambda Then
                    Return Message.InCompatibleType(GetType(TextGrepLambda), grep.GetType, env)
                Else
                    keyX = DirectCast(grep, TextGrepLambda).TextGrep(keyX)
                    keyY = DirectCast(grep, TextGrepLambda).TextGrep(keyY)
                End If
            End If

            ' 20221207
            ' index i is zero-based
            ' indexing of the right dataset
            Dim idx As Dictionary(Of String, Integer) = Index(Of String).Indexing(keyY)
            Dim i As Integer() = keyX _
                .Select(Function(key)
                            ' due to the reason of left join some data may be missing in the right dataset
                            ' so index -1 maybe generates at here.
                            Return idx.TryGetValue(key, [default]:=-1)
                        End Function) _
                .ToArray
            Dim rightSubset = right.GetByRowIndex(i, env) ' checked

            If rightSubset Like GetType(Message) Then
                Return rightSubset.TryCast(Of Message)
            End If

            right = rightSubset.TryCast(Of dataframe)
            left = New dataframe(left)

            ' finally do column combine of two dataset
            For Each colName As String In right.colnames
                If left.hasName(colName) Then
                    ' renames the colname from right dataset if the column name
                    ' is already existed inside the left dataset.
                    Call left.columns.Add($"{colName}.1", right(colName))
                Else
                    Call left.columns.Add(colName, right(colName))
                End If
            Next

            Return left
        End Function

        ''' <summary>
        ''' apply for the pipeline progress report
        ''' </summary>
        ''' <param name="x">
        ''' the pipeline object or a progress number if 
        ''' current function invoke is occurs in a parallel 
        ''' task.
        ''' </param>
        ''' <param name="msgFunc">a text message to display or function for show message</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' value range of parameter <paramref name="x"/> should be in numeric 
        ''' range ``[0,100]`` if the progress function is invoked in a parallel 
        ''' stack environment.
        ''' </remarks>
        <ExportAPI("progress")>
        Public Function progress(<RRawVectorArgument>
                                 x As Object,
                                 Optional msgFunc As Object = Nothing,
                                 Optional env As Environment = Nothing) As Object

            If msgFunc Is Nothing Then
                Dim symbol As Symbol = env.FindSymbol(parallelApplys.ParallelTaskWorkerSymbol)
                Dim worker As TaskWorker = TryCast(symbol?.value, TaskWorker)

                If Not worker Is Nothing Then
                    worker.progress = REnv.single(
                        x:=CLRVector.asNumeric(x),
                        forceSingle:=True
                    )
                Else
                    Dim typeX As RType = RType.TypeOf(x)
                    Dim stack = env.stackTrace.ElementAtOrDefault(2)

                    If typeX.mode.IsNumeric Then
                        Call RunSlavePipeline.SendProgress(CDbl(REnv.single(x, True)), If(stack Is Nothing, "Pipeline progress report", stack.Method.Method))
                    Else
                        Call base.print("",, env)
                    End If
                End If
            ElseIf msgFunc.GetType.ImplementInterface(Of RFunction) Then
                Call DirectCast(msgFunc, RFunction).Invoke({x}, env)
            Else
                Call base.print(msgFunc,, env)
            End If

            Return x
        End Function

        ''' <summary>
        ''' create data index for the given input data sequence
        ''' </summary>
        ''' <param name="x">a data array as sequence</param>
        ''' <param name="mode">the element mode of the data input seuqnce <paramref name="x"/></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("as.index")>
        Public Function fastIndexing(x As Array,
                                     <RRawVectorArgument(GetType(String))>
                                     Optional mode As Object = "any|character|numeric|integer",
                                     Optional env As Environment = Nothing) As Object

            Select Case obj.ToString(CLRVector.asCharacter(mode).DefaultFirst("any")).ToLower
                Case "any"
                    Throw New NotImplementedException
                Case "character"
                    Return CLRVector.asCharacter(x).Indexing
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        ''' <summary>
        ''' take the first n items from the given input sequence data 
        ''' </summary>
        ''' <param name="sequence">the input sequence data</param>
        ''' <param name="n">the number of first n element</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("take")>
        Public Function take(<RRawVectorArgument>
                             sequence As Object,
                             <RRawVectorArgument>
                             n As Object,
                             Optional env As Environment = Nothing) As Object

            Dim nvec As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(n), env)

            If sequence Is Nothing Then
                Return Nothing
            ElseIf nvec.GetType.GetElementType Like RType.integers Then
                Dim nscalar As Integer = nvec.AsObjectEnumerator.Select(Function(o) CInt(o)).First

                If TypeOf sequence Is list Then
                    Dim list As list = DirectCast(sequence, list)
                    Dim names As String() = list.getNames.Take(nscalar).ToArray
                    Dim subset As New list With {
                        .slots = names _
                            .ToDictionary(Function(key) key,
                                          Function(key)
                                              Return list.slots(key)
                                          End Function)
                    }

                    Return subset
                ElseIf TypeOf sequence Is pipeline Then
                    Return DirectCast(sequence, pipeline) _
                        .populates(Of Object)(env) _
                        .Take(nscalar) _
                        .DoCall(Function(seq)
                                    Return New pipeline(seq, DirectCast(sequence(), pipeline).elementType)
                                End Function)
                Else
                    Return ObjectSet.GetObjectSet(sequence, env).Take(nscalar).ToArray
                End If
            ElseIf nvec.GetType.GetElementType Like RType.characters Then
                If TypeOf sequence Is dataframe Then
                    Dim colnames As String() = nvec

                    If colnames.Length = 1 Then
                        ' get column vector
                        Return DirectCast(sequence, dataframe).getColumnVector(colnames(Scan0))
                    Else
                        Throw New NotImplementedException
                    End If
                Else
                    Return Message.InCompatibleType(GetType(dataframe), sequence.GetType(), env)
                End If
            Else
                Throw New NotImplementedException
            End If
        End Function

        ''' <summary>
        ''' ### Value Matching
        ''' 
        ''' match returns a vector of the positions of 
        ''' (first) matches of its first argument in 
        ''' its second.
        ''' 
        ''' find the index of the elements in input sequence
        ''' <paramref name="x"/> in the source target sequence
        ''' <paramref name="table"/>
        ''' 
        ''' find the index of (where x in table)
        ''' </summary>
        ''' <param name="x">
        ''' vector or NULL: the values to be matched. Long 
        ''' vectors are supported.
        ''' </param>
        ''' <param name="table">
        ''' vector or NULL: the values to be matched against. 
        ''' Long vectors are not supported. (using as index 
        ''' object.)
        ''' </param>
        ''' <param name="nomatch">
        ''' the value to be returned in the case when no 
        ''' match is found. Note that it is coerced to 
        ''' integer.
        ''' </param>
        ''' <param name="incomparables">
        ''' a vector of values that cannot be matched. 
        ''' Any value in x matching a value in this vector 
        ''' is assigned the nomatch value. For historical 
        ''' reasons, FALSE is equivalent to NULL.
        ''' </param>
        ''' <returns>
        ''' A vector of the same length as x.
        ''' An integer vector giving the position in table of 
        ''' the first match if there Is a match, otherwise 
        ''' nomatch.
        ''' If x[i] Is found To equal table[j] Then the value 
        ''' returned In the i-th position Of the Return value 
        ''' Is j, For the smallest possible j. If no match Is 
        ''' found, the value Is nomatch.
        ''' </returns>
        ''' <remarks>
        ''' https://stackoverflow.com/questions/7530765/get-the-index-of-the-values-of-one-vector-in-another
        ''' 
        ''' ```r
        ''' first  = c("a", "c", "b");
        ''' second = c("c", "b", "a");
        ''' match(second, first);
        ''' 
        ''' #   c b a  &lt;-second 
        ''' [1] 2 3 1
        ''' ```
        ''' </remarks>
        <ExportAPI("match")>
        Public Function match(x As Array, table As Array,
                              Optional nomatch As Integer = -1,
                              Optional incomparables As Integer = Nothing) As Integer()

            Dim index As Index(Of String) = CLRVector.asCharacter(table)
            Dim values As String() = CLRVector.asCharacter(x)
            Dim ordinal As Integer() = (From str As String
                                        In values
                                        Let i As Integer = If(str Like index, index(str) + 1, nomatch)
                                        Select i).ToArray

            Return ordinal
        End Function

        ''' <summary>
        ''' Bypasses a specified number of elements in a sequence and then 
        ''' returns the remaining elements.
        ''' </summary>
        ''' <param name="sequence">An System.Collections.Generic.IEnumerable`1 to return elements from.</param>
        ''' <param name="n">The number of elements to skip before returning the remaining elements.</param>
        ''' <returns>An System.Collections.Generic.IEnumerable`1 that contains the elements that occur
        ''' after the specified index in the input sequence.</returns>
        <ExportAPI("skip")>
        Public Function skip(<RRawVectorArgument> sequence As Object, n%, Optional env As Environment = Nothing) As Object
            If sequence Is Nothing Then
                Return Nothing
            ElseIf TypeOf sequence Is pipeline Then
                Return DirectCast(sequence, pipeline) _
                    .populates(Of Object)(env) _
                    .Skip(n) _
                    .DoCall(Function(seq)
                                Return New pipeline(seq, DirectCast(sequence, pipeline).elementType)
                            End Function)
            Else
                Return ObjectSet.GetObjectSet(sequence, env).Skip(n).ToArray
            End If
        End Function

        ''' <summary>
        ''' Returns distinct elements from a sequence by using a specified 
        ''' IEqualityComparer to compare values.
        ''' </summary>
        ''' <param name="items">The sequence to remove duplicate elements from.</param>
        ''' <param name="getKey">An IEqualityComparer to compare values.</param>
        ''' <param name="envir"></param>
        ''' <returns>An IEnumerable that contains distinct elements from
        ''' the source sequence.</returns>
        <ExportAPI("unique")>
        Private Function unique(<RRawVectorArgument>
                                items As Object,
                                Optional getKey As RFunction = Nothing,
                                Optional envir As Environment = Nothing) As Object

            If TypeOf items Is String() Then
                Return DirectCast(items, String()).Distinct.ToArray
            End If

            If Not getKey Is Nothing Then
                Return ObjectSet.GetObjectSet(items, envir) _
                   .GroupBy(Function(o)
                                Dim arg = InvokeParameter.CreateLiterals(o)
                                Return getKey.Invoke(envir, arg)
                            End Function) _
                   .Select(Function(g)
                               Return g.First
                           End Function) _
                   .ToArray
            Else
                Return ObjectSet.GetObjectSet(items, envir) _
                   .GroupBy(Function(o)
                                Return o
                            End Function) _
                   .Select(Function(g) g.Key) _
                   .ToArray
            End If
        End Function

        ''' <summary>
        ''' A lapply/sapply liked mapping function
        ''' </summary>
        ''' <param name="sequence"></param>
        ''' <param name="project"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("projectAs")>
        Private Function projectAs(<RRawVectorArgument>
                                   sequence As Object,
                                   project As RFunction,
                                   Optional envir As Environment = Nothing) As Object

            If sequence Is Nothing Then
                Return Nothing
            End If

            Dim doProject As Func(Of Object, Object) = Function(o) project.Invoke(envir, InvokeParameter.CreateLiterals(o))
            Dim type As IRType = project.getReturns(envir)

            If TypeOf sequence Is pipeline Then
                ' run in pipeline mode
                Dim seq As pipeline = DirectCast(sequence, pipeline)
                Dim projection As IEnumerable(Of Object) = seq _
                    .populates(Of Object)(envir) _
                    .Select(doProject)

                If TypeOf type Is RType Then
                    Return New pipeline(projection, DirectCast(type, RType))
                Else
                    Return New pipeline(projection, GetType(list))
                End If
            Else
                Dim result As Object() = ObjectSet.GetObjectSet(sequence, envir) _
                    .Select(doProject) _
                    .ToArray

                If TypeOf type Is RType Then
                    Return New vector(result, DirectCast(type, RType))
                Else
                    Return New vector(result, RType.list)
                End If
            End If
        End Function

        ''' <summary>
        ''' ### Which indices are TRUE?
        ''' 
        ''' The which test filter, Give the TRUE indices of a logical 
        ''' object, allowing for array indices.
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="x">
        ''' a logical vector or array. NAs are allowed and omitted
        ''' (treated as if FALSE).
        ''' </param>
        ''' <param name="first">
        ''' get the first index element where the assert is TRUE, this parameter
        ''' works for test expression is nothing
        ''' </param>
        ''' <returns>
        ''' an integer vector with length equal to sum(x), i.e., to 
        ''' the number of TRUEs in x; Basically, the result is 
        ''' ``(1:length(x))[x]``.
        ''' </returns>
        ''' <remarks>
        ''' Unlike most other base R functions this does not coerce to 
        ''' x to logical: only arguments with typeof logical are 
        ''' accepted and others give an error.
        ''' </remarks>
        ''' <example>
        ''' x = [TRUE FALSE FALSE TRUE];
        ''' 
        ''' print(which(x));
        ''' # [1] 1 4
        ''' 
        ''' # just returns the first element index which is assert as TRUE
        ''' print(which(x, first = TRUE));
        ''' # [1] 1
        ''' </example>
        <ExportAPI("which")>
        Private Function where(<RRawVectorArgument>
                               x As Object,
                               Optional test As Object = Nothing,
                               Optional pipelineFilter As Boolean = True,
                               Optional first As Boolean = False,
                               Optional env As Environment = Nothing) As Object

            If test Is Nothing Then
                If first Then
                    ' assert for first which index
                    Dim asserts As Boolean() = CLRVector.asLogical(x)

                    For i As Integer = 0 To asserts.Length - 1
                        If asserts(i) Then
                            ' index offset start from 1 in R# language
                            Return i + 1
                        End If
                    Next

                    ' all is false
                    Return Nothing
                Else
                    ' assert for all which index
                    Return which.IsTrue(CLRVector.asLogical(x), offset:=1)
                End If
            ElseIf TypeOf x Is pipeline Then
                ' run in pipeline mode
                Return runFilterPipeline(x, test, pipelineFilter, env)
            ElseIf TypeOf x Is list Then
                Dim subKeys As New list With {.slots = New Dictionary(Of String, Object)}
                Dim list As list = DirectCast(x, list)
                Dim filter = getPredicate(test, env)

                If filter Like GetType(Message) Then
                    Return filter.TryCast(Of Message)
                End If

                Dim predicate As Predicate(Of Object) = filter.TryCast(Of Predicate(Of Object))

                For Each name As String In list.slotKeys
                    Dim testFlag As Boolean = predicate(list.getByName(name))

                    If testFlag Then
                        subKeys.add(name, list.getByName(name))
                    End If
                Next

                Return subKeys
            Else
                Return whichGenericClrSet(x, test, pipelineFilter, env)
            End If
        End Function

        Private Function whichGenericClrSet(x As Object, test As Object, pipelineFilter As Boolean, env As Environment) As Object
            Dim testResult = ObjectSet.GetObjectSet(x, env) _
                .runWhichFilter(test, env) _
                .ToArray

            If pipelineFilter Then
                Dim objs As New List(Of Object)

                ' returns the object vector where test result is true
                For Each obj As Object In testResult
                    If TypeOf obj Is Message Then
                        Return obj
                    Else
                        With DirectCast(obj, (Boolean, Object))
                            If .Item1 Then
                                Call objs.Add(.Item2)
                            End If
                        End With
                    End If
                Next

                Return REnv.TryCastGenericArray(objs.ToArray, env)
            Else
                Dim booleans As New List(Of Boolean)

                ' returns the logical test vector result
                For Each obj As Object In testResult
                    If TypeOf obj Is Message Then
                        Return obj
                    Else
                        With DirectCast(obj, (Boolean, Object))
                            booleans.Add(.Item1)
                        End With
                    End If
                Next

                Return booleans.ToArray
            End If
        End Function

        Private Function booleanFilter(seq As IEnumerable(Of Object)) As [Variant](Of Boolean(), Message)
            Dim booleans As New List(Of Boolean)

            For Each obj As Object In seq
                If TypeOf obj Is Message Then
                    Return obj
                Else
                    With DirectCast(obj, (Boolean, Object))
                        booleans.Add(.Item1)
                    End With
                End If
            Next

            Return booleans.ToArray
        End Function

        Private Iterator Function objectPopulator(seq As IEnumerable(Of Object)) As IEnumerable(Of Object)
            For Each obj As Object In seq
                If TypeOf obj Is Message Then
                    Yield obj
                    Return
                Else
                    With DirectCast(obj, (Boolean, Object))
                        If .Item1 Then
                            Yield .Item2
                        End If
                    End With
                End If
            Next
        End Function

        Private Function runFilterPipeline(sequence As Object,
                                           test As Object,
                                           pipelineFilter As Boolean,
                                           env As Environment) As Object

            Return DirectCast(sequence, pipeline) _
                .populates(Of Object)(env) _
                .runWhichFilter(test, env) _
                .DoCall(Function(seq)
                            If pipelineFilter Then
                                Return New pipeline(objectPopulator(seq), DirectCast(sequence, pipeline).elementType)
                            Else
                                Return booleanFilter(seq).Value
                            End If
                        End Function)
        End Function

        Private Function getPredicate(test As Object, env As Environment) As [Variant](Of Predicate(Of Object), Message)
            Dim predicate As Predicate(Of Object)

            If TypeOf test Is RFunction Then
                predicate = Function(item)
                                Dim arg = InvokeParameter.CreateLiterals(item)
                                Dim result = DirectCast(test, RFunction).Invoke(env, arg)

                                Return CLRVector.asLogical(result)(Scan0)
                            End Function
            ElseIf TypeOf test Is Predicate(Of Object) Then
                predicate = DirectCast(test, Predicate(Of Object))
            ElseIf TypeOf test Is Func(Of Object, Boolean) Then
                predicate = New Predicate(Of Object)(AddressOf DirectCast(test, Func(Of Object, Boolean)).Invoke)
            Else
                Return Internal.debug.stop(Message.InCompatibleType(GetType(Predicate(Of Object)), test.GetType, env), env)
            End If

            Return predicate
        End Function

        <Extension>
        Private Iterator Function runWhichFilter(sequence As IEnumerable(Of Object),
                                                 test As Object,
                                                 env As Environment) As IEnumerable(Of Object)

            Dim getFunc = getPredicate(test, env)

            If getFunc Like GetType(Message) Then
                Yield getFunc.TryCast(Of Message)
                Return
            End If

            Dim predicate As Predicate(Of Object) = getFunc
            Dim result As Boolean

            For Each item As Object In sequence
                result = predicate(item)
                'If Program.isException(result) Then
                '    Yield result
                '    Exit For
                'End If
                Yield (result, item)
            Next
        End Function

        ''' <summary>
        ''' Where is the Min() or Max() or first TRUE or FALSE ?
        ''' 
        ''' Determines the location, i.e., index of the (first) minimum or maximum of a 
        ''' numeric (or logical) vector.
        ''' </summary>
        ''' <param name="x">
        ''' numeric (logical, integer or double) vector or an R object for which the internal 
        ''' coercion to double works whose min or max is searched for.
        ''' </param>
        ''' <param name="eval"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' Missing and NaN values are discarded.
        ''' 
        ''' an integer Or on 64-bit platforms, if length(x) = n>= 2^31 an integer valued 
        ''' double of length 1 Or 0 (iff x has no non-NAs), giving the index of the first 
        ''' minimum Or maximum respectively of x.
        ''' 
        ''' If this extremum Is unique (Or empty), the results are the same As (but more 
        ''' efficient than) ``which(x == min(x, na.rm = True))`` Or 
        ''' ``which(x == max(x, na.rm = True))`` respectively.
        ''' 
        ''' Logical x – First True Or False
        ''' 
        ''' For a logical vector x with both FALSE And TRUE values, which.min(x) And 
        ''' which.max(x) return the index of the first FALSE Or TRUE, respectively, as 
        ''' FALSE &lt; TRUE. However, match(FALSE, x) Or match(TRUE, x) are typically 
        ''' preferred, as they do indicate mismatches.
        ''' </returns>
        <ExportAPI("which.max")>
        <RApiReturn(GetType(Integer))>
        Public Function whichMax(<RRawVectorArgument>
                                 x As Object,
                                 Optional eval As Object = Nothing,
                                 Optional env As Environment = Nothing) As Object

            If eval Is Nothing Then
                If x Is Nothing Then
                    Return New Integer() {}
                Else
                    Dim dbl As Double() = CLRVector.asNumeric(x)

                    If dbl.Length = 0 Then
                        Return New Integer() {}
                    Else
                        Return which.Max(dbl) + 1
                    End If
                End If
            Else
                Dim lambda = LinqPipeline.lambda.CreateProjectLambda(Of Double)(eval, env)
                Dim scores = ObjectSet.GetObjectSet(x, env).Select(lambda).ToArray

                If scores.Length = 0 Then
                    Return New Integer() {}
                Else
                    Return which.Max(scores) + 1
                End If
            End If
        End Function

        ''' <summary>
        ''' Where is the Min() or Max() or first TRUE or FALSE ?
        ''' 
        ''' Determines the location, i.e., index of the (first) minimum or maximum of a 
        ''' numeric (or logical) vector.
        ''' </summary>
        ''' <param name="x">
        ''' numeric (logical, integer or double) vector or an R object for which the internal 
        ''' coercion to double works whose min or max is searched for.
        ''' </param>
        ''' <param name="eval"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' Missing and NaN values are discarded.
        ''' 
        ''' an integer Or on 64-bit platforms, if length(x) = n>= 2^31 an integer valued 
        ''' double of length 1 Or 0 (iff x has no non-NAs), giving the index of the first 
        ''' minimum Or maximum respectively of x.
        ''' 
        ''' If this extremum Is unique (Or empty), the results are the same As (but more 
        ''' efficient than) which(x == min(x, na.rm = True)) Or which(x == max(x, na.rm = True)) 
        ''' respectively.
        ''' 
        ''' Logical x – First True Or False
        ''' 
        ''' For a logical vector x with both FALSE And TRUE values, which.min(x) And 
        ''' which.max(x) return the index of the first FALSE Or TRUE, respectively, as 
        ''' FALSE &lt; TRUE. However, match(FALSE, x) Or match(TRUE, x) are typically 
        ''' preferred, as they do indicate mismatches.
        ''' </returns>
        <ExportAPI("which.min")>
        <RApiReturn(GetType(Integer))>
        Public Function whichMin(<RRawVectorArgument> x As Object, Optional eval As Object = Nothing, Optional env As Environment = Nothing) As Object
            If eval Is Nothing Then
                If x Is Nothing Then
                    Return New Integer() {}
                Else
                    Dim dbl As Double() = CLRVector.asNumeric(x)

                    If dbl.Length = 0 Then
                        Return New Integer() {}
                    Else
                        Return which.Min(dbl) + 1
                    End If
                End If
            Else
                Dim lambda = LinqPipeline.lambda.CreateProjectLambda(Of Double)(eval, env)
                Dim scores = ObjectSet.GetObjectSet(x, env).Select(lambda).ToArray

                If scores.Length = 0 Then
                    Return New Integer() {}
                Else
                    Return which.Min(scores) + 1
                End If
            End If
        End Function

        ''' <summary>
        ''' Returns the first element of a sequence.
        ''' </summary>
        ''' <param name="sequence">
        ''' The System.Collections.Generic.IEnumerable`1 to return 
        ''' the first element of.
        ''' </param>
        ''' <param name="test">An element test assert lambda function
        ''' for find the first element which matched with this test 
        ''' condition</param>
        ''' <param name="envir"></param>
        ''' <returns>The first element in the specified sequence. NULL
        ''' value will be returned if there is no element could be found
        ''' in the given seuqnece or under the given <paramref name="test"/>
        ''' condition.</returns>
        <ExportAPI("first")>
        Private Function first(<RRawVectorArgument>
                               sequence As Object,
                               Optional test As RFunction = Nothing,
                               Optional envir As Environment = Nothing) As Object

            Dim pass As Boolean
            Dim arg As InvokeParameter()

            If test Is Nothing Then
                Return ObjectSet.GetObjectSet(sequence, envir).FirstOrDefault
            End If

            For Each item As Object In ObjectSet.GetObjectSet(sequence, envir)
                arg = InvokeParameter.CreateLiterals(item)
                pass = CLRVector.asLogical(test.Invoke(envir, arg))(Scan0)

                If pass Then
                    Return item
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' get the last element in the sequence
        ''' </summary>
        ''' <param name="sequence">a general data sequence</param>
        ''' <param name="test">
        ''' if this test function is nothing, then means get the last element in 
        ''' the sequence. else if the function is not nothing, then means get the
        ''' last element that which meet this test condition in the sequence
        ''' data input.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("last")>
        Public Function last(<RRawVectorArgument>
                             sequence As Object,
                             Optional test As RFunction = Nothing,
                             Optional envir As Environment = Nothing) As Object

            Dim pass As Boolean
            Dim arg As InvokeParameter()

            If test Is Nothing Then
                Return ObjectSet.GetObjectSet(sequence, envir).LastOrDefault
            End If

            Dim lastVal As Object = Nothing

            For Each item As Object In ObjectSet.GetObjectSet(sequence, envir)
                arg = InvokeParameter.CreateLiterals(item)
                pass = CLRVector.asLogical(test.Invoke(envir, arg))(Scan0)

                If pass Then
                    lastVal = item
                End If
            Next

            Return lastVal
        End Function

        <Extension>
        Private Function groupDataframeRows(table As dataframe, getKey As Object, safe As Boolean, env As Environment) As Object
            Dim colName As String = CLRVector.asCharacter(getKey).FirstOrDefault

            If colName Is Nothing Then
                Return Internal.debug.stop("the dataframe group field key name could not be nothing!", env)
            End If

            Return New list(GetType(dataframe)) With {
                .slots = table _
                    .groupBy(colName, safe) _
                    .ToDictionary(Function(d) d.Key,
                                  Function(d)
                                      Return CObj(d.Value)
                                  End Function)
            }
        End Function

        ''' <summary>
        ''' group vector/list by a given evaluator or group a dataframe rows
        ''' by the cell values of a specific column.
        ''' </summary>
        ''' <param name="sequence"></param>
        ''' <param name="getKey"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function could also accept a numeric tolerance error as the
        ''' <paramref name="getKey"/> for do numeric bin grouping
        ''' </remarks>
        <ExportAPI("groupBy")>
        Private Function groupBy(<RRawVectorArgument>
                                 sequence As Object,
                                 Optional getKey As Object = Nothing,
                                 Optional safe As Boolean = False,
                                 Optional env As Environment = Nothing) As Object

            If TypeOf sequence Is dataframe Then
                Return DirectCast(sequence, dataframe).groupDataframeRows(getKey, safe, env)
            ElseIf TypeOf sequence Is Group Then
                sequence = DirectCast(sequence, Group).group
            End If

            If getKey IsNot Nothing Then
                Dim keyType As Type = getKey.GetType

                If DataFramework.IsNumericType(keyType) OrElse DataFramework.IsNumericCollection(keyType) Then
                    Dim tolerance As Double = CLRVector.asNumeric(getKey).First
                    Dim data As Double() = CLRVector.asNumeric(sequence)
                    Dim groups1D As New list With {.slots = New Dictionary(Of String, Object)}

                    For Each bin As NamedCollection(Of Double) In data.GroupBy(tolerance)
                        Call groups1D.add(bin.name, bin.ToArray)
                    Next

                    Return groups1D
                End If
            End If

            Dim measure = tryKeyBy(getKey, env)

            If measure Like GetType(Message) Then
                Return measure.TryCast(Of Message)
            End If

            Dim err As Message = Nothing
            Dim projectList = measure.TryCast(Of Func(Of Object, Object)).produceKeyedSequence(sequence, env, err)

            If Not err Is Nothing Then
                Return err
            End If

            Dim result As Group() = projectList _
                .GroupBy(Function(a) a.key) _
                .Select(Function(group)
                            Return New Group With {
                                .key = group.Key,
                                .group = group.Select(Function(a) a.obj).ToArray
                            }
                        End Function) _
                .ToArray

            If TypeOf sequence Is list Then
                Dim ref As Dictionary(Of String, Object) = DirectCast(sequence, list).slots
                Dim subListGroups As New list With {.slots = New Dictionary(Of String, Object)}

                For Each subgroup As Group In result
                    Dim key As String = Scripting.ToString(subgroup.key)
                    Dim subKeys As String() = subgroup.group.AsObjectEnumerator(Of String)().ToArray
                    Dim subList As New list With {.slots = New Dictionary(Of String, Object)}

                    For Each name As String In subKeys
                        subList.slots.Add(name, ref(name))
                    Next

                    subListGroups.add(key, subList)
                Next

                Return subListGroups
            Else
                Return result
            End If
        End Function

        ''' <summary>
        ''' get key value from the input data sequence and 
        ''' then populate the key with the original value 
        ''' elements.
        ''' </summary>
        ''' <param name="keyBy"></param>
        ''' <param name="sequence"></param>
        ''' <param name="env"></param>
        ''' <param name="err"></param>
        ''' <returns>
        ''' ***** element names will be returns as object reference 
        ''' if the given <paramref name="sequence"/> is a 
        ''' list. *****
        ''' </returns>
        <Extension>
        Private Function produceKeyedSequence(keyBy As Func(Of Object, Object),
                                              sequence As Object,
                                              env As Environment,
                                              ByRef err As Message) As IEnumerable(Of (key As Object, obj As Object))

            Dim projectList As New List(Of (key As Object, obj As Object))
            Dim key As Object

            If TypeOf sequence Is list Then
                Dim list As list = DirectCast(sequence, list)

                For Each name As String In list.slots.Keys.ToArray
                    key = keyBy(list.slots(name))

                    If Program.isException(key) Then
                        err = key
                        Exit For
                    Else
                        projectList.Add((key, CObj(name)))
                    End If
                Next
            Else
                For Each item As Object In ObjectSet.GetObjectSet(sequence, env)
                    key = keyBy(item)

                    If Program.isException(key) Then
                        err = key
                        Exit For
                    Else
                        projectList.Add((key, item))
                    End If
                Next
            End If

            Return projectList
        End Function

        Private Function tryKeyBy(getKey As Object, env As Environment) As [Variant](Of Message, Func(Of Object, Object))
            If getKey Is Nothing Then
                Return New Func(Of Object, Object)(Function(obj) obj)
            ElseIf TypeOf getKey Is RFunction Then
                Dim keyFun As RFunction = DirectCast(getKey, RFunction)

                Return New Func(Of Object, Object)(
                    Function(o)
                        Dim arg = InvokeParameter.CreateLiterals(o)
                        Dim keyVal As Object = REnv.single(getKey.Invoke(env, arg))

                        Return keyVal
                    End Function)
            ElseIf TypeOf getKey Is String Then
                Dim listKey As String = DirectCast(getKey, String)

                Return New Func(Of Object, Object)(
                    Function(o)
                        If o Is Nothing Then
                            Return Nothing
                        ElseIf o.GetType.ImplementInterface(GetType(RNameIndex)) Then
                            Return REnv.single(DirectCast(o, RNameIndex).getByName(listKey))
                        Else
                            Return debug.stop(Message.InCompatibleType(GetType(RNameIndex), o.GetType, env), env)
                        End If
                    End Function)
            Else
                Return debug.stop(Message.InCompatibleType(GetType(RFunction), getKey.GetType, env), env)
            End If
        End Function

        ''' <summary>
        ''' ### Sorting or Ordering Vectors
        ''' 
        ''' Sort (or order) a vector or factor (partially) into ascending 
        ''' or descending order. For ordering along more than one variable, 
        ''' e.g., for sorting data frames, see order.
        ''' </summary>
        ''' <param name="x">
        ''' For sort an R object with a class Or a numeric, complex, 
        ''' character Or logical vector. For sort.int, a numeric, complex, 
        ''' character Or logical vector, Or a factor.
        ''' </param>
        ''' <param name="decreasing">
        ''' logical. Should the sort be increasing or decreasing? For the 
        ''' "radix" method, this can be a vector of length equal to the 
        ''' number of arguments in .... For the other methods, it must be 
        ''' length one. Not available for partial sorting.
        ''' </param>
        ''' <param name="na_last">
        ''' for controlling the treatment of NAs. If TRUE, missing values 
        ''' in the data are put last; if FALSE, they are put first; if NA, 
        ''' they are removed.
        ''' </param>
        ''' <remarks>
        ''' sort is a generic function for which methods can be written, and 
        ''' sort.int is the internal method which is compatible with S if 
        ''' only the first three arguments are used.
        ''' The Default sort method makes use Of order For classed objects, 
        ''' which In turn makes use Of the generic Function xtfrm (And can be 
        ''' slow unless a xtfrm method has been defined Or Is.numeric(x) Is 
        ''' True).
        ''' Complex values are sorted first by the real part, Then the imaginary 
        ''' part.
        ''' The "auto" method selects "radix" for short (less than 2^31 elements) 
        ''' numeric vectors, integer vectors, logical vectors And factors; 
        ''' otherwise, "shell".
        ''' Except for method "radix", the sort order for character vectors will 
        ''' depend on the collating sequence of the locale in use: see Comparison. 
        ''' The sort order For factors Is the order Of their levels (which Is 
        ''' particularly appropriate For ordered factors).
        ''' If partial Is Not NULL, it Is taken to contain indices of elements of 
        ''' the result which are to be placed in their correct positions in the 
        ''' sorted array by partial sorting. For each of the result values in 
        ''' a specified position, any values smaller than that one are guaranteed 
        ''' to have a smaller index in the sorted array And any values which 
        ''' are greater are guaranteed to have a bigger index in the sorted array. 
        ''' (This Is included for efficiency, And many of the options are Not 
        ''' available for partial sorting. It Is only substantially more efficient 
        ''' if partial has a handful of elements, And a full sort Is done (a 
        ''' Quicksort if possible) if there are more than 10.) Names are discarded 
        ''' for partial sorting.
        ''' Method "shell" uses Shellsort (an O(n^{4/3}) variant from Sedgewick 
        ''' (1986)). If x has names a stable modification Is used, so ties are Not 
        ''' reordered. (This only matters if names are present.)
        ''' Method "quick" uses Singleton (1969)'s implementation of Hoare's 
        ''' Quicksort method and is only available when x is numeric (double or 
        ''' integer) and partial is NULL. (For other types of x Shellsort is used, 
        ''' silently.) It is normally somewhat faster than Shellsort (perhaps 50% 
        ''' faster on vectors of length a million and twice as fast at a billion)
        ''' but has poor performance in the rare worst case. (Peto's modification 
        ''' using a pseudo-random midpoint is used to make the worst case rarer.) 
        ''' This is not a stable sort, and ties may be reordered.
        ''' Method "radix" relies on simple hashing to scale time linearly with 
        ''' the input size, i.e., its asymptotic time complexity Is O(n). The specific 
        ''' variant And its implementation originated from the data.table package 
        ''' And are due to Matt Dowle And Arun Srinivasan. For small inputs (&lt; 200), 
        ''' the implementation uses an insertion sort (O(n^2)) that operates in-place 
        ''' to avoid the allocation overhead of the radix sort. For integer vectors 
        ''' of range less than 100,000, it switches to a simpler And faster linear 
        ''' time counting sort. In all cases, the sort Is stable; the order of ties 
        ''' Is preserved. It Is the default method for integer vectors And factors.
        ''' The "radix" method generally outperforms the other methods, especially 
        ''' for character vectors And small integers. Compared to quick sort, it Is 
        ''' slightly faster for vectors with large integer Or real values (but unlike 
        ''' quick sort, radix Is stable And supports all na.last options). The 
        ''' implementation Is orders of magnitude faster than shell sort for character 
        ''' vectors, in part thanks to clever use of the internal CHARSXP table.
        ''' However, there are some caveats with the radix sort
        ''' If x Is a character vector, all elements must share the same encoding. 
        ''' Only UTF-8 (including ASCII) And Latin-1 encodings are supported. Collation 
        ''' always follows the "C" locale.
        ''' Long vectors(with more than 2^32 elements) And complex vectors are Not 
        ''' supported yet.
        ''' </remarks>
        ''' <returns>
        ''' For sort, the result depends on the S3 method which is dispatched. If 
        ''' x does not have a class sort.int is used and it description applies. 
        ''' For classed objects which do not have a specific method the default method 
        ''' will be used and is equivalent to x[order(x, ...)]: this depends on the 
        ''' class having a suitable method for [ (and also that order will work, 
        ''' which requires a xtfrm method).
        ''' For sort.int the value Is the sorted vector unless index.return Is true, 
        ''' when the result Is a list with components named x And ix containing the 
        ''' sorted numbers And the ordering index vector. In the latter case, if 
        ''' method == "quick" ties may be reversed in the ordering (unlike sort.list) 
        ''' as quicksort Is Not stable. For method == "radix", index.return Is 
        ''' supported for all na.last modes. The other methods only support index.return 
        ''' when na.last Is NA. The index vector refers To element numbers after removal 
        ''' Of NAs: see order If you want the original element numbers.
        ''' All attributes are removed from the Return value (see Becker et al, 1988, 
        ''' p.146) except names, which are sorted. (If Partial Is specified even the 
        ''' names are removed.) Note that this means that the returned value has no 
        ''' Class, except For factors And ordered factors (which are treated specially 
        ''' And whose result Is transformed back To the original Class).
        ''' </returns>
        <ExportAPI("sort")>
        Public Function sort(<RRawVectorArgument>
                             x As Object,
                             Optional decreasing As Boolean = False,
                             Optional na_last As Boolean = False) As Object

            Return orderBy(x, desc:=decreasing)
        End Function

        ''' <summary>
        ''' Sorts the elements of a sequence in ascending order according to a key.
        ''' </summary>
        ''' <param name="sequence">A sequence of values to order.</param>
        ''' <param name="getKey">
        ''' A function to extract a key from an element. and this parameter value 
        ''' can also be the field name or column name to sort.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' An System.Linq.IOrderedEnumerable`1 whose elements are sorted according 
        ''' to a key. The sort result could be situations:
        ''' 
        ''' 1. a vector which is sort by the element evaluated value
        ''' 2. a list which is sort by the specific element value
        ''' 3. a dataframe which is sort its rows by a specific column value
        ''' 
        ''' </returns>
        <ExportAPI("orderBy")>
        Public Function orderBy(<RRawVectorArgument>
                                sequence As Object,
                                Optional getKey As Object = Nothing,
                                Optional desc As Boolean = False,
                                Optional envir As Environment = Nothing) As Object

            If TypeOf getKey Is String Then
                Return sortByKeyValue(sequence, getKey, desc, envir)
            ElseIf getKey Is Nothing OrElse getKey.GetType.ImplementInterface(Of RFunction) Then
                Return sortByKeyFunction(sequence, getKey, desc, envir)
            Else
                Return Message.InCompatibleType(GetType(RFunction), getKey.GetType, envir)
            End If
        End Function

        Private Function sortByKeyValue(sequence As Object, key As String, desc As Boolean, envir As Environment) As Object
            If TypeOf sequence Is dataframe Then
                Dim rows = DirectCast(sequence, dataframe) _
                    .listByRows.slots _
                    .ToArray

                If desc Then
                    rows = rows _
                        .OrderByDescending(
                            Function(i)
                                Return DirectCast(i.Value, list).slots()(key)
                            End Function) _
                        .ToArray
                Else
                    rows = rows _
                        .OrderBy(Function(i) DirectCast(i.Value, list).slots()(key)) _
                        .ToArray
                End If

                ' re-assemble back the row list
                ' to dataframe
                Dim columns As New Dictionary(Of String, Array)
                Dim colNames = DirectCast(sequence, dataframe).colnames
                Dim v As Array

                For Each name As String In colNames
                    v = rows _
                        .Select(Function(i)
                                    Return DirectCast(i.Value, list).slots()(name)
                                End Function) _
                        .ToArray
                    v = REnv.TryCastGenericArray(v, envir)
                    columns(name) = v
                Next

                Return New dataframe With {
                    .columns = columns,
                    .rownames = rows _
                        .Select(Function(r) r.Key) _
                        .ToArray
                }
            Else
                Return Internal.debug.stop(New NotImplementedException(), envir)
            End If
        End Function

        Private Function sortByKeyFunction(sequence As Object, getKey As RFunction, desc As Boolean, envir As Environment) As Object
            Dim result As Array
            Dim measure = tryKeyBy(getKey, envir)

            If measure Like GetType(Message) Then
                Return measure.TryCast(Of Message)
            End If

            Dim err As Message = Nothing
            Dim projectList = measure.TryCast(Of Func(Of Object, Object)).produceKeyedSequence(sequence, envir, err)

            If Not err Is Nothing Then
                Return err
            End If

            If desc Then
                result = projectList _
                        .OrderByDescending(Function(o) o.key) _
                        .Select(Function(a) a.obj) _
                        .ToArray
            Else
                result = projectList _
                        .OrderBy(Function(o) o.key) _
                        .Select(Function(a) a.obj) _
                        .ToArray
            End If

            If TypeOf sequence Is list Then
                Dim ref = DirectCast(sequence, list).slots
                Dim orderList As New list With {
                    .slots = New Dictionary(Of String, Object)
                }

                For Each key As String In result.AsObjectEnumerator(Of String)
                    Call orderList.slots.Add(key, ref(key))
                Next

                Return orderList
            Else
                Return result
            End If
        End Function

        ''' <summary>
        ''' reverse a given sequence
        ''' </summary>
        ''' <param name="sequence"></param>
        ''' <returns></returns>
        <ExportAPI("reverse")>
        Public Function reverse(<RRawVectorArgument> sequence As Object) As Object
            Dim array As Array = REnv.asVector(Of Object)(sequence)
            Call Array.Reverse(array)
            Return array
        End Function

        ''' <summary>
        ''' # Are Some Values True?
        ''' 
        ''' Given a set of logical vectors, is at least one of the values true?
        ''' </summary>
        ''' <param name="test">
        ''' zero or more logical vectors. Other objects of zero length are ignored, 
        ''' and the rest are coerced to logical ignoring any class.
        ''' </param>
        ''' <param name="narm">
        ''' logical. If true NA values are removed before the result Is computed.
        ''' </param>
        ''' <returns>
        ''' The value is a logical vector of length one.
        '''
        ''' Let x denote the concatenation of all the logical vectors in ... 
        ''' (after coercion), after removing NAs if requested by na.rm = TRUE.
        ''' 
        ''' The value returned Is True If at least one Of the values In x Is True, 
        ''' And False If all Of the values In x are False (including If there are 
        ''' no values). Otherwise the value Is NA (which can only occur If 
        ''' na.rm = False And ... contains no True values And at least one NA 
        ''' value).
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("any")>
        Public Function any(<RRawVectorArgument> test As Object, Optional narm As Boolean = False) As Boolean
            Return CLRVector.asLogical(test).Any(Function(b) b = True)
        End Function

        ''' <summary>
        ''' # Are All Values True?
        ''' 
        ''' Given a set of logical vectors, are all of the values true?
        ''' </summary>
        ''' <param name="test">zero or more logical vectors. Other objects of zero 
        ''' length are ignored, and the rest are coerced to logical ignoring any 
        ''' class.</param>
        ''' <param name="narm">
        ''' logical. If true NA values are removed before the result is computed.
        ''' </param>
        ''' <returns>
        ''' The value is a logical vector of length one.
        '''
        ''' Let x denote the concatenation of all the logical vectors in ... 
        ''' (after coercion), after removing NAs if requested by na.rm = TRUE.
        '''
        ''' The value returned Is True If all Of the values In x are True 
        ''' (including If there are no values), And False If at least one Of 
        ''' the values In x Is False. Otherwise the value Is NA (which can 
        ''' only occur If na.rm = False And ... contains no False values And 
        ''' at least one NA value).
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("all")>
        Public Function all(<RRawVectorArgument> test As Object, Optional narm As Boolean = False) As Boolean
            Return CLRVector.asLogical(test).All(Function(b) b = True)
        End Function

        <ExportAPI("while")>
        Public Function doWhile(<RRawVectorArgument>
                                seq As Object,
                                predicate As RFunction,
                                Optional action$ = "take",
                                Optional env As Environment = Nothing) As Object

            Throw New NotImplementedException
        End Function

        <Extension>
        Private Function splitByPartitionSize(env As Environment, x As Object, argv As list) As Object
            Dim vecSize As Integer = base.length(x)
            Dim size As Integer

            If argv.hasName("parts") Then
                Dim parts = argv.getValue("parts", env, [default]:=0)

                If parts = 0 Then
                    Return Internal.debug.stop("invalid parameter value parts!", env)
                Else
                    size = vecSize / parts
                End If
            Else
                size = argv.getValue("size", env, [default]:=0)

                If size = 0 Then
                    Return Internal.debug.stop("invalid parameter value size!", env)
                End If
            End If

            If TypeOf x Is list Then
                ' split key set and then break the list into multiple parts
                Return DirectCast(x, list).splitList(size)
            Else
                Return pipeline _
                    .TryCreatePipeline(Of Object)(x, env) _
                    .splitVector(size, env)
            End If
        End Function

        <Extension>
        Private Function splitVector(x As pipeline, size As Integer, env As Environment) As Object
            Dim vec As Object() = x.populates(Of Object)(env).ToArray
            Dim matrix As Object()() = vec.Split(partitionSize:=size)
            Dim list As New list With {.slots = New Dictionary(Of String, Object)}

            For i As Integer = 0 To matrix.Length - 1
                Call list.add($"`{i + 1}`", REnv.TryCastGenericArray(matrix(i), env))
            Next

            Return list
        End Function

        <Extension>
        Private Function splitList(x As list, size As Integer) As Object
            Dim keys As String()() = x.getNames.Split(partitionSize:=size)
            Dim list As New list With {.slots = New Dictionary(Of String, Object)}

            For i As Integer = 0 To keys.Length - 1
                Call list.add($"`{i + 1}`", x.getByName(keys(i)))
            Next

            Return list
        End Function

        Private Class SplitPredicateFunction

            Public _error As Message = Nothing
            Public delimiter As RFunction
            Public env As Environment
            Public testObject As Object

            Public Function GetPredicate() As Predicate(Of Object)
                If delimiter Is Nothing Then
                    Return AddressOf AssertEquals
                Else
                    Return AddressOf AssertThat
                End If
            End Function

            Public Function AssertThat(obj As Object) As Boolean
                If Not _error Is Nothing Then
                    Return False
                End If

                Dim out As Object = DirectCast(delimiter, RFunction).Invoke(env, invokeArgument(obj))

                If TypeOf out Is Message Then
                    _error = out
                    Return False
                End If

                Dim sng As Object = REnv.single(out)
                Dim flag As Object = Converts.RCType.CTypeDynamic(sng, GetType(Boolean), env)

                Return DirectCast(flag, Boolean)
            End Function

            Public Function AssertEquals(obj As Object) As Boolean
                Return obj Is testObject
            End Function
        End Class

        ''' <summary>
        ''' split content sequence with a given condition as element delimiter.
        ''' </summary>
        ''' <param name="x">a given data sequence</param>
        ''' <param name="delimiter">
        ''' an element test function to determine that element is a delimiter object
        ''' </param>
        ''' <param name="argv">
        ''' + parts: will split the input sequence into n(parts = n) multiple parts
        ''' + size: will split the input sequence into m multiple parts, and each part size is n(size=n)
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the generated result is different between the vector/list:
        ''' 
        ''' + for vector data: split the value array directly
        ''' + for list data: split the list keys array and then break the input list 
        '''       data into multiple parts by keys
        ''' </remarks>
        <ExportAPI("split")>
        Public Function split(<RRawVectorArgument> x As Object,
                              Optional delimiter As Object = Nothing,
                              <RListObjectArgument>
                              Optional argv As list = Nothing,
                              Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Call env.AddMessage("the given object x to split can not be nothing")
                Return Nothing
            End If

            Dim obj_type As Type = x.GetType
            Dim generic_split As GenericFunction = Nothing

            If generic.exists(x, "split", obj_type, env, generic_split) Then
                Return generic_split(x, argv, env)
            Else
                ' is a collection
                Return splitCollection(x, delimiter, argv, env)
            End If
        End Function

        Private Function splitCollection(x As Object, delimiter As Object, argv As list, env As Environment) As Object
            Dim _split As SplitPredicateFunction

            If delimiter Is Nothing Then
                If argv.hasName("parts") OrElse argv.hasName("size") Then
                    Return env.splitByPartitionSize(x, argv)
                Else
                    Return Internal.debug.stop("the required delimiter value can not be nothing!", env)
                End If
            ElseIf delimiter.GetType.ImplementInterface(Of RFunction) Then
                _split = New SplitPredicateFunction With {.delimiter = delimiter, .env = env}
            Else
                _split = New SplitPredicateFunction With {.testObject = delimiter}
            End If

            Dim seq As pipeline = pipeline.TryCreatePipeline(Of Object)(x, env)
            Dim blocks() = seq.populates(Of Object)(env) _
                .Split(_split.GetPredicate) _
                .Select(Function(block, i)
                            Return New Group With {.group = block, .key = i + 1}
                        End Function) _
                .ToArray

            If Not _split._error Is Nothing Then
                Return _split._error
            Else
                Return blocks
            End If
        End Function

        <ExportAPI("rotate_right")>
        Public Function rotate_right(<RRawVectorArgument> x As Object, i As Integer, Optional env As Environment = Nothing) As Object
            Dim vec As Object() = REnv.asVector(x, GetType(Object), env)
            vec.RotateRight(i)
            Return REnv.TryCastGenericArray(vec, env)
        End Function

        <ExportAPI("rotate_left")>
        Public Function rotate_left(<RRawVectorArgument> x As Object, i As Integer, Optional env As Environment = Nothing) As Object
            Dim vec As Object() = REnv.asVector(x, GetType(Object), env)
            vec.RotateLeft(i)
            Return REnv.TryCastGenericArray(vec, env)
        End Function

        ''' <summary>
        ''' ### Keep or drop columns using their names and types
        ''' 
        ''' Select (and optionally rename) variables in a data frame, using a 
        ''' concise mini-language that makes it easy to refer to variables 
        ''' based on their name (e.g. a:f selects all columns from a on the 
        ''' left to f on the right) or type (e.g. where(is.numeric) selects all
        ''' numeric columns).
        ''' </summary>
        ''' <param name="_data">
        ''' A data frame, data frame extension (e.g. a tibble), or a lazy data 
        ''' frame (e.g. from dbplyr or dtplyr). See Methods, below, for more 
        ''' details.
        ''' </param>
        ''' <param name="selectors">
        ''' &lt;tidy-select> One or more unquoted expressions separated by commas. 
        ''' Variable names can be used as if they were positions in the data frame, 
        ''' so expressions like x:y can be used to select a range of variables.
        ''' 
        ''' syntax for the selectors:
        ''' 
        ''' 1. select by name: ``select(name1, name2)``
        ''' 2. field renames: ``select(name1 -> data1)``
        ''' </param>
        ''' <param name="strict">
        ''' By default when this function running in strict mode, an error message 
        ''' will be returned if there is a missing data fields exists in the selector
        ''' list
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' An object of the same type as .data. The output has the following properties:
        '''
        ''' 1. Rows are Not affected.
        ''' 2. Output columns are a subset Of input columns, potentially With a 
        '''    different order. Columns will be renamed If new_name = old_name 
        '''    form Is used.
        ''' 3. Data frame attributes are preserved.
        ''' 4. Groups are maintained; you can't select off grouping variables.
        ''' </returns>
        <ExportAPI("select")>
        <RApiReturn(GetType(dataframe))>
        Public Function [select](_data As dataframe,
                                 Optional strict As Boolean = True,
                                 <RListObjectArgument>
                                 Optional selectors As list = Nothing,
                                 Optional env As Environment = Nothing) As Object

            If _data Is Nothing Then
                Return Nothing
            End If

            Dim newDf As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = _data.rownames
            }
            Dim src_name As String
            Dim rename As String

            For Each f As NamedValue(Of Object) In selectors.namedValues
                If TypeOf f.Value Is String Then
                    src_name = CStr(f.Value)
                    rename = Nothing
                ElseIf TypeOf f.Value Is DeclareLambdaFunction Then
                    Dim lambda As DeclareLambdaFunction = DirectCast(f.Value, DeclareLambdaFunction)

                    rename = ValueAssignExpression.GetSymbol(lambda.closure)
                    src_name = lambda.parameterNames(Scan0)
                Else
                    Return Message.InCompatibleType(
                        require:=GetType(String),
                        given:=f.ValueType,
                        envir:=env,
                        message:=$"the data selector '{f.Name}' can not be interpreted!"
                    )
                End If

                If Not _data.hasName(src_name) Then
                    Dim message As String() = {
                        $"missing data field '{src_name}' in your dataframe!",
                        $"field: {src_name}"
                    }

                    If src_name = NameOf(strict) Then
                        Continue For
                    ElseIf Not strict Then
                        ' skip of the missing data field in the select function
                        ' when set strict parameter value to TRUE
                        Call warning(message, env)
                        Continue For
                    End If

                    Return Internal.debug.stop(message, env)
                ElseIf rename.StringEmpty Then
                    rename = src_name
                End If

                Call newDf.columns.Add(rename, _data(src_name))
            Next

            Return newDf
        End Function
    End Module
End Namespace

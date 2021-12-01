#Region "Microsoft.VisualBasic::dc4bc2bcc992b10e9d5e88f49dd02bdd, R#\Runtime\Internal\internalInvokes\Linq\linq.vb"

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

'     Module linq
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: all, any, doWhile, fastIndexing, first
'                   groupBy, groupsSummary, groupSummary, last, orderBy
'                   produceKeyedSequence, progress, projectAs, reverse, rotate_left
'                   rotate_right, runFilterPipeline, runWhichFilter, skip, split
'                   take, tryKeyBy, unique, where, whichMax
'                   whichMin
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set
Imports obj = Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.ComponentModel.Collection

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
        ''' 
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="by"></param>
        ''' <returns></returns>
        <ExportAPI("left_join")>
        Public Function left_join(left As dataframe, right As dataframe,
                                  Optional by_x As String = Nothing,
                                  Optional by_y As String = Nothing,
                                  Optional [by] As String = Nothing,
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
                Return Internal.debug.stop("missing primary key for join two dataframe!", env)
            Else
                keyX = left.getColumnVector(by_x)
                keyY = right.getColumnVector(by_y)
            End If

            Dim idx As Dictionary(Of String, Integer) = Index(Of String).Indexing(keyY)
            Dim i As Integer() = keyX _
                .Select(Function(key) idx.TryGetValue(key, [default]:=-1)) _
                .ToArray

            right = right.GetByRowIndex(i)
            left = New dataframe(left)

            For Each colName As String In right.colnames
                If left.hasName(colName) Then
                    left.columns.Add($"{colName}.1", right(colName))
                Else
                    left.columns.Add(colName, right(colName))
                End If
            Next

            Return left
        End Function

        ''' <summary>
        ''' apply for the pipeline progress report
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="msgFunc">a text message to display or function for show message</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("progress")>
        Public Function progress(<RRawVectorArgument> x As Object, msgFunc As Object, Optional env As Environment = Nothing) As Object
            If msgFunc Is Nothing Then
                Call base.print("", env)
            ElseIf TypeOf msgFunc Is String Then
                Call base.print(msgFunc, env)
            ElseIf msgFunc.GetType.ImplementInterface(Of RFunction) Then
                Call DirectCast(msgFunc, RFunction).Invoke({x}, env)
            Else
                Return Message.InCompatibleType(GetType(RFunction), msgFunc.GetType, env)
            End If

            Return x
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="mode"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("as.index")>
        Public Function fastIndexing(x As Array,
                                     <RRawVectorArgument(GetType(String))>
                                     Optional mode As Object = "any|character|numeric|integer",
                                     Optional env As Environment = Nothing) As Object
            Select Case obj.ToString(REnv.asVector(Of String)(mode).AsObjectEnumerator.DefaultFirst("any")).ToLower
                Case "any"
                    Throw New NotImplementedException
                Case "character"
                    Return REnv.asVector(Of String)(x).AsObjectEnumerator(Of String).Indexing
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        <ExportAPI("take")>
        Public Function take(<RRawVectorArgument> sequence As Object, n%, Optional env As Environment = Nothing) As Object
            If sequence Is Nothing Then
                Return Nothing
            ElseIf TypeOf sequence Is pipeline Then
                Return DirectCast(sequence, pipeline) _
                    .populates(Of Object)(env) _
                    .Take(n) _
                    .DoCall(Function(seq)
                                Return New pipeline(seq, DirectCast(sequence(), pipeline).elementType)
                            End Function)
            Else
                Return Rset.getObjectSet(sequence, env).Take(n).ToArray
            End If
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
                Return Rset.getObjectSet(sequence, env).Skip(n).ToArray
            End If
        End Function

        ''' <summary>
        ''' Returns distinct elements from a sequence by using a specified System.Collections.Generic.IEqualityComparer`1
        ''' to compare values.
        ''' </summary>
        ''' <param name="items">The sequence to remove duplicate elements from.</param>
        ''' <param name="getKey">An System.Collections.Generic.IEqualityComparer`1 to compare values.</param>
        ''' <param name="envir"></param>
        ''' <returns>An System.Collections.Generic.IEnumerable`1 that contains distinct elements from
        ''' the source sequence.</returns>
        <ExportAPI("unique")>
        Private Function unique(<RRawVectorArgument>
                                items As Object,
                                Optional getKey As RFunction = Nothing,
                                Optional envir As Environment = Nothing) As Object

            If Not getKey Is Nothing Then
                Return Rset.getObjectSet(items, envir) _
                   .GroupBy(Function(o)
                                Dim arg = InvokeParameter.CreateLiterals(o)
                                Return getKey.Invoke(envir, arg)
                            End Function) _
                   .Select(Function(g)
                               Return g.First
                           End Function) _
                   .ToArray
            Else
                Return Rset.getObjectSet(items, envir) _
                   .GroupBy(Function(o)
                                Return o
                            End Function) _
                   .Select(Function(g) g.Key) _
                   .ToArray
            End If
        End Function

        <ExportAPI("projectAs")>
        Private Function projectAs(<RRawVectorArgument>
                                   sequence As Object,
                                   project As RFunction,
                                   Optional envir As Environment = Nothing) As Object

            If sequence Is Nothing Then
                Return Nothing
            End If

            Dim doProject As Func(Of Object, Object) = Function(o) project.Invoke(envir, InvokeParameter.CreateLiterals(o))

            If TypeOf sequence Is pipeline Then
                ' run in pipeline mode
                Dim seq As pipeline = DirectCast(sequence, pipeline)
                Dim projection As IEnumerable(Of Object) = seq _
                    .populates(Of Object)(envir) _
                    .Select(doProject)

                Return New pipeline(projection, project.getReturns(envir))
            Else
                Dim result As Object() = Rset.getObjectSet(sequence, envir) _
                    .Select(doProject) _
                    .ToArray

                Return New vector(result, project.getReturns(envir))
            End If
        End Function

        ''' <summary>
        ''' The which test filter
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("which")>
        Private Function where(<RRawVectorArgument>
                               sequence As Object,
                               Optional test As Object = Nothing,
                               Optional pipelineFilter As Boolean = True,
                               Optional env As Environment = Nothing) As Object

            If test Is Nothing Then
                ' test for which index
                Return which.IsTrue(REnv.asLogical(sequence), offset:=1)
            ElseIf TypeOf sequence Is pipeline Then
                ' run in pipeline mode
                Return runFilterPipeline(sequence, test, pipelineFilter, env)
            Else
                Dim testResult = Rset.getObjectSet(sequence, env) _
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
            End If
        End Function

        Private Function runFilterPipeline(sequence As Object, test As Object, pipelineFilter As Boolean, env As Environment) As Object
            Return DirectCast(sequence, pipeline) _
                    .populates(Of Object)(env) _
                    .runWhichFilter(test, env) _
                    .DoCall(Function(seq)
                                If pipelineFilter Then
                                    Return Iterator Function() As IEnumerable(Of Object)
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
                                           End Function() _
                                       .DoCall(Function(pip)
                                                   Return New pipeline(pip, DirectCast(sequence, pipeline).elementType)
                                               End Function)
                                Else
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
                                End If
                            End Function)
        End Function

        <Extension>
        Private Iterator Function runWhichFilter(sequence As IEnumerable(Of Object), test As Object, env As Environment) As IEnumerable(Of Object)
            Dim predicate As Predicate(Of Object)

            If TypeOf test Is RFunction Then
                predicate = Function(item)
                                Dim arg = InvokeParameter.CreateLiterals(item)
                                Dim result = DirectCast(test, RFunction).Invoke(env, arg)

                                Return REnv.asLogical(result)(Scan0)
                            End Function
            ElseIf TypeOf test Is Predicate(Of Object) Then
                predicate = DirectCast(test, Predicate(Of Object))
            ElseIf TypeOf test Is Func(Of Object, Boolean) Then
                predicate = New Predicate(Of Object)(AddressOf DirectCast(test, Func(Of Object, Boolean)).Invoke)
            Else
                Yield Internal.debug.stop(Message.InCompatibleType(GetType(Predicate(Of Object)), test.GetType, env), env)
                Return
            End If

            For Each item As Object In sequence
                Yield (predicate(item), item)
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
        <ExportAPI("which.max")>
        <RApiReturn(GetType(Integer))>
        Public Function whichMax(<RRawVectorArgument> x As Object, Optional eval As Object = Nothing, Optional env As Environment = Nothing) As Object
            If eval Is Nothing Then
                If x Is Nothing Then
                    Return New Integer() {}
                Else
                    Dim dbl As Double() = DirectCast(asVector(Of Double)(Rset.getObjectSet(x, env).ToArray), Double())

                    If dbl.Length = 0 Then
                        Return New Integer() {}
                    Else
                        Return which.Max(dbl) + 1
                    End If
                End If
            Else
                Dim lambda = LinqPipeline.lambda.CreateProjectLambda(Of Double)(eval, env)
                Dim scores = Rset.getObjectSet(x, env).Select(lambda).ToArray

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
                    Dim dbl As Double() = DirectCast(asVector(Of Double)(Rset.getObjectSet(x, env)), Double())

                    If dbl.Length = 0 Then
                        Return New Integer() {}
                    Else
                        Return which.Min(dbl) + 1
                    End If
                End If
            Else
                Dim lambda = LinqPipeline.lambda.CreateProjectLambda(Of Double)(eval, env)
                Dim scores = Rset.getObjectSet(x, env).Select(lambda).ToArray

                If scores.Length = 0 Then
                    Return New Integer() {}
                Else
                    Return which.Min(scores) + 1
                End If
            End If
        End Function

        <ExportAPI("first")>
        Private Function first(<RRawVectorArgument>
                               sequence As Object,
                               Optional test As RFunction = Nothing,
                               Optional envir As Environment = Nothing) As Object

            Dim pass As Boolean
            Dim arg As InvokeParameter()

            If test Is Nothing Then
                Return Rset.getObjectSet(sequence, envir).FirstOrDefault
            End If

            For Each item As Object In Rset.getObjectSet(sequence, envir)
                arg = InvokeParameter.CreateLiterals(item)
                pass = REnv.asLogical(test.Invoke(envir, arg))(Scan0)

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
                Return Rset.getObjectSet(sequence, envir).LastOrDefault
            End If

            Dim lastVal As Object = Nothing

            For Each item As Object In Rset.getObjectSet(sequence, envir)
                arg = InvokeParameter.CreateLiterals(item)
                pass = REnv.asLogical(test.Invoke(envir, arg))(Scan0)

                If pass Then
                    lastVal = item
                End If
            Next

            Return lastVal
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sequence"></param>
        ''' <param name="getKey"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("groupBy")>
        Private Function groupBy(<RRawVectorArgument>
                                 sequence As Object,
                                 Optional getKey As Object = Nothing,
                                 Optional env As Environment = Nothing) As Object

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

            Return result
        End Function

        <Extension>
        Private Function produceKeyedSequence(keyBy As Func(Of Object, Object), sequence As Object, env As Environment, ByRef err As Message) As IEnumerable(Of (key As Object, obj As Object))
            Dim projectList As New List(Of (key As Object, obj As Object))
            Dim key As Object

            For Each item As Object In Rset.getObjectSet(sequence, env)
                key = keyBy(item)

                If Program.isException(key) Then
                    err = key
                    Exit For
                Else
                    projectList.Add((key, item))
                End If
            Next

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
        ''' Sorts the elements of a sequence in ascending order according to a key.
        ''' </summary>
        ''' <param name="sequence">A sequence of values to order.</param>
        ''' <param name="getKey">A function to extract a key from an element.</param>
        ''' <param name="envir"></param>
        ''' <returns>An System.Linq.IOrderedEnumerable`1 whose elements are sorted according to a
        ''' key.</returns>
        <ExportAPI("orderBy")>
        Public Function orderBy(<RRawVectorArgument>
                                sequence As Object,
                                Optional getKey As RFunction = Nothing,
                                Optional desc As Boolean = False,
                                Optional envir As Environment = Nothing) As Object

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

            Return result
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
            Return REnv.asLogical(test).Any(Function(b) b = True)
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
            Return REnv.asLogical(test).All(Function(b) b = True)
        End Function

        <ExportAPI("while")>
        Public Function doWhile(<RRawVectorArgument>
                                seq As Object,
                                predicate As RFunction,
                                Optional action$ = "take",
                                Optional env As Environment = Nothing) As Object

            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' split content sequence with a given condition as element delimiter.
        ''' </summary>
        ''' <param name="x">a given data sequence</param>
        ''' <param name="delimiter">an element test function to determine that element is a delimiter object</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("split")>
        Public Function split(<RRawVectorArgument> x As Object, delimiter As Object, Optional env As Environment = Nothing) As Object
            Dim seq As pipeline = pipeline.TryCreatePipeline(Of Object)(x, env)
            Dim _split As Predicate(Of Object)
            Dim _error As Message = Nothing

            If delimiter Is Nothing Then
                Return Internal.debug.stop("the required delimiter value can not be nothing!", env)
            ElseIf delimiter.GetType.ImplementInterface(Of RFunction) Then
                _split = Function(obj)
                             If Not _error Is Nothing Then
                                 Return False
                             End If

                             Dim out As Object = DirectCast(delimiter, RFunction).Invoke(env, invokeArgument(obj))

                             If TypeOf out Is Message Then
                                 _error = out
                                 Return False
                             End If

                             Dim sng As Object = REnv.single(out)
                             Dim flag As Object = RCType.CTypeDynamic(sng, GetType(Boolean), env)

                             Return DirectCast(flag, Boolean)
                         End Function
            Else
                _split = Function(obj) obj Is delimiter
            End If

            Dim blocks() = seq.populates(Of Object)(env) _
                .Split(_split) _
                .Select(Function(block, i)
                            Return New Group With {.group = block, .key = i + 1}
                        End Function) _
                .ToArray

            If Not _error Is Nothing Then
                Return _error
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
    End Module
End Namespace

#Region "Microsoft.VisualBasic::b3c449f8c26339d0b349897b77c7de09, R-sharp\R#\Runtime\Internal\internalInvokes\Linq\linq.vb"

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

'   Total Lines: 1206
'    Code Lines: 673
' Comment Lines: 401
'   Blank Lines: 132
'     File Size: 55.58 KB


'     Module linq
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: all, any, doWhile, fastIndexing, first
'                   getPredicate, groupBy, groupsSummary, groupSummary, last
'                   left_join, match, orderBy, produceKeyedSequence, progress
'                   projectAs, reverse, rotate_left, rotate_right, runFilterPipeline
'                   runWhichFilter, skip, sort, split, take
'                   tryKeyBy, unique, where, whichMax, whichMin
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Linq
Imports SMRUCC.Rsharp.Runtime.Interop
Imports obj = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set

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
                Call base.print("",, env)
            ElseIf TypeOf msgFunc Is String Then
                Call base.print(msgFunc,, env)
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

            Select Case obj.ToString(REnv.asVector(Of String)(mode).AsObjectEnumerator.DefaultFirst("any")).ToLower
                Case "any"
                    Throw New NotImplementedException
                Case "character"
                    Return REnv.asVector(Of String)(x).AsObjectEnumerator(Of String).Indexing
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
                    Return Rset.getObjectSet(sequence, env).Take(nscalar).ToArray
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

            Dim index As Index(Of String) = DirectCast(REnv.asVector(Of String)(table), String())
            Dim values As String() = REnv.asVector(Of String)(x)
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
                Return Rset.getObjectSet(sequence, env).Skip(n).ToArray
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
        <ExportAPI("which")>
        Private Function where(<RRawVectorArgument>
                               x As Object,
                               Optional test As Object = Nothing,
                               Optional pipelineFilter As Boolean = True,
                               Optional env As Environment = Nothing) As Object

            If test Is Nothing Then
                ' test for which index
                Return which.IsTrue(Vectorization.asLogical(x), offset:=1)
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

                For Each name As String In list.getNames
                    Dim testFlag As Boolean = predicate(list.getByName(name))

                    If testFlag Then
                        subKeys.add(name, list.getByName(name))
                    End If
                Next

                Return subKeys
            Else
                Dim testResult = Rset.getObjectSet(x, env) _
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

        Private Function runFilterPipeline(sequence As Object,
                                           test As Object,
                                           pipelineFilter As Boolean,
                                           env As Environment) As Object

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

        Private Function getPredicate(test As Object, env As Environment) As [Variant](Of Predicate(Of Object), Message)
            Dim predicate As Predicate(Of Object)

            If TypeOf test Is RFunction Then
                predicate = Function(item)
                                Dim arg = InvokeParameter.CreateLiterals(item)
                                Dim result = DirectCast(test, RFunction).Invoke(env, arg)

                                Return Vectorization.asLogical(result)(Scan0)
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
                pass = Vectorization.asLogical(test.Invoke(envir, arg))(Scan0)

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
                pass = Vectorization.asLogical(test.Invoke(envir, arg))(Scan0)

                If pass Then
                    lastVal = item
                End If
            Next

            Return lastVal
        End Function

        ''' <summary>
        ''' group vector/list by a given evaluator or group a dataframe rows
        ''' by the cell values of a specific column.
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

            If TypeOf sequence Is dataframe Then
                Dim table As dataframe = DirectCast(sequence, dataframe)
                Dim colName As String = DirectCast(REnv.asVector(Of String)(getKey), String()).GetValue(Scan0)

                Return New list(GetType(dataframe)) With {
                    .slots = table _
                        .groupBy(colName) _
                        .ToDictionary(Function(d) d.Key,
                                      Function(d)
                                          Return CObj(d.Value)
                                      End Function)
                }
            ElseIf TypeOf sequence Is Group Then
                sequence = DirectCast(sequence, Group).group
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

                For Each name As String In list.getNames
                    key = keyBy(list.slots(name))

                    If Program.isException(key) Then
                        err = key
                        Exit For
                    Else
                        projectList.Add((key, CObj(name)))
                    End If
                Next
            Else
                For Each item As Object In Rset.getObjectSet(sequence, env)
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
            Return Vectorization.asLogical(test).Any(Function(b) b = True)
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
            Return Vectorization.asLogical(test).All(Function(b) b = True)
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
        Private Function splitByPartitionSize(seq As pipeline, argv As list, env As Environment) As Object
            Dim vec As Object() = seq.populates(Of Object)(env).ToArray
            Dim size As Integer

            If argv.hasName("parts") Then
                Dim parts = argv.getValue("parts", env, [default]:=0)

                If parts = 0 Then
                    Return Internal.debug.stop("invalid parameter value parts!", env)
                Else
                    size = vec.Length / parts
                End If
            Else
                size = argv.getValue("size", env, [default]:=0)

                If size = 0 Then
                    Return Internal.debug.stop("invalid parameter value size!", env)
                End If
            End If

            Dim matrix As Object()() = vec.Split(partitionSize:=size)
            Dim list As New list With {.slots = New Dictionary(Of String, Object)}

            For i As Integer = 0 To matrix.Length - 1
                Call list.add($"`{i + 1}`", matrix(i))
            Next

            Return list
        End Function

        ''' <summary>
        ''' split content sequence with a given condition as element delimiter.
        ''' </summary>
        ''' <param name="x">a given data sequence</param>
        ''' <param name="delimiter">an element test function to determine that element is a delimiter object</param>
        ''' <param name="argv">
        ''' + parts: will split the input sequence into n(parts = n) multiple parts
        ''' + size: will split the input sequence into m multiple parts, and each part size is n(size=n)
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("split")>
        Public Function split(<RRawVectorArgument> x As Object,
                              Optional delimiter As Object = Nothing,
                              <RListObjectArgument>
                              Optional argv As list = Nothing,
                              Optional env As Environment = Nothing) As Object

            Dim seq As pipeline = pipeline.TryCreatePipeline(Of Object)(x, env)
            Dim _split As Predicate(Of Object)
            Dim _error As Message = Nothing

            If delimiter Is Nothing Then
                If argv.hasName("parts") OrElse argv.hasName("size") Then
                    Return seq.splitByPartitionSize(argv, env)
                Else
                    Return Internal.debug.stop("the required delimiter value can not be nothing!", env)
                End If
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
                             Dim flag As Object = Converts.RCType.CTypeDynamic(sng, GetType(Boolean), env)

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

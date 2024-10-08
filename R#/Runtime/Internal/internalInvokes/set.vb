﻿#Region "Microsoft.VisualBasic::454f8ff7c7f5416387d6538796bd84f7, R#\Runtime\Internal\internalInvokes\set.vb"

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

    '   Total Lines: 653
    '    Code Lines: 367 (56.20%)
    ' Comment Lines: 202 (30.93%)
    '    - Xml Docs: 90.10%
    ' 
    '   Blank Lines: 84 (12.86%)
    '     File Size: 27.90 KB


    '     Module [set]
    ' 
    '         Function: combn, count, createLoop, createSet, crossing
    '                   duplicated, indexOf, intersect, jaccard, match_val_against
    '                   rev, set_diff, set_equals, set_intersect, set_not_equals
    '                   set_ratio, set_union, setdiff, table, union
    '                   unset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports collectionSet = Microsoft.VisualBasic.ComponentModel.DataStructures.Set
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' Set Operations
    ''' </summary>
    ''' 
    <Package("set")>
    Module [set]

        ''' <summary>
        ''' unset — Unset the feature slots value from a given variable
        ''' </summary>
        ''' <param name="x">
        ''' should be a tuple list object or dataframe object
        ''' </param>
        ''' <param name="args">the features names to deletes from the given object <paramref name="x"/>.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the orginal object will be modified by this function
        ''' </remarks>
        <ExportAPI("unset")>
        Public Function unset(x As Object,
                              <RListObjectArgument>
                              Optional args As list = Nothing,
                              Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return x
            End If

            Dim names As String()

            If args.slots.Count = 1 Then
                names = CLRVector.asCharacter(args.slots.First.Value)
            Else
                names = CLRVector.asCharacter(args.slots.Values)
            End If

            If names.IsNullOrEmpty Then
                Call env.AddMessage("no slot features name was provided to deletes...", MSG_TYPES.WRN)
                Return x
            End If

            If TypeOf x Is list Then
                Dim ls As list = x

                For Each name As String In names
                    Call ls.slots.Remove(name)
                Next

                x = ls
            ElseIf TypeOf x Is dataframe Then
                Dim df As dataframe = x

                For Each name As String In names
                    Call df.columns.Remove(name)
                Next

                x = df
            ElseIf x.GetType.ImplementInterface(GetType(IDictionary)) Then
                Dim dict As IDictionary = x
                Dim generics = x.GetType.GetGenericArguments

                If generics.Length > 0 Then
                    If Not generics(0) Is GetType(String) Then
                        GoTo type_err
                    End If
                End If

                ' key is string
                For Each name As String In names
                    Call dict.Remove(name)
                Next

                x = dict
            Else
type_err:
                Return Message.InCompatibleType(GetType(list), x.GetType, env)
            End If

            Return x
        End Function

        ''' <summary>
        ''' ### Cross Tabulation and Table Creation
        ''' 
        ''' table uses the cross-classifying factors to build
        ''' a contingency table of the counts at each combination 
        ''' of factor levels.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("table")>
        Public Function table(x As String(), Optional desc As Boolean? = Nothing) As Object
            Dim factors = x.GroupBy(Function(str) If(str, "")).ToArray

            If Not desc Is Nothing Then
                If CBool(desc) Then
                    factors = factors _
                        .OrderByDescending(Function(c) c.Count) _
                        .ToArray
                Else
                    factors = factors _
                        .OrderBy(Function(c) c.Count) _
                        .ToArray
                End If
            End If

            Dim total As Integer = x.Length
            Dim size As Integer() = factors.Select(Function(c) c.Count).ToArray
            Dim summary As New dataframe With {
                .columns = New Dictionary(Of String, Array) From {
                    {"factors", factors.Select(Function(c) c.Key).ToArray},
                    {"size", size},
                    {"prop", SIMD.Divide.int32_op_divide_int32_scalar(size, total)}
                }
            }

            Return summary
        End Function

        ''' <summary>
        ''' ### setdiff: Set Difference of Subsets
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <ExportAPI("setdiff")>
        Public Function setdiff(x As Array, y As Array, Optional env As Environment = Nothing) As Object
            Dim s1 As String() = CLRVector.asCharacter(x)
            Dim s2 As String() = CLRVector.asCharacter(y)
            Dim diffs As New List(Of String)
            Dim i1 = s1.Indexing
            Dim i2 = s2.Indexing

            Call diffs.AddRange(From s As String In s1 Where Not s Like i2)
            Call diffs.AddRange(From s As String In s2 Where Not s Like i1)

            Return diffs.ToArray
        End Function

        ''' <summary>
        ''' ## Reverse Elements
        ''' 
        ''' rev provides a reversed version of its argument. 
        ''' It is generic function with a default method for 
        ''' vectors and one for dendrograms.
        ''' 
        ''' Note that this Is no longer needed (nor efficient) 
        ''' For obtaining vectors sorted into descending order,
        ''' since that Is now rather more directly achievable 
        ''' by sort(x, decreasing = True).
        ''' </summary>
        ''' <param name="x">
        ''' a vector Or another Object For which reversal Is 
        ''' defined.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("rev")>
        Public Function rev(<RRawVectorArgument>
                            x As Object,
                            <RListObjectArgument>
                            Optional args As list = Nothing,
                            Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Call env.AddMessage("x is nothing for do sequence order reverse", MSG_TYPES.WRN)
                Return Nothing
            End If

            If TypeOf x Is list Then
                Dim reverseMapping As Boolean = args.getValue("mapping", env, [default]:=False)

                If reverseMapping Then
                    Dim newMap As New list With {.slots = New Dictionary(Of String, Object)}
                    Dim oldMap As list = DirectCast(x, list)

                    For Each name As String In oldMap.getNames
                        Dim str As String = oldMap.getValue(Of String)(name, env, [default]:=Nothing)

                        If str IsNot Nothing AndAlso Not newMap.hasName(str) Then
                            Call newMap.add(str, name)
                        End If
                    Next

                    Return newMap
                Else
                    Throw New NotImplementedException
                End If
            ElseIf TypeOf x Is vector Then
                Dim vec As vector = DirectCast(x, vector)
                Dim a As Array = Array.CreateInstance(vec.data.GetType.GetElementType, vec.length)

                Call Array.ConstrainedCopy(vec.data, Scan0, a, Scan0, vec.length)
                Call Array.Reverse(a)

                Return New vector() With {.data = a}
            ElseIf x.GetType.IsArray Then
                Dim vec As Array = DirectCast(x, Array)
                Dim a As Array = Array.CreateInstance(vec.GetType.GetElementType, vec.Length)

                Call Array.ConstrainedCopy(vec, Scan0, a, Scan0, vec.Length)
                Call Array.Reverse(a)

                Return New vector() With {.data = a}
            Else
                Throw New NotImplementedException
            End If
        End Function

        ''' <summary>
        ''' is a ``table`` liked function for count string occurance number
        ''' </summary>
        ''' <param name="str">
        ''' A character vector that may contains the
        ''' duplicated string value
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("count")>
        Public Function count(str As Array) As list
            Dim counts = CLRVector.asCharacter(str) _
                .GroupBy(Function(t) t) _
                .ToDictionary(Function(t) t.Key,
                              Function(t)
                                  Return CObj(t.Count)
                              End Function)
            Dim data As New list With {.slots = counts}

            Return data
        End Function

        ''' <summary>
        ''' Performs set intersection
        ''' </summary>
        ''' <param name="x">vectors (of the same mode) containing a sequence of items (conceptually) with no duplicated values.</param>
        ''' <param name="y">vectors (of the same mode) containing a sequence of items (conceptually) with no duplicated values.</param>
        ''' <returns></returns>
        <ExportAPI("intersect")>
        Public Function intersect(<RRawVectorArgument> x As Object,
                                  <RRawVectorArgument> y As Object,
                                  Optional env As Environment = Nothing) As Object

            Dim index_a As New Index(Of Object)(ObjectSet.GetObjectSet(x, env))
            Dim inter As Object() = index_a _
                .Intersect(collection:=ObjectSet.GetObjectSet(y, env)) _
                .Distinct _
                .ToArray

            Return inter
        End Function

        ''' <summary>
        ''' Performs set union
        ''' </summary>
        ''' <param name="x">vectors (of the same mode) containing a sequence of items (conceptually) with no duplicated values.</param>
        ''' <param name="y">vectors (of the same mode) containing a sequence of items (conceptually) with no duplicated values.</param>
        ''' <returns></returns>
        <ExportAPI("union")>
        Public Function union(<RRawVectorArgument> x As Object,
                              <RRawVectorArgument> y As Object,
                              Optional env As Environment = Nothing) As Object

            Dim join As Object() = ObjectSet.GetObjectSet(x, env) _
                .JoinIterates(ObjectSet.GetObjectSet(y, env)) _
                .Distinct _
                .ToArray
            Return join
        End Function

        ''' <summary>
        ''' Create the hash index for element search
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="getKey">extract a character vector from one of the elements 
        ''' inside the collection x, using as the index key.</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("index_of")>
        <RApiReturn(GetType(Index(Of String)))>
        Public Function indexOf(<RRawVectorArgument>
                                x As Object,
                                Optional getKey As Object = Nothing,
                                Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return Nothing
            ElseIf x.GetType Is GetType(vector) Then
                x = DirectCast(x, vector).data
            End If

            If x.GetType.IsArray Then
                x = REnv.TryCastGenericArray(x, env)
            End If

            Dim typeofX As Type = x.GetType

            If REnv.isVector(Of String)(x) Then
                Return CLRVector.asCharacter(x).Indexing
            Else
                Return Internal.debug.stop(New NotImplementedException(typeofX.FullName), env)
            End If
        End Function

        ''' <summary>
        ''' create subset of the given <paramref name="listSet"/> via 
        ''' tuple value match against with the given 
        ''' <paramref name="index_set"/>.
        ''' </summary>
        ''' <param name="listSet"></param>
        ''' <param name="index_set"></param>
        ''' <returns></returns>
        <ExportAPI("against")>
        Public Function match_val_against(listSet As list,
                                          <RRawVectorArgument>
                                          index_set As Object,
                                          Optional env As Environment = Nothing) As Object

            Dim filters As String() = CLRVector.asCharacter(index_set)
            Dim reshape = reshape2.flip_list(listSet, env)

            If Program.isException(reshape) Then
                Return reshape
            End If

            Dim keys As String() = filters _
                .Select(AddressOf DirectCast(reshape, list).getByName) _
                .Where(Function(a) Not a Is Nothing) _
                .Select(Function(a) CLRVector.asCharacter(a)) _
                .IteratesALL _
                .Distinct _
                .ToArray
            Dim subset As list = listSet.subset(keys)

            Return subset
        End Function

        <ExportAPI("loop")>
        Public Function createLoop(<RRawVectorArgument> x As Object) As RMethodInfo
            Dim loops As New LoopArray(Of Object)(asVector(Of Object)(x).AsObjectEnumerator)
            Dim populator As Func(Of Object) = AddressOf loops.Next

            Return New RMethodInfo(App.NextTempName, populator)
        End Function

        ''' <summary>
        ''' ### Determine Duplicate Elements
        ''' 
        ''' ``duplicated()`` determines which elements of a vector or data frame 
        ''' are duplicates of elements with smaller subscripts, and returns a 
        ''' logical vector indicating which elements (rows) are duplicates.
        ''' </summary>
        ''' <param name="x">a vector Or a data frame Or an array Or NULL.</param>
        ''' <returns>duplicated(): For a vector input, a logical vector of the 
        ''' same length as x. For a data frame, a logical vector with one element 
        ''' for each row. For a matrix or array, and when MARGIN = 0, a logical 
        ''' array with the same dimensions and dimnames.</returns>
        ''' <remarks>
        ''' These are generic functions with methods for vectors (including lists), data frames 
        ''' and arrays (including matrices).
        '''
        ''' For the default methods, And whenever there are equivalent method definitions for 
        ''' duplicated And anyDuplicated, anyDuplicated(x, ...) Is a “generalized” shortcut 
        ''' for any(duplicated(x, ...)), in the sense that it returns the index i of the first 
        ''' duplicated entry x[i] if there Is one, And 0 otherwise. Their behaviours may be 
        ''' different when at least one of duplicated And anyDuplicated has a relevant method.
        '''
        ''' duplicated(x, fromLast = TRUE) Is equivalent to but faster than rev(duplicated(rev(x))).
        '''
        ''' The data frame method works by pasting together a character representation Of the 
        ''' rows separated by \r, so may be imperfect If the data frame has characters With 
        ''' embedded carriage returns Or columns which Do Not reliably map To characters.
        '''
        ''' The array method calculates For Each element Of the Sub-array specified by MARGIN 
        ''' If the remaining dimensions are identical To those For an earlier (Or later, When 
        ''' fromLast = True) element (In row-major order). This would most commonly be used 
        ''' To find duplicated rows (the Default) Or columns (With MARGIN = 2). Note that 
        ''' MARGIN = 0 returns an array Of the same dimensionality attributes As x.
        '''
        ''' Missing values("NA") are regarded As equal, numeric And complex ones differing from 
        ''' NaN; character strings will be compared In a “common encoding”; For details, see 
        ''' match (And unique) which use the same concept.
        '''
        ''' Values in incomparables will never be marked as duplicated. This Is intended to be 
        ''' used for a fairly small set of values And will Not be efficient for a very large 
        ''' set.
        '''
        ''' When used on a data frame with more than one column, Or an array Or matrix when 
        ''' comparing dimensions of length greater than one, this tests for identity of character 
        ''' representations. This will catch people who unwisely rely on exact equality of 
        ''' floating-point numbers!
        '''
        ''' Except for factors, logical And raw vectors the default nmax = NA Is equivalent 
        ''' to nmax = length(x). Since a hash table of size 8*nmax bytes Is allocated, setting 
        ''' nmax suitably can save large amounts of memory. For factors it Is automatically 
        ''' set to the smaller of length(x) And the number of levels plus one (for NA). 
        ''' If nmax Is set too small there Is liable to be an error nmax = 1 Is silently 
        ''' ignored.
        '''
        ''' Long vectors are supported For the Default method Of duplicated, but may only be 
        ''' usable if nmax Is supplied.
        ''' </remarks>
        <ExportAPI("duplicated")>
        Public Function duplicated(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
            Dim checked As New Index(Of Object)
            Dim flags As New List(Of Boolean)

            For Each item As Object In ObjectSet.GetObjectSet(x, env)
                Call flags.Add(item Like checked)

                If Not flags.Last Then
                    Call checked.Add(item)
                End If
            Next

            Return flags.ToArray
        End Function

        ''' <summary>
        ''' Find Unique Combinations of All Elements 
        ''' from Two Vectors in R. Expand data frame 
        ''' to include all possible combinations of 
        ''' values.
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <ExportAPI("crossing")>
        Public Function crossing(a As Array, b As Array, Optional env As Environment = Nothing) As dataframe
            Dim combn As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }
            Dim al As New List(Of Object)
            Dim bl As New List(Of Object)

            For Each x1 As Object In a.AsObjectEnumerator
                For Each y1 As Object In b.AsObjectEnumerator
                    Call al.Add(x1)
                    Call bl.Add(y1)
                Next
            Next

            Call combn.columns.Add("vec1", REnv.TryCastGenericArray(al.ToArray, env))
            Call combn.columns.Add("vec2", REnv.TryCastGenericArray(bl.ToArray, env))

            Return combn
        End Function

        ''' <summary>
        ''' Generate All Combinations of n Elements, Taken m at a Time
        ''' 
        ''' Generate all combinations of the elements of x taken 
        ''' m at a time. If x is a positive integer, returns all 
        ''' combinations of the elements of seq(x) taken m at a 
        ''' time. If argument FUN is not NULL, applies a function 
        ''' given by the argument to each point. If simplify is 
        ''' FALSE, returns a list; otherwise returns an array, typically 
        ''' a matrix. ... are passed unchanged to the FUN function, 
        ''' if specified.
        ''' </summary>
        ''' <remarks>Factors x are accepted.</remarks>
        ''' <param name="x">
        ''' vector source For combinations, Or Integer n For x &lt;- seq_len(n).
        ''' </param>
        ''' <param name="m">
        ''' number of elements to choose.
        ''' </param>
        ''' <returns>
        ''' A list or array, see the simplify argument above. In the 
        ''' latter case, the identity dim(combn(n, m)) == c(m, choose(n, m)) 
        ''' holds.
        ''' </returns>
        <ExportAPI("combn")>
        Public Function combn(x As Array, m As Integer, Optional env As Environment = Nothing) As Object
            If x Is Nothing OrElse x.Length = 0 Then
                Return Nothing
            End If

            If x.Length = 1 Then
                Dim size As Object = x.GetValue(Scan0)

                If RType.GetRSharpType(size.GetType).mode = TypeCodes.integer Then
                    x = CLRVector.asInteger(size)(Scan0) _
                        .SeqIterator(offset:=1) _
                        .ToArray
                End If
            Else
                x = REnv.TryCastGenericArray(x, env)
            End If

            Dim all = x.AsObjectEnumerator(Of Object) _
                .ToArray _
                .AllCombinations(m) _
                .ToArray

            Dim type As RType = RType.GetRSharpType(x(0).GetType)

            If type.mode = TypeCodes.boolean OrElse
                type.mode = TypeCodes.double OrElse
                type.mode = TypeCodes.integer OrElse
                type.mode = TypeCodes.string Then

                ' dataframe with column size is m
                ' contains n combination rows
                Dim combines As New dataframe With {
                    .columns = New Dictionary(Of String, Array)
                }

#Disable Warning
                For i As Integer = 0 To m - 1
                    Call combines.add($"v{i + 1}", all.Select(Function(r) r(i)))
                Next
#Enable Warning

                Return combines
            Else
                ' list data
                Return New list With {
                    .slots = all _
                        .SeqIterator(offset:=1) _
                        .ToDictionary(Function(d) d.i.ToString,
                                      Function(d)
                                          Return CObj(d.value)
                                      End Function)
                }
            End If
        End Function

        ''' <summary>
        ''' The Jaccard Index, also known as the Jaccard similarity coefficient, 
        ''' is a statistic used in understanding the similarities between sample 
        ''' sets. The measurement emphasizes similarity between finite sample 
        ''' sets, and is formally defined as the size of the intersection divided
        ''' by the size of the union of the sample sets.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("jaccard")>
        Public Function jaccard(x As Array, y As Array, Optional env As Environment = Nothing) As Double
            If x Is Nothing OrElse x.Length = 0 Then
                Call env.AddMessage("vector set x is nothing, no intersection elements for count!", MSG_TYPES.WRN)
                Return 0
            ElseIf y Is Nothing OrElse y.Length = 0 Then
                Call env.AddMessage("vector set y is nothing, no intersection elements for count!", MSG_TYPES.WRN)
                Return 0
            End If

            x = REnv.TryCastGenericArray(x, env)
            y = REnv.TryCastGenericArray(y, env)

            If TypeOf x Is String() AndAlso TypeOf y Is String() Then
                Dim union = DirectCast(x, String()).JoinIterates(DirectCast(y, String())).Distinct.ToArray
                Dim ix = DirectCast(x, String()).Indexing
                Dim iy = DirectCast(y, String()).Indexing
                Dim intersect As String() = union _
                    .Where(Function(s) s Like ix AndAlso s Like iy) _
                    .ToArray

                Return intersect.Length / union.Length
            Else
                Dim set1 As New collectionSet(x.AsObjectEnumerator)
                Dim set2 As New collectionSet(y.AsObjectEnumerator)
                Dim intersect As Integer = (set1 And set2).Length
                Dim union As Integer = (set1 Or set2).Length

                Return intersect / union
            End If
        End Function

        ''' <summary>
        ''' create a collection set based on a given vector or tuple list
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="mode"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("as.set")>
        Public Function createSet(<RRawVectorArgument>
                                  x As Object,
                                  <RRawVectorArgument(TypeCodes.string)>
                                  Optional mode As Object = "any|character|integer|number|raw",
                                  Optional env As Environment = Nothing) As Object

            Dim modes As String() = CLRVector.asCharacter(mode)
            Dim mode_flag As String = modes.ElementAtOrDefault(0, [default]:="any")

            Select Case LCase(mode_flag)
                Case "character" : Return New collectionSet(CLRVector.asCharacter(x), Function(a, b) CStr(a) = CStr(b))
                Case "integer" : Return New collectionSet(CLRVector.asInteger(x), Function(a, b) CInt(a) = CInt(b))
                Case "number" : Return New collectionSet(CLRVector.asNumeric(x), Function(a, b) CDbl(a) = CDbl(b))
                Case "raw" : Return New collectionSet(CLRVector.asRawByte(x), Function(a, b) CByte(a) = CByte(b))
                Case Else
                    Return New collectionSet(CLRVector.asObject(x))
            End Select
        End Function

        <ROperator("+")>
        Public Function set_union(a As collectionSet, b As collectionSet) As collectionSet
            Return a Or b
        End Function

        <ROperator("-")>
        Public Function set_diff(a As collectionSet, b As collectionSet) As collectionSet
            Return a - b
        End Function

        <ROperator("&")>
        Public Function set_intersect(a As collectionSet, b As collectionSet) As collectionSet
            Return a And b
        End Function

        <ROperator("==")>
        Public Function set_equals(a As collectionSet, b As collectionSet) As Boolean
            Return a = b
        End Function

        <ExportAPI("!=")>
        Public Function set_not_equals(a As collectionSet, b As collectionSet) As Boolean
            Return Not (a = b)
        End Function

        ''' <summary>
        ''' equivalent to: length(a) / length(b)
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <ROperator("/")>
        Public Function set_ratio(a As collectionSet, b As collectionSet) As Double
            Return a.Length / b.Length
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::4e45a7b0b02b88ecea689d4eab94f3ad, R#\Runtime\Internal\internalInvokes\set.vb"

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

'     Module [set]
' 
'         Function: createLoop, duplicated, getObjectSet, indexOf, intersect
'                   union
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' Set Operations
    ''' </summary>
    Module [set]

        ''' <summary>
        ''' 将任意类型的序列输入转换为统一的对象枚举序列
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function getObjectSet(x As Object, env As Environment, Optional ByRef elementType As RType = Nothing) As IEnumerable(Of Object)
            If x Is Nothing Then
                Return {}
            End If

            Dim type As Type = x.GetType

            If type Is GetType(vector) Then
                With DirectCast(x, vector)
                    elementType = .elementType
                    Return .data.AsObjectEnumerator
                End With
            ElseIf type Is GetType(list) Then
                With DirectCast(x, list)
                    ' list value as sequence data
                    Dim raw As Object() = .slots.Values.ToArray
                    elementType = MeasureRealElementType(raw).DoCall(AddressOf RType.GetRSharpType)
                    Return raw.AsEnumerable
                End With
            ElseIf type.ImplementInterface(GetType(IDictionary(Of String, Object))) Then
                With DirectCast(x, IDictionary(Of String, Object))
                    Dim raw As Object() = .Values.AsEnumerable.ToArray
                    elementType = MeasureRealElementType(raw).DoCall(AddressOf RType.GetRSharpType)
                    Return raw.AsEnumerable
                End With
            ElseIf type.IsArray Then
                With DirectCast(x, Array)
                    elementType = .GetType.GetElementType.DoCall(AddressOf RType.GetRSharpType)
                    Return .AsObjectEnumerator
                End With
            ElseIf type Is GetType(pipeline) Then
                With DirectCast(x, pipeline)
                    elementType = .elementType
                    Return .populates(Of Object)(env)
                End With
            Else
                elementType = RType.GetRSharpType(x.GetType)
                Return {x}
            End If
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

            Dim index_a As New Index(Of Object)(getObjectSet(x, env))
            Dim inter As Object() = index_a _
                .Intersect(collection:=getObjectSet(y, env)) _
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

            Dim join As Object() = getObjectSet(x, env) _
                .JoinIterates(getObjectSet(y, env)) _
                .Distinct _
                .ToArray
            Return join
        End Function

        <ExportAPI("index.of")>
        Public Function indexOf(<RRawVectorArgument>
                                x As Object,
                                Optional getKey As Object = Nothing,
                                Optional env As Environment = Nothing) As Object

            If x Is Nothing Then
                Return Nothing
            ElseIf x.GetType Is GetType(vector) Then
                x = DirectCast(x, vector).data
            End If

            Dim typeofX As Type = x.GetType

            Throw New NotImplementedException
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
        Public Function duplicated(<RRawVectorArgument> x As Object) As Object
            Dim vec As Object() = REnv.asVector(Of Object)(x)
            Dim checked As New Index(Of Object)
            Dim flags As New List(Of Boolean)

            For Each item As Object In vec
                Call flags.Add(item Like checked)

                If Not flags.Last Then
                    Call checked.Add(item)
                End If
            Next

            Return flags.ToArray
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
                    x = DirectCast(REnv.asVector(Of Integer)(size), Integer())(Scan0) _
                        .SeqIterator(offset:=1) _
                        .ToArray
                End If
            Else
                x = REnv.TryCastGenericArray(x, env)
            End If

            Dim all = x.AsObjectEnumerator(Of Object) _
                .ToArray _
                .AllCombinations _
                .Where(Function(c) c.Length = m) _
                .ToArray
            Dim type As RType = RType.GetRSharpType(x(0))

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
                    Call combines.add($"v{i + 1}", all.Select(Function(r) r(i)).ToArray)
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
    End Module
End Namespace

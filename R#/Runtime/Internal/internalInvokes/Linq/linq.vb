#Region "Microsoft.VisualBasic::ff23d14df67d27ee10cc50e830d0b0c4, R#\Runtime\Internal\internalInvokes\Linq\linq.vb"

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
'         Function: all, any, doWhile, first, groupBy
'                   orderBy, projectAs, reverse, runWhichFilter, skip
'                   take, unique, where, whichMax, whichMin
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set

Namespace Runtime.Internal.Invokes.LinqPipeline

    ''' <summary>
    ''' Provides a set of static (Shared in Visual Basic) methods for querying objects
    ''' that implement System.Collections.Generic.IEnumerable`1.
    ''' </summary>
    <Package("linq", Category:=APICategories.SoftwareTools, Publisher:="xie.guigang@live.com")>
    Module linq

        <ExportAPI("take")>
        Public Function take(<RRawVectorArgument> sequence As Object, n%) As Object
            If sequence Is Nothing Then
                Return Nothing
            ElseIf TypeOf sequence Is pipeline Then
                Return DirectCast(sequence, pipeline) _
                    .populates(Of Object) _
                    .Take(n) _
                    .DoCall(Function(seq)
                                Return New pipeline(seq, DirectCast(sequence(), pipeline).elementType)
                            End Function)
            Else
                Return Rset.getObjectSet(sequence).Take(n).ToArray
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
        Public Function skip(<RRawVectorArgument> sequence As Object, n%) As Object
            If sequence Is Nothing Then
                Return Nothing
            ElseIf TypeOf sequence Is pipeline Then
                Return DirectCast(sequence, pipeline) _
                    .populates(Of Object) _
                    .Skip(n) _
                    .DoCall(Function(seq)
                                Return New pipeline(seq, DirectCast(sequence, pipeline).elementType)
                            End Function)
            Else
                Return Rset.getObjectSet(sequence).Skip(n).ToArray
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
                Return Rset.getObjectSet(items) _
                   .GroupBy(Function(o)
                                Dim arg = InvokeParameter.CreateLiterals(o)
                                Return getKey.Invoke(envir, arg)
                            End Function) _
                   .Select(Function(g)
                               Return g.First
                           End Function) _
                   .ToArray
            Else
                Return Rset.getObjectSet(items) _
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
                    .populates(Of Object) _
                    .Select(doProject)

                Return New pipeline(projection, project.returns)
            Else
                Dim result As Object() = Rset.getObjectSet(sequence) _
                    .Select(doProject) _
                    .ToArray

                Return New vector(result, project.returns)
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
                Return Which.IsTrue(REnv.asLogical(sequence), offset:=1)
            ElseIf TypeOf sequence Is pipeline Then
                ' run in pipeline mode
                Return runFilterPipeline(sequence, test, pipelineFilter, env)
            Else
                Dim testResult = Rset.getObjectSet(sequence) _
                    .runWhichFilter(test, env) _
                    .ToArray

                If pipelineFilter Then
                    Dim objs As New List(Of Boolean)

                    For Each obj In testResult
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

                    Return objs.ToArray
                Else
                    Dim booleans As New List(Of Boolean)

                    For Each obj In testResult
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
                    .populates(Of Object) _
                    .runWhichFilter(test, env) _
                    .DoCall(Function(seq)
                                If pipelineFilter Then
                                    Return Iterator Function() As IEnumerable(Of Object)
                                               For Each obj In seq
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
                                                   Return New pipeline(pip, DirectCast(sequence(), pipeline).elementType)
                                               End Function)
                                Else
                                    Dim booleans As New List(Of Boolean)

                                    For Each obj In seq
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

        <ExportAPI("which.max")>
        Public Function whichMax(<RRawVectorArgument> sequence As Object, Optional eval As Object = Nothing, Optional env As Environment = Nothing) As Integer
            If eval Is Nothing Then
                Return Which.Max(DirectCast(asVector(Of Double)(Rset.getObjectSet(sequence).ToArray), Double())) + 1
            Else
                Dim lambda = LinqPipeline.lambda.CreateProjectLambda(Of Double)(eval, env)
                Dim scores = Rset.getObjectSet(sequence).Select(lambda).ToArray

                Return Which.Max(scores) + 1
            End If
        End Function

        <ExportAPI("which.min")>
        Public Function whichMin(<RRawVectorArgument> sequence As Object, Optional eval As Object = Nothing, Optional env As Environment = Nothing) As Integer
            If eval Is Nothing Then
                Return Which.Min(DirectCast(asVector(Of Double)(Rset.getObjectSet(sequence)), Double())) + 1
            Else
                Dim lambda = LinqPipeline.lambda.CreateProjectLambda(Of Double)(eval, env)
                Dim scores = Rset.getObjectSet(sequence).Select(lambda).ToArray

                Return Which.Min(scores) + 1
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
                Return Rset.getObjectSet(sequence).FirstOrDefault
            End If

            For Each item As Object In Rset.getObjectSet(sequence)
                arg = InvokeParameter.CreateLiterals(item)
                pass = Runtime.asLogical(test.Invoke(envir, arg))(Scan0)

                If pass Then
                    Return item
                End If
            Next

            Return Nothing
        End Function

        <ExportAPI("groupBy")>
        Private Function groupBy(<RRawVectorArgument>
                                 sequence As Object,
                                 getKey As RFunction,
                                 Optional envir As Environment = Nothing) As Object

            Dim source As IEnumerable(Of Object) = Rset.getObjectSet(sequence)
            Dim result As Group() = source _
                .GroupBy(Function(o)
                             Dim arg = InvokeParameter.CreateLiterals(o)
                             Dim keyVal As Object = Runtime.single(getKey.Invoke(envir, arg))

                             Return keyVal
                         End Function) _
                .Select(Function(group)
                            Return New Group With {
                                .key = group.Key,
                                .group = group.ToArray
                            }
                        End Function) _
                .ToArray

            Return result
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

            Dim source As Object() = Rset.getObjectSet(sequence).ToArray
            Dim getKeyFunc As Func(Of Object, Object)
            Dim result As Array

            If getKey Is Nothing Then
                getKeyFunc = Function(o) o
            Else
                getKeyFunc = Function(o)
                                 Dim arg = InvokeParameter.CreateLiterals(o)
                                 Dim index As Object = getKey.Invoke(envir, arg)

                                 Return index
                             End Function
            End If

            If desc Then
                result = source _
                    .OrderByDescending(Function(o) Runtime.single(getKeyFunc(o))) _
                    .ToArray
            Else
                result = source _
                    .OrderBy(Function(o) Runtime.single(getKeyFunc(o))) _
                    .ToArray
            End If

            Return result
        End Function

        <ExportAPI("reverse")>
        Public Function reverse(<RRawVectorArgument> sequence As Object) As Object

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
            Return Runtime.asLogical(test).Any(Function(b) b = True)
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
            Return Runtime.asLogical(test).All(Function(b) b = True)
        End Function

        <ExportAPI("while")>
        Public Function doWhile(<RRawVectorArgument>
                                seq As Object,
                                predicate As RFunction,
                                Optional action$ = "take",
                                Optional env As Environment = Nothing) As Object

        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::31643bb76740e59001ce76e4dac65e63, R#\Runtime\Internal\internalInvokes\Linq\linq.vb"

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
    '         Function: first, groupBy, projectAs, unique, where
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rset = SMRUCC.Rsharp.Runtime.Internal.Invokes.set

Namespace Runtime.Internal.Invokes.LinqPipeline

    Module linq

        <ExportAPI("unique")>
        Private Function unique(<RRawVectorArgument> items As Object,
                                Optional getKey As RFunction = Nothing,
                                Optional envir As Environment = Nothing) As Object

            If Not getKey Is Nothing Then
                Return Rset.getObjectSet(items) _
                   .GroupBy(Function(o)
                                Dim arg = InvokeParameter.Create(o)
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
        Private Function projectAs(sequence As Array, project As RFunction, envir As Environment) As Object
            Dim doProject As Func(Of Object, Object) = Function(o) project.Invoke(envir, InvokeParameter.Create(o))
            Dim result As Object() = sequence _
                .AsObjectEnumerator _
                .Select(doProject) _
                .ToArray

            Return result
        End Function

        ''' <summary>
        ''' The which test filter
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("which")>
        Private Function where(sequence As Array,
                               Optional test As RFunction = Nothing,
                               Optional envir As Environment = Nothing) As Object

            If test Is Nothing Then
                ' test for which index
                Return Which.IsTrue(Runtime.asLogical(sequence))
            End If

            Dim pass As Boolean
            Dim arg As InvokeParameter()
            Dim filter As New List(Of Object)

            For Each item As Object In sequence
                arg = InvokeParameter.Create(item)
                pass = Runtime.asLogical(test.Invoke(envir, arg))(Scan0)

                If pass Then
                    Call filter.Add(item)
                End If
            Next

            Return filter.ToArray
        End Function

        <ExportAPI("first")>
        Private Function first(sequence As Array,
                               Optional test As RFunction = Nothing,
                               Optional envir As Environment = Nothing) As Object

            Dim pass As Boolean
            Dim arg As InvokeParameter()

            If test Is Nothing Then
                If sequence.Length = 0 Then
                    Return Nothing
                Else
                    Return sequence.GetValue(Scan0)
                End If
            End If

            For Each item As Object In sequence
                arg = InvokeParameter.Create(item)
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
                             Dim arg = InvokeParameter.Create(o)
                             Return getKey.Invoke(envir, arg)
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
    End Module
End Namespace

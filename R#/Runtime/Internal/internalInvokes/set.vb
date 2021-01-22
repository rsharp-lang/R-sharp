#Region "Microsoft.VisualBasic::76d485c980455abce6da2a81fde7d4ca, R#\Runtime\Internal\internalInvokes\set.vb"

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
    '         Function: createLoop, diff, getObjectSet, indexOf, intersect
    '                   union
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

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
        Public Function intersect(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object, Optional env As Environment = Nothing) As Object
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
        Public Function union(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object, Optional env As Environment = Nothing) As Object
            Dim join As Object() = getObjectSet(x, env) _
                .JoinIterates(getObjectSet(y, env)) _
                .Distinct _
                .ToArray
            Return join
        End Function

        <ExportAPI("diff")>
        Public Function diff(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object) As Object
            Throw New NotImplementedException
        End Function

        <ExportAPI("index.of")>
        Public Function indexOf(<RRawVectorArgument> x As Object, Optional getKey As Object = Nothing, Optional env As Environment = Nothing) As Object
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
    End Module
End Namespace

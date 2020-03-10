#Region "Microsoft.VisualBasic::9a205f9f05fdd7ea058afbd855776415, R#\Runtime\Internal\internalInvokes\set.vb"

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
    '         Function: getObjectSet, intersect, union
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
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
        Public Function getObjectSet(x As Object) As IEnumerable(Of Object)
            If x Is Nothing Then
                Return {}
            End If

            Dim type As Type = x.GetType

            If type Is GetType(vector) Then
                Return DirectCast(x, vector).data.AsObjectEnumerator
            ElseIf type Is GetType(list) Then
                ' list value as sequence data
                Return DirectCast(x, list).slots.Values.AsEnumerable
            ElseIf type.ImplementInterface(GetType(IDictionary(Of String, Object))) Then
                Return DirectCast(x, IDictionary(Of String, Object)).Values.AsEnumerable
            ElseIf type.IsArray Then
                Return DirectCast(x, Array).AsObjectEnumerator
            ElseIf type Is GetType(pipeline) Then
                Return DirectCast(x, pipeline).populates(Of Object)
            Else
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
        Public Function intersect(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object) As Object
            Dim index_a As New Index(Of Object)(getObjectSet(x))
            Dim inter As Object() = index_a _
                .Intersect(collection:=getObjectSet(y)) _
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
        Public Function union(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object) As Object
            Dim join As Object() = getObjectSet(x) _
                .JoinIterates(getObjectSet(y)) _
                .Distinct _
                .ToArray
            Return join
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::0c9dc8431ab14cfe57b2bc42a3290f1e, R#\Runtime\Internal\internalInvokes\Linq\search.vb"

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

    '   Total Lines: 58
    '    Code Lines: 47 (81.03%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (18.97%)
    '     File Size: 2.21 KB


    '     Module search
    ' 
    '         Function: binaryIndex, binarySearch, blockIndex, blockQuery
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Runtime.Internal.Invokes.LinqPipeline

    <Package("search")>
    Module search

        <ExportAPI("binarySearch")>
        Public Function binarySearch(v As BinarySearchFunction(Of Double, SeqValue(Of Double)), x As Double) As Integer
            Dim index As Integer = v.BinarySearch(x)

            If index = -1 Then
                Return -1
            Else
                Dim search As SeqValue(Of Double) = v(index)
                Dim i As Integer = search.i

                Return i
            End If
        End Function

        <ExportAPI("binaryIndex")>
        Public Function binaryIndex(v As Double()) As BinarySearchFunction(Of Double, SeqValue(Of Double))
            Dim i As SeqValue(Of Double)() = v _
                .SeqIterator(offset:=1) _
                .OrderBy(Function(a) a.value) _
                .ToArray
            Dim index As New BinarySearchFunction(Of Double, SeqValue(Of Double))(
                source:=i,
                key:=Function(a) a.value,
                compares:=Function(a, b) a.CompareTo(b)
            )

            Return index
        End Function

        <ExportAPI("blockQuery")>
        Public Function blockQuery(v As BlockSearchFunction(Of Object), x As Object) As Object()
            Return v.Search(x).ToArray
        End Function

        <ExportAPI("blockIndex")>
        Public Function blockIndex(v As Array,
                                   tolerance As Double,
                                   eval As DeclareLambdaFunction,
                                   Optional env As Environment = Nothing) As BlockSearchFunction(Of Object)

            Dim f As Func(Of Object, Double) = eval.CreateLambda(Of Object, Double)(env)
            Dim index As New BlockSearchFunction(Of Object)(v.AsObjectEnumerator, f, tolerance)

            Return index
        End Function
    End Module
End Namespace

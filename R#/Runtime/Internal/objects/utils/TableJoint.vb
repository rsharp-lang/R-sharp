#Region "Microsoft.VisualBasic::4d32d705fa8c23de87f1630ffe5c4d61, E:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/utils/TableJoint.vb"

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

    '   Total Lines: 52
    '    Code Lines: 44
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 2.40 KB


    '     Module TableJoint
    ' 
    '         Function: union
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.Utils

    Module TableJoint

        <ExportAPI("union_joint")>
        Public Function union(x As dataframe, y As dataframe, Optional null As Object = Nothing, Optional env As Environment = Nothing) As dataframe
            If x Is Nothing Then
                Return y
            ElseIf y Is Nothing Then
                Return x
            End If

            Dim unionSet As New dataframe
            Dim colleft As String() = x.columns.Keys.ToArray
            Dim colright As String() = y.columns.Keys.ToArray
            Dim left = TableRow.CreateRows(x, colleft).GroupBy(Function(i) i.id).ToDictionary(Function(i) i.Key, Function(i) i.First)
            Dim right = TableRow.CreateRows(y, colright).GroupBy(Function(i) i.id).ToDictionary(Function(i) i.Key, Function(i) i.First)
            Dim unionId = left.Keys.AsList + right.Keys
            Dim unionFeatures = (colleft.AsList + colright) _
                .makeNames(unique:=True) _
                .Select(Function(vec) (vec, New List(Of Object))) _
                .ToArray
            Dim emptyLeft As New TableRow With {.cells = Repeats(Of Object)(null, colleft.Length)}
            Dim emptyRight As New TableRow With {.cells = Repeats(Of Object)(null, colright.Length)}

            For Each id As String In unionId
                Dim cl As TableRow = left.TryGetValue(id, [default]:=emptyLeft)
                Dim cr As TableRow = right.TryGetValue(id, [default]:=emptyRight)

                For i As Integer = 0 To colleft.Length - 1
                    unionFeatures(i).Item2.Add(cl.cells(i))
                Next
                For j As Integer = 0 To colright.Length - 1
                    unionFeatures(j + colleft.Length).Item2.Add(cr.cells(j))
                Next
            Next

            unionSet.columns = unionFeatures _
                .ToDictionary(Function(vec) vec.Item1,
                              Function(vec)
                                  Return DirectCast(REnv.TryCastGenericArray(vec.Item2.ToArray, env), Array)
                              End Function)
            unionSet.rownames = unionId

            Return unionSet
        End Function
    End Module
End Namespace

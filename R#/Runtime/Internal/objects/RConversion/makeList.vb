#Region "Microsoft.VisualBasic::be37b612e4cdb96ebb2579b8f238b3d4, R#\Runtime\Internal\objects\RConversion\makeList.vb"

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

    '   Total Lines: 96
    '    Code Lines: 75 (78.12%)
    ' Comment Lines: 7 (7.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (14.58%)
    '     File Size: 3.50 KB


    '     Module makeList
    ' 
    '         Function: listByColumns, (+2 Overloads) listByRows
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Object.Converts

    Module makeList

        <Extension>
        Public Function listByColumns(data As dataframe) As list
            Return New list With {
                .slots = data.columns _
                    .ToDictionary(Function(t) t.Key,
                                  Function(t)
                                      Return CObj(t.Value)
                                  End Function)
            }
        End Function

        ''' <summary>
        ''' cast the dataframe object to row list
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns>
        ''' row list wrap the column list
        ''' </returns>
        <Extension>
        Public Function listByRows(data As dataframe) As list
            Dim rows As New Dictionary(Of String, Object)
            Dim columns As String() = data.columns.Keys.ToArray
            Dim getVals As New Dictionary(Of String, Func(Of Integer, Object))

            For Each key As String In columns
                Dim val As Array = data.columns(key)

                If val.Length = 1 Then
                    Dim first As Object = val.GetValue(Scan0)
                    getVals.Add(key, Function() first)
                Else
                    getVals.Add(key, Function(i) val.GetValue(i))
                End If
            Next

            Dim index As Integer
            Dim row As Dictionary(Of String, Object)
            Dim rownames As String() = If(
                data.rownames.IsNullOrEmpty,
                data.nrows.Sequence(offset:=1).Select(Function(i) $"[[{i}]]").ToArray,
                data.rownames.uniqueNames
            )

            For i As Integer = 0 To data.nrows - 1
                index = i
                row = columns.ToDictionary(Function(col) col, Function(col) getVals(col)(index))
                rows.Add(rownames(i), New list With {.slots = row})
            Next

            Return New list With {.slots = rows}
        End Function

        <Extension>
        Public Function listByRows(df As dataframe, names As String, env As Environment) As Object
            Dim list As list = df.listByRows
            Dim obj As Object
            Dim nameVec As String()

            If Not names.StringEmpty Then
                If df.hasName(names) Then
                    nameVec = CLRVector.asCharacter(df.columns(names))
                    nameVec = nameVec.uniqueNames
                    obj = list.setNames(nameVec, env)

                    If TypeOf obj Is Message Then
                        Return obj
                    End If
                Else
                    Return Internal.debug.stop({
                        $"undefined column '{names}' that selected for used as list names!",
                        $"column: {names}"
                    }, env)
                End If
            ElseIf Not df.rownames.IsNullOrEmpty Then
                nameVec = df.rownames
                nameVec = nameVec.uniqueNames
                obj = list.setNames(nameVec, env)

                If TypeOf obj Is Message Then
                    Return obj
                End If
            End If

            Return list
        End Function
    End Module
End Namespace

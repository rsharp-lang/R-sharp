#Region "Microsoft.VisualBasic::e10784feb4a7e548c3ee0dda6dfd3b7d, Library\base\dataframe\CharacterTable.vb"

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

    '   Total Lines: 104
    '    Code Lines: 78 (75.00%)
    ' Comment Lines: 8 (7.69%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 18 (17.31%)
    '     File Size: 3.68 KB


    ' Class CharacterTable
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GenericEnumerator, getColumn, getRow, getRowNames
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports renv = SMRUCC.Rsharp.Runtime

''' <summary>
''' a collection of the <see cref="EntityObject"/>.
''' </summary>
''' <remarks>
''' a dataframe liked object that store data in rows
''' </remarks>
Public Class CharacterTable : Implements IdataframeReader, Enumeration(Of EntityObject)

    ReadOnly rows As EntityObject()

    Default Public ReadOnly Property GetVector(field As String) As String()
        Get
            Return rows.Select(Function(r) r(field)).ToArray
        End Get
    End Property

    Sub New(rows As IEnumerable(Of EntityObject))
        Me.rows = rows.SafeQuery.ToArray
    End Sub

    Public Function getColumn(index As Object, env As Environment) As Object Implements IdataframeReader.getColumn
        Dim names As String() = CLRVector.asCharacter(index)
        Dim project As EntityObject() = New EntityObject(rows.Length - 1) {}
        Dim r As EntityObject

        For i As Integer = 0 To project.Length - 1
            r = rows(i)
            project(i) = New EntityObject With {
                .ID = r.ID,
                .Properties = names _
                    .ToDictionary(Function(v) v,
                                  Function(v)
                                      Return r(v)
                                  End Function)
            }
        Next

        Return New CharacterTable(project)
    End Function

    Public Function getRow(index As Object, env As Environment) As Object Implements IdataframeReader.getRow
        Dim vec As Object = renv.TryCastGenericArray(index, env)

        If TypeOf vec Is Message Then
            Return vec
        End If

        Dim offsets As Array = vec
        Dim slice As EntityObject()

        If offsets.GetType.GetElementType Is GetType(String) Then
            ' get by id
            slice = CLRVector.asCharacter(offsets) _
                .Select(Function(id)
                            Return rows.AsParallel _
                                .Where(Function(r) r.ID = id) _
                                .FirstOrDefault
                        End Function) _
                .Where(Function(r) Not r Is Nothing) _
                .ToArray
        ElseIf offsets.GetType.GetElementType Is GetType(Boolean) Then
            Dim list As New List(Of EntityObject)
            Dim bools As Boolean() = CLRVector.asLogical(offsets)

            For i As Integer = 0 To rows.Length - 1
                If bools(i) Then
                    Call list.Add(rows(i))
                End If
            Next

            slice = list.ToArray
        Else
            ' 1-based
            Dim ints As Integer() = CLRVector.asInteger(offsets)

            slice = New EntityObject(ints.Length - 1) {}

            For i As Integer = 0 To ints.Length - 1
                slice(i) = rows(ints(i) - 1)
            Next
        End If

        Return New CharacterTable(slice)
    End Function

    Public Function getRowNames() As String() Implements IdataframeReader.getRowNames
        Return rows.Keys
    End Function

    Public Iterator Function GenericEnumerator() As IEnumerator(Of EntityObject) Implements Enumeration(Of EntityObject).GenericEnumerator
        For Each row As EntityObject In rows
            Yield row
        Next
    End Function
End Class

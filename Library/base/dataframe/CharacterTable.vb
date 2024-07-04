Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports renv = SMRUCC.Rsharp.Runtime

''' <summary>
''' a collection of the <see cref="EntityObject"/>.
''' </summary>
Public Class CharacterTable : Implements IdataframeReader, Enumeration(Of EntityObject)

    ReadOnly rows As EntityObject()

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

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

Public Class UnionMatrix

    ReadOnly records As New List(Of NamedValue(Of List))

    Public Sub Add(recordName As String, data As List)
        records.Add(New NamedValue(Of List)(recordName, data))
    End Sub

    Public Function CreateMatrix() As Rdataframe
        Dim allFeatures As String() = records _
            .Select(Function(v) v.Value.getNames) _
            .IteratesALL _
            .ToArray _
            .DoCall(AddressOf CLRVector.asCharacter) _
            .Distinct _
            .ToArray
        Dim rownames As String() = records.Select(Function(a) a.Name).uniqueNames
        Dim matrix As New Dictionary(Of String, Array)

        For Each name As String In allFeatures
            Dim v As Object() = records _
                .Select(Function(a)
                            Return If(a.Value.hasName(name), REnv.single(a.Value.getByName(name)), 0.0)
                        End Function) _
                .ToArray

            Call matrix.Add(name, CLRVector.asNumeric(v))
        Next

        Return New Rdataframe With {
            .rownames = rownames,
            .columns = matrix
        }
    End Function

End Class
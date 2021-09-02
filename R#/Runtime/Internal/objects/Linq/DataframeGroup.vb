Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Internal.Object.Linq

    Public Module DataframeGroup

        <Extension>
        Public Function GroupBy(data As dataframe, key As String) As Dictionary(Of String, dataframe)
            Dim values As vector = REnv _
                .asVector(Of String)(data.getColumnVector(columnName:=key)) _
                .DoCall(Function(v)
                            Return vector.asVector(Of String)(v)
                        End Function)
            Dim factors As String() = DirectCast(values.data, String()).Distinct.ToArray
            Dim groups As New Dictionary(Of String, dataframe)
            Dim env As New Environment

            For Each factor As String In factors
                Dim i As Boolean() = (values = factor).aslogical
                Dim partRows As dataframe = data.sliceByRow(i, env)

                groups.Add(factor, partRows)
            Next

            Return groups
        End Function
    End Module
End Namespace
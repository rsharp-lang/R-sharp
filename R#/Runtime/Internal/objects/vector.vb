Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal

    Public Class vector : Implements RNames

        Public Property data As Array

        Dim names As String()
        Dim nameIndex As Index(Of String)

        Public Function getNames() As String() Implements RNames.getNames
            Return names
        End Function

        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            If Not names.IsNullOrEmpty Then
                If names.Length <> data.Length Then
                    Return Internal.stop($"vector names is not equals in length with the vector element data!", envir)
                Else
                    Me.names = names
                    Me.nameIndex = names.Indexing
                End If
            Else
                Me.names = names
                Me.nameIndex = Nothing
            End If

            Return data
        End Function
    End Class
End Namespace
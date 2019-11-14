Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal

    Public Class list : Implements RNames, RIndex

        Public Property slots As Dictionary(Of String, Object)

        Public Function getNames() As String() Implements RNames.getNames
            Return slots.Keys.ToArray
        End Function

        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            Dim oldNames = slots.Keys.ToArray
            Dim newSlots As Dictionary(Of String, Object)

            If names.IsNullOrEmpty Then
                ' delete the source names
                names = oldNames _
                    .Select(Function(null, i) $"[[{i + 1}]]") _
                    .ToArray
            ElseIf oldNames.Length <> names.Length Then
                Return Internal.stop("Inconsist name list length!", envir)
            End If

            newSlots = oldNames _
                .SeqIterator _
                .ToDictionary(Function(i) names(i),
                              Function(index)
                                  Return slots(oldNames(index))
                              End Function)
            slots = newSlots

            Return names
        End Function

        Public Overrides Function ToString() As String
            Return getNames.GetJson
        End Function

        Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
            Throw New NotImplementedException()
        End Function

        Public Function getByIndex(i() As Integer) As Object() Implements RIndex.getByIndex
            Throw New NotImplementedException()
        End Function

        Public Function setByIndex(i As Integer, value As Object) As Object Implements RIndex.setByIndex
            Throw New NotImplementedException()
        End Function

        Public Function setByindex(i() As Integer, value As Array) As Object Implements RIndex.setByindex
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
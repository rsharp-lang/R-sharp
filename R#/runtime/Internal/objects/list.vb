Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal

    Public Class list : Implements RNames, RIndex, RNameIndex

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

        Public Function setByIndex(i As Integer, value As Object, envir As Environment) As Object Implements RIndex.setByIndex
            Throw New NotImplementedException()
        End Function

        Public Function setByindex(i() As Integer, value As Array, envir As Environment) As Object Implements RIndex.setByindex
            Throw New NotImplementedException()
        End Function

        Public Function getByName(name As String) As Object Implements RNameIndex.getByName
            If slots.ContainsKey(name) Then
                Return slots(name)
            Else
                Return Nothing
            End If
        End Function

        Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
            Return names.Select(AddressOf getByName).ToArray
        End Function

        Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            slots(name) = value
            Return value
        End Function

        Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
            Dim getValue As Func(Of Integer, Object)

            If value.Length = 1 Then
                Dim val As Object = value.GetValue(Scan0)

                getValue = Function(i)
                               Return val
                           End Function
            Else
                If names.Length <> value.Length Then
                    Return Internal.stop({
                        $"Number of items to replace is not equals to replacement length!",
                        $"length(index): {i.Length}",
                        $"length(value): {value.Length}"
                    }, envir)
                End If

                getValue = Function(i)
                               Return value.GetValue(i)
                           End Function
            End If

            Dim result As New List(Of Object)
            Dim message As Object

            For index As Integer = 0 To names.Length - 1
                message = setByIndex(names(index), getValue(index), envir)

                If Not message Is Nothing AndAlso message.GetType Is GetType(Runtime.Components.Message) Then
                    Return message
                Else
                    result += message
                End If
            Next

            Return result.ToArray
        End Function
    End Class
End Namespace
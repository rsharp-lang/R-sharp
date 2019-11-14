Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal

    Public Class vector : Implements RNames, RIndex

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

        Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
            If i < 0 Then
                i = data.Length + i
            End If

            If i >= data.Length OrElse i < 0 Then
                Return Nothing
            Else
                Return data.GetValue(i)
            End If
        End Function

        Public Function getByIndex(i() As Integer) As Object() Implements RIndex.getByIndex
            Return i.Select(AddressOf getByIndex).ToArray
        End Function

        Public Function setByIndex(i As Integer, value As Object, envir As Environment) As Object Implements RIndex.setByIndex
            If i < 0 Then
                i = data.Length + i
            End If

            If i < 0 Then
                Return Internal.stop($"Invalid element index value '{i}'!", envir)
            End If

            Dim delta = i - data.Length

            If delta <= 0 Then
                data.SetValue(value, i)
            Else
                Dim resize As Array = New Object(i - 1) {}
                Array.ConstrainedCopy(data, Scan0, resize, Scan0, data.Length)
                data = resize
                data.SetValue(value, i - 1)
            End If

            Return value
        End Function

        Public Function setByindex(i() As Integer, value As Array, envir As Environment) As Object Implements RIndex.setByindex
            Dim getValue As Func(Of Integer, Object)

            If value.Length = 1 Then
                Dim val As Object = value.GetValue(Scan0)

                getValue = Function(j%) As Object
                               Return val
                           End Function
            Else
                If i.Length <> value.Length Then
                    Return Internal.stop({
                        $"Number of items to replace is not equals to replacement length!",
                        $"length(index): {i.Length}",
                        $"length(value): {value.Length}"
                    }, envir)
                End If

                getValue = Function(j%) As Object
                               Return value.GetValue(j)
                           End Function
            End If

            Dim result As New List(Of Object)
            Dim message As Object

            For index As Integer = 0 To i.Length - 1
                message = setByIndex(i(index), getValue(index), envir)

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
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace System.Components

    Module Encoder

        Public Function GetObject(Robj As Object) As Object
            If TypeOf Robj Is vector Then
                Dim array As New List(Of Object)

                For Each x As Object In DirectCast(Robj, vector).data
                    Call array.Add(Encoder.GetObject(x))
                Next

                Return array.ToArray
            ElseIf TypeOf Robj Is list Then
                Dim list As New Dictionary(Of String, Object)

                For Each slot In DirectCast(Robj, list).slots
                    Call list.Add(slot.Key, Encoder.GetObject(slot.Value))
                Next

                Return list
            ElseIf TypeOf Robj Is vbObject Then
                Return Encoder.GetObject(DirectCast(Robj, vbObject).target)
            Else
                Return Robj
            End If
        End Function
    End Module
End Namespace
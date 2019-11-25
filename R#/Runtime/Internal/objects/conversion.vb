Namespace Runtime.Internal

    Module conversion

        Public Function asObject(obj As Object) As Object
            If obj Is Nothing Then
                Return Nothing
            Else
                Select Case obj.GetType
                    Case GetType(vbObject), GetType(vector), GetType(list)
                        Return obj
                    Case Else
                        Return New vbObject(obj)
                End Select
            End If
        End Function
    End Module
End Namespace
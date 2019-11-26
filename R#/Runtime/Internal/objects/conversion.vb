Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Runtime.Internal

    Module RConversion

        Sub New()
            Call Internal.invoke.add(New GenericInternalInvoke("as.object", AddressOf asObject))
        End Sub

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
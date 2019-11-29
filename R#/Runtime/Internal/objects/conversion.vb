Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Runtime.Internal

    Module RConversion

        Sub New()
            Call Internal.invoke.add(New GenericInternalInvoke("as.object", AddressOf asObject))
        End Sub

        Friend Sub pushEnvir()
            ' do nothing
        End Sub

        Public Function asObject(obj As Object) As Object
            If obj Is Nothing Then
                Return Nothing
            Else
                Dim type As Type = obj.GetType

                Select Case type
                    Case GetType(vbObject), GetType(vector), GetType(list)
                        Return obj
                    Case Else
                        If type.IsArray Then
                            Return Runtime.asVector(Of Object)(obj) _
                                .AsObjectEnumerator _
                                .Select(Function(o) New vbObject(o)) _
                                .ToArray
                        Else
                            Return New vbObject(obj)
                        End If
                End Select
            End If
        End Function
    End Module
End Namespace
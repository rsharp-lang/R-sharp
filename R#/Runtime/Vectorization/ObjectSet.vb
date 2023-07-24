Namespace Runtime.Vectorization

    Public NotInheritable Class ObjectSet

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 将任意类型的序列输入转换为统一的对象枚举序列
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Function GetObjectSet(x As Object, env As Environment, Optional ByRef elementType As RType = Nothing) As IEnumerable(Of Object)
            If x Is Nothing Then
                Return {}
            End If

            Dim type As Type = x.GetType

            If type Is GetType(vector) Then
                With DirectCast(x, vector)
                    elementType = .elementType
                    Return .data.AsObjectEnumerator
                End With
            ElseIf type Is GetType(List) Then
                With DirectCast(x, List)
                    ' list value as sequence data
                    Dim raw As Object() = .slots.Values.ToArray
                    elementType = MeasureRealElementType(raw).DoCall(AddressOf RType.GetRSharpType)
                    Return raw.AsEnumerable
                End With
            ElseIf type.ImplementInterface(GetType(IDictionary(Of String, Object))) Then
                With DirectCast(x, IDictionary(Of String, Object))
                    Dim raw As Object() = .Values.AsEnumerable.ToArray
                    elementType = MeasureRealElementType(raw).DoCall(AddressOf RType.GetRSharpType)
                    Return raw.AsEnumerable
                End With
            ElseIf type.ImplementInterface(Of IDictionary) Then
                With DirectCast(x, IDictionary)
                    Dim raw As Object() = .Values.ToVector
                    elementType = MeasureRealElementType(raw).DoCall(AddressOf RType.GetRSharpType)
                    Return raw.AsEnumerable
                End With
            ElseIf type.IsArray Then
                With DirectCast(x, Array)
                    elementType = .GetType.GetElementType.DoCall(AddressOf RType.GetRSharpType)
                    Return .AsObjectEnumerator
                End With
            ElseIf type Is GetType(pipeline) Then
                With DirectCast(x, pipeline)
                    elementType = .elementType
                    Return .populates(Of Object)(env)
                End With
            Else
                elementType = RType.GetRSharpType(x.GetType)
                Return {x}
            End If
        End Function

    End Class
End Namespace
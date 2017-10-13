Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Runtime.PrimitiveTypes

    Module Core

        Friend NotInheritable Class TypeDefine(Of T)

            Private Sub New()
            End Sub

            Shared singleType, collectionType As Type

            Public Shared Function GetSingleType() As Type
                If singleType Is Nothing Then
                    singleType = GetType(T)
                End If
                Return singleType
            End Function

            Public Shared Function GetCollectionType() As Type
                If collectionType Is Nothing Then
                    collectionType = GetType(IEnumerable(Of T))
                End If
                Return collectionType
            End Function
        End Class

        ''' <summary>
        ''' The ``In`` operator
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="collection"></param>
        ''' <returns></returns>
        Public Function op_In(Of T)(x As Object, collection As IEnumerable(Of T)) As IEnumerable(Of Boolean)
            With collection.AsList
                If x Is Nothing Then
                    Return {}
                Else
                    Dim type As Type = x.GetType

                    If type Is TypeDefine(Of T).GetSingleType Then
                        Return { .IndexOf(DirectCast(x, T)) > -1}
                    ElseIf type.ImplementsInterface(TypeDefine(Of T).GetCollectionType) Then
                        Return DirectCast(x, IEnumerable(Of T)).Select(Function(n) .IndexOf(n) > -1)
                    Else
                        Throw New InvalidOperationException(type.FullName)
                    End If
                End If
            End With
        End Function
    End Module
End Namespace
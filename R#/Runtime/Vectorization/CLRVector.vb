Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Runtime.Vectorization

    Public NotInheritable Class CLRVector

        Private Sub New()
        End Sub

        Public Shared Function asLong(x As Object) As Long()
            Throw New NotImplementedException
        End Function

        Public Shared Function asCharacter(x As Object) As String()
            Throw New NotImplementedException
        End Function

        Public Shared Function asInteger(x As Object) As Integer()
            Throw New NotImplementedException
        End Function

        Public Shared Function asNumeric(x As Object) As Double()
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' NULL -> false
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Function asLogical(x As Object) As Boolean()
            If x Is Nothing Then
                Return {False}
            End If

            Dim vector As Array = REnv.asVector(Of Object)(x)
            Dim type As Type

            If vector.Length = 0 Then
                Return {}
            Else
                Dim test As Object = (From obj As Object
                                      In vector.AsQueryable
                                      Where Not obj Is Nothing).FirstOrDefault

                If Not test Is Nothing Then
                    type = test.GetType
                Else
                    ' all is nothing?
                    Return (From obj As Object
                            In vector.AsQueryable
                            Select False).ToArray
                End If
            End If

            If type Is GetType(Boolean) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(b)
                                If b Is Nothing Then
                                    Return False
                                Else
                                    Return DirectCast(b, Boolean)
                                End If
                            End Function) _
                    .ToArray
            ElseIf DataFramework.IsNumericType(type) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(num) CDbl(num) <> 0) _
                    .ToArray
            ElseIf type Is GetType(String) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(o)
                                Return DirectCast(o, String).ParseBoolean
                            End Function) _
                    .ToArray
            Else
                Return vector.AsObjectEnumerator _
                    .Select(Function(o) Not o Is Nothing) _
                    .ToArray
            End If
        End Function
    End Class
End Namespace
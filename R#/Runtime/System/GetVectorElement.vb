
Namespace Runtime.Components

    Public Class GetVectorElement

        ReadOnly [single] As Object
        ReadOnly vector As Array

        Public ReadOnly Property isNullOrEmpty As Boolean
            Get
                Return vector Is Nothing OrElse vector.Length = 0
            End Get
        End Property

        Sub New(vec As Array)
            Me.vector = vec

            If vec Is Nothing Then
                [single] = Nothing
            ElseIf vec.Length = 0 Then
                [single] = Nothing
            Else
                [single] = vec.GetValue(Scan0)
            End If
        End Sub

        Public Function Getter() As Func(Of Integer, Object)
            If isNullOrEmpty OrElse vector.Length = 1 Then
                Return Function() [single]
            Else
                ' R对向量的访问是可以下标越界的
                Return Function(i)
                           If i >= vector.Length Then
                               Return Nothing
                           Else
                               Return vector.GetValue(i)
                           End If
                       End Function
            End If
        End Function
    End Class
End Namespace
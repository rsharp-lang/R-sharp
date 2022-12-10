Namespace Runtime.Internal.Object

    ''' <summary>
    ''' NA literal value
    ''' </summary>
    Public Class invalidObject

        ''' <summary>
        ''' a single object across the entire R# runtime environment
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property value As New invalidObject

        Public ReadOnly Property literalAny As Object
            Get
                Return Nothing
            End Get
        End Property

        Private Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return "NA"
        End Function

        Public Shared Narrowing Operator CType(na As invalidObject) As Double
            Return Double.NaN
        End Operator

        Public Shared Narrowing Operator CType(na As invalidObject) As Integer
            Return Integer.MinValue
        End Operator

        Public Shared Narrowing Operator CType(na As invalidObject) As String
            Return "NA"
        End Operator

    End Class
End Namespace
Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class ROperatorAttribute : Inherits Attribute

        Public ReadOnly Property [operator] As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[operator]"></param>
        Sub New([operator] As String)
            Me.[operator] = [operator]
        End Sub

        Public Overrides Function ToString() As String
            Return [operator]
        End Function

    End Class
End Namespace
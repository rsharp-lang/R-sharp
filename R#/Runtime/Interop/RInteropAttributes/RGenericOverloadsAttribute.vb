Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=True, Inherited:=True)>
    Public Class RGenericOverloadsAttribute : Inherits RInteropAttribute

        Public ReadOnly Property FunctionName As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="func">
        ''' The name of the target function for overloads
        ''' </param>
        Sub New(func As String)
            FunctionName = func
        End Sub

        Public Overrides Function ToString() As String
            Return $"{FunctionName}(...)"
        End Function
    End Class
End Namespace
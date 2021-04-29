Imports System.Reflection

Namespace Runtime.Interop

    ''' <summary>
    ''' For make compatibale with return value and exception message or R# object wrapper
    ''' The .NET api is usually declare as returns object value, then we could use this
    ''' attribute to let user known the actual returns type of the target api function
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RApiReturnAttribute : Inherits RInteropAttribute

        Public ReadOnly Property returnTypes As Type()

        Sub New(ParamArray type As Type())
            returnTypes = type
        End Sub

        Public Overrides Function ToString() As String
            Return $"fun() -> {returnTypes.Select(Function(type) type.Name).JoinBy("|")}"
        End Function

        Public Shared Function GetActualReturnType(api As MethodInfo) As Type()
            Dim tag As RApiReturnAttribute = api.GetCustomAttribute(Of RApiReturnAttribute)

            If tag Is Nothing Then
                Return {api.ReturnType}
            Else
                Return tag.returnTypes
            End If
        End Function
    End Class
End Namespace
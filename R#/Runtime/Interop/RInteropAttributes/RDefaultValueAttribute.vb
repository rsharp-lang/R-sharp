Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Runtime.Interop

    ''' <summary>
    ''' param as type = "xxxxxx"
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter)>
    Public Class RDefaultValueAttribute : Inherits RInteropAttribute

        Public ReadOnly Property defaultValue As String

        Sub New([default] As String)
            defaultValue = [default]
        End Sub

        Public Overrides Function ToString() As String
            Return defaultValue
        End Function

        ''' <summary>
        ''' This method require a implit ctype operator that parse string to target <paramref name="parameterType"/>
        ''' </summary>
        ''' <param name="parameterType"></param>
        ''' <returns></returns>
        Public Function ParseDefaultValue(parameterType As Type) As Object
            Static implictCTypeCache As New Dictionary(Of Type, IImplictCTypeOperator(Of String, Object))

            Dim implict As IImplictCTypeOperator(Of String, Object) = implictCTypeCache _
                .ComputeIfAbsent(
                    key:=parameterType,
                    lazyValue:=Function()
                                   Return ImplictCType.GetWideningOperator(Of String)(parameterType)
                               End Function
                 )
            Dim value As Object = implict(defaultValue)

            Return value
        End Function
    End Class
End Namespace
Imports System.Globalization
Imports System.Reflection

Namespace Runtime

    Public Class RMethodInfo : Inherits MethodInfo

        Public Overrides ReadOnly Property ReturnTypeCustomAttributes As ICustomAttributeProvider
        Public Overrides ReadOnly Property MethodHandle As RuntimeMethodHandle
        Public Overrides ReadOnly Property Attributes As MethodAttributes
        Public Overrides ReadOnly Property Name As String
        Public Overrides ReadOnly Property DeclaringType As Type
        Public Overrides ReadOnly Property ReflectedType As Type

        Public Overrides Function GetBaseDefinition() As MethodInfo
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetParameters() As ParameterInfo()
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetMethodImplementationFlags() As MethodImplAttributes
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Invoke(obj As Object, invokeAttr As BindingFlags, binder As Binder, parameters() As Object, culture As CultureInfo) As Object
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetCustomAttributes(inherit As Boolean) As Object()
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetCustomAttributes(attributeType As Type, inherit As Boolean) As Object()
            Throw New NotImplementedException()
        End Function

        Public Overrides Function IsDefined(attributeType As Type, inherit As Boolean) As Boolean
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
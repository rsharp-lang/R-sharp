Imports System.Globalization
Imports System.Reflection

Namespace Runtime

    ''' <summary>
    ''' 这个模型主要是为了兼容PrimitiveTypes类型之中的自定义操作符而设置的
    ''' </summary>
    Public Class RMethodInfo : Inherits MethodInfo

        Public Overrides ReadOnly Property ReturnTypeCustomAttributes As ICustomAttributeProvider
        Public Overrides ReadOnly Property MethodHandle As RuntimeMethodHandle
        Public Overrides ReadOnly Property Attributes As MethodAttributes
        Public Overrides ReadOnly Property Name As String
        Public Overrides ReadOnly Property DeclaringType As Type
        Public Overrides ReadOnly Property ReflectedType As Type

        Public Overrides Function GetBaseDefinition() As MethodInfo
            Return Nothing
        End Function

        Public Overrides Function GetParameters() As ParameterInfo()
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetMethodImplementationFlags() As MethodImplAttributes
            Return MethodImplAttributes.InternalCall
        End Function

        ''' <summary>
        ''' 函数调用
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="invokeAttr"></param>
        ''' <param name="binder"></param>
        ''' <param name="parameters"></param>
        ''' <param name="culture"></param>
        ''' <returns></returns>
        Public Overrides Function Invoke(obj As Object, invokeAttr As BindingFlags, binder As Binder, parameters() As Object, culture As CultureInfo) As Object

        End Function

        Public Overrides Function GetCustomAttributes(inherit As Boolean) As Object()
            Return {}
        End Function

        Public Overrides Function GetCustomAttributes(attributeType As Type, inherit As Boolean) As Object()
            Return {}
        End Function

        Public Overrides Function IsDefined(attributeType As Type, inherit As Boolean) As Boolean
            Return True
        End Function
    End Class
End Namespace
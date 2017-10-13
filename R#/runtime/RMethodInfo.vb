Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.CompilerServices

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

        Dim args As ParameterInfo()
        Dim api As Func(Of Object, Object, Object)

        Sub New(args As RParameterInfo(), api As Func(Of Object, Object, Object), <CallerMemberName> Optional name$ = Nothing)
            Me.Name = name
            Me.api = api
            Me.args = args _
                .Select(Function(a) DirectCast(a, ParameterInfo)) _
                .ToArray
        End Sub

        Public Overrides Function GetBaseDefinition() As MethodInfo
            Return Nothing
        End Function

        Public Overrides Function GetParameters() As ParameterInfo()
            Return args
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
            Return api(parameters(0), parameters(1))
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

    Public Class RParameterInfo : Inherits ParameterInfo

        Public Overrides ReadOnly Property ParameterType As Type
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _type
            End Get
        End Property

        Public Overrides ReadOnly Property Name As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _name
            End Get
        End Property

        Public Overrides ReadOnly Property Position As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _pos
            End Get
        End Property

        ReadOnly _pos%, _name$
        ReadOnly _type As Type

        Sub New(name$, type As Type, pos%)
            _pos = pos
            _name = name
            _type = type
        End Sub
    End Class
End Namespace
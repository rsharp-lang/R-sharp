
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal

    ''' <summary>
    ''' 类型通用函数重载申明
    ''' fun(x, ...)
    ''' </summary>
    ''' <param name="x">脚本引擎会根据这个参数的类型进行通用函数的调用</param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 可以将这个机制看作为函数重载
    ''' </remarks>
    Public Delegate Function GenericFunction(x As Object, args As list, env As Environment) As Object

    ''' <summary>
    ''' Typped generic function invoke
    ''' </summary>
    Public Module generic

        ReadOnly generics As New Dictionary(Of String, Dictionary(Of Type, GenericFunction))

        Public Sub add(name$, x As Type, [overloads] As GenericFunction)
            If Not generics.ContainsKey(name) Then
                generics(name) = New Dictionary(Of Type, GenericFunction)
            End If

            generics(name)(x) = [overloads]
        End Sub

        Public Function exists(funcName As String) As Boolean
            Return generics.ContainsKey(funcName)
        End Function

        Friend Function invokeGeneric(funcName$, x As Object, args As list, env As Environment) As Object
            Dim type As Type = x.GetType
            Dim apiCalls As GenericFunction = generics(funcName)(type)
            Dim result As Object = apiCalls(x, args, env)

            Return result
        End Function
    End Module
End Namespace
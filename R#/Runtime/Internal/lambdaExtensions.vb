Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal

    Module lambdaExtensions

        ''' <summary>
        ''' 主要是应用于单个参数的R运行时函数的调用
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        Friend Function invokeArgument(value As Object) As InvokeParameter()
            Return InvokeParameter.Create(value)
        End Function

        Friend Function getRFunc(FUN As Object, env As Environment) As [Variant](Of RFunction, Message)
            If FUN Is Nothing Then
                Return Internal.debug.stop({"Missing apply function!"}, env)
            ElseIf TypeOf FUN Is Message Then
                Return DirectCast(FUN, Message)
            ElseIf Not FUN.GetType.ImplementInterface(GetType(RFunction)) Then
                Return Internal.debug.stop({"Target is not a function!"}, env)
            End If

            Return DirectCast(FUN, RFunction)
        End Function
    End Module
End Namespace
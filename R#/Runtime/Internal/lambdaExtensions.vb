Imports SMRUCC.Rsharp.Runtime.Components

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
    End Module
End Namespace
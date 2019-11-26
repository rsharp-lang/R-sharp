Imports System.Runtime.CompilerServices

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' 内部函数的调用接口
    ''' </summary>
    Public MustInherit Class RInternalFuncInvoke

        ''' <summary>
        ''' 函数名称（函数名称主要是在抛出错误的时候添加调试信息所使用的）
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property funcName As String

        ''' <summary>
        ''' 执行这个内部函数
        ''' </summary>
        ''' <param name="envir">代码所执行的环境对象</param>
        ''' <param name="paramVals">函数的参数</param>
        ''' <returns></returns>
        Public MustOverride Function invoke(envir As Environment, paramVals As Object()) As Object

    End Class

    Public Class GenericInternalInvoke : Inherits RInternalFuncInvoke

        Public Overrides ReadOnly Property funcName As String

        ReadOnly handle As Func(Of Environment, Object(), Object)

        Sub New(name$, invoke As Func(Of Object, Object))
            funcName = name
            handle = Function(envir, params) invoke(params(Scan0))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function invoke(envir As Environment, paramVals() As Object) As Object
            Return handle(envir, paramVals)
        End Function
    End Class
End Namespace
Imports SMRUCC.Rsharp.Runtime.Components

Public Class RInvoke

    Public Property content_type As String

    ''' <summary>
    ''' 这是一个base64字符串，主要是为了兼容文本与图像输出
    ''' </summary>
    ''' <returns></returns>
    Public Property stdOut As String
    Public Property warnings As Message()
    Public Property err As Message
    Public Property code As Integer

End Class

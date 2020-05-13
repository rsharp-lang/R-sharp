Imports Flute.Http.AppEngine
Imports SMRUCC.Rsharp.Runtime.Components

''' <summary>
''' <see cref="RInvoke.info"/>是一个base64字符串，主要是为了兼容文本与图像输出
''' </summary>
Public Class RInvoke : Inherits JsonResponse(Of String)

    Public Property content_type As String
    Public Property warnings As Message()
    Public Property err As Message
    Public Property server_time As Date = Now

End Class

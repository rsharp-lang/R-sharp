Namespace Interpreter.Parser.Tokens

    ''' <summary>
    ''' 包含有函数句柄以及调用的接口的描述信息
    ''' </summary>
    Public Class EntryPoint : Inherits Token

        Public ReadOnly Property Name As InternalExpression

        Sub New(Name As String)
            Call MyBase.New(0, Name)
            Me.Name = New InternalExpression(Name)
        End Sub

        Public Overrides Function ToString() As String
            Return MyBase.ToString()
        End Function
    End Class
End Namespace
Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the Linq expression
    ''' </summary>
    Public MustInherit Class Expression

        Public MustOverride ReadOnly Property name As String

        ''' <summary>
        ''' Evaluate the expression
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Public MustOverride Function Exec(context As ExecutableContext) As Object

    End Class
End Namespace
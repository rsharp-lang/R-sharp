Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the Linq expression
    ''' </summary>
    Public MustInherit Class Expression

        Public ReadOnly Property name As String
            Get
                Return MyClass.GetType.Name.ToLower
            End Get
        End Property

        ''' <summary>
        ''' Evaluate the expression
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Public MustOverride Function Exec(context As ExecutableContext) As Object

    End Class
End Namespace
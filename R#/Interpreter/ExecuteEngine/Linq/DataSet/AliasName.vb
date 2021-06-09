
Namespace Interpreter.ExecuteEngine.LINQ

    Public Class AliasName : Inherits Expression

        Public Property OldName As String
        Public Property NewAlias As String

        Public Overrides ReadOnly Property name As String
            Get
                Return ToString()
            End Get
        End Property

        Sub New(old As String, [alias] As String)
            Me.OldName = old
            Me.NewAlias = [alias]
        End Sub

        Public Overrides Function ToString() As String
            Return $"{OldName} As {NewAlias}"
        End Function

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim value As Object = context.FindSymbol(OldName)?.value
            context.SetSymbol(NewAlias, value)
            Return value
        End Function
    End Class
End Namespace
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class Literal : Inherits Expression

        Public Property value As Object

        Public ReadOnly Property type As Type
            Get
                If value Is Nothing Then
                    Return GetType(Void)
                Else
                    Return value.GetType
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property name As String
            Get
                Return "constant"
            End Get
        End Property

        Sub New(t As Token)
            Select Case t.name
                Case TokenType.booleanLiteral : value = t.text.ParseBoolean
                Case TokenType.integerLiteral : value = t.text.ParseInteger
                Case TokenType.numberLiteral : value = t.text.ParseDouble
                Case TokenType.stringLiteral : value = t.text
                Case Else
                    Throw New InvalidCastException
            End Select
        End Sub

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Return value
        End Function

        Public Overrides Function ToString() As String
            Return $"({type.Name.ToLower}) {any.ToString(value, "null")}"
        End Function
    End Class
End Namespace
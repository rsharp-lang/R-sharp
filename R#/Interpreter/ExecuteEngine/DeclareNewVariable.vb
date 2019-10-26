Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class DeclareNewVariable : Inherits Expression

        Dim name As String
        Dim value As Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Sub New(code As Token())
            ' 0   1    2   3    4 5
            ' let var [as type [= ...]]
            name = code(1).text

            If code(2).name = TokenType.keyword AndAlso code(2).text = "as" Then
                type = code(3).text.GetRTypeCode

                If code.Length > 4 AndAlso
                   code(4).name = TokenType.operator AndAlso
                   code(4).text = "=" Then

                    Call code.Skip(5).DoCall(AddressOf getInitializeValue)
                End If
            Else
                type = TypeCodes.generic

                If code.Length > 2 AndAlso
                   code(2).name = TokenType.operator AndAlso
                   code(2).text = "=" Then

                    Call code.Skip(3).DoCall(AddressOf getInitializeValue)
                End If
            End If
        End Sub

        Private Sub getInitializeValue(code As IEnumerable(Of Token))
            value = Expression.CreateExpression(code.ToArray)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object

            If Me.value Is Nothing Then
                value = Nothing
            Else
                value = Me.value.Evaluate(envir)
            End If

            Call envir.Push(name, value, type)

            Return value
        End Function
    End Class
End Namespace
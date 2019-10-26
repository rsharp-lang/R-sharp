Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public MustInherit Class Expression

        ''' <summary>
        ''' 推断出的这个表达式可能产生的值的类型
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property type As TypeCodes

        Public MustOverride Function Evaluate(envir As Environment) As Object

        Shared ReadOnly literalTypes As Index(Of TokenType) = {
            TokenType.stringLiteral
        }

        Public Shared Function CreateExpression(code As Token()) As Expression
            If code(Scan0).name = TokenType.keyword Then
                Dim keyword As String = code(Scan0).text

                Select Case keyword
                    Case "let" : Return New DeclareNewVariable(code)
                    Case Else
                        Throw New SyntaxErrorException
                End Select
            ElseIf code.Length = 1 AndAlso code(0).name Like literalTypes Then
                Return New Literal(code(0))
            End If

            Throw New NotImplementedException
        End Function
    End Class
End Namespace
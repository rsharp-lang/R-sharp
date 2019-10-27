Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class StringInterpolation : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        ''' <summary>
        ''' 这些表达式产生的全部都是字符串结果值
        ''' </summary>
        Dim stringParts As Expression()

        Sub New(token As Token)
            Dim tokens = TokenIcer.StringInterpolation.ParseTokens(token.text)
            Dim block = tokens.SplitByTopLevelDelimiter(TokenType.stringLiteral)
            Dim parts As New List(Of Expression)

            For Each part As Token() In block
                If part.isLiteral(TokenType.stringLiteral) Then
                    parts += New Literal(part(Scan0))
                Else
                    parts += Expression.CreateExpression(part)
                End If
            Next

            stringParts = parts
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
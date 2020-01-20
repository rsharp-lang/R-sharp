Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.SyntaxImplements

    Module StringInterpolationSyntax

        Public Function StringInterpolation(token As Token) As SyntaxResult
            Dim tokens As Token() = TokenIcer.StringInterpolation.ParseTokens(token.text)
            Dim block As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.stringLiteral)
            Dim parts As New List(Of Expression)
            Dim syntaxTemp As SyntaxResult

            For Each part As Token() In block
                If part.isLiteral(TokenType.stringLiteral) Then
                    parts += New Literal(part(Scan0))
                Else
                    syntaxTemp = part _
                        .Skip(1) _
                        .Take(part.Length - 2) _
                        .DoCall(AddressOf Expression.CreateExpression)

                    If syntaxTemp.isException Then
                        Return syntaxTemp
                    Else
                        parts += syntaxTemp.expression
                    End If
                End If
            Next

            Return New StringInterpolation(parts)
        End Function
    End Module
End Namespace
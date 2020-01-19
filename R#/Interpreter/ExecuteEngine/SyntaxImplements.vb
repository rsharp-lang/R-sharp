Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine

    Module SyntaxImplements

        Public Function FunctionInvoke(tokens As Token()) As SyntaxResult
            Dim params = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .ToArray

            Dim funcName = New Literal(tokens(Scan0).text)
            Dim span = tokens(Scan0).span
            Dim parameters As New List(Of Expression)

            For Each token As SyntaxResult In params _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .Select(Function(param)
                            Return Expression.CreateExpression(param)
                        End Function)

                If token.isException Then
                    Return token
                Else
                    parameters.Add(token.expression)
                End If
            Next

            Return New FunctionInvoke(funcName, parameters.ToArray)
        End Function
    End Module
End Namespace
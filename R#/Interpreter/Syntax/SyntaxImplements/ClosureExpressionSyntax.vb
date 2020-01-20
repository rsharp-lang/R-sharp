Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module ClosureExpressionSyntax

        Public Function ClosureExpression(tokens As Token()) As SyntaxResult
            Dim [error] As SyntaxResult = Nothing
            Dim lines = Interpreter.GetExpressions(tokens, Nothing, Sub(ex) [error] = ex).ToArray

            If Not [error] Is Nothing Then
                Return [error]
            Else
                Return New SyntaxResult(New ClosureExpression(lines))
            End If
        End Function
    End Module
End Namespace
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module ForLoopSyntax

        Public Function ForLoopSyntax(tokens As IEnumerable(Of Token)) As SyntaxResult
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.close)
            Dim [loop] As List(Of Token()) = blocks(Scan0) _
                .Skip(1) _
                .SplitByTopLevelDelimiter(TokenType.keyword)
            Dim vars As Token() = [loop](Scan0)
            Dim variables$()

            If vars.Length = 1 Then
                variables = {vars(Scan0).text}
            Else
                variables = vars _
                    .Skip(1) _
                    .Take(vars.Length - 2) _
                    .Where(Function(x) Not x.name = TokenType.comma) _
                    .Select(Function(x) x.text) _
                    .ToArray
            End If

            Dim sequence As SyntaxResult = [loop] _
                .Skip(2) _
                .IteratesALL _
                .DoCall(AddressOf Expression.CreateExpression)

            If sequence.isException Then
                Return sequence
            End If

            Dim parallel As Boolean = False
            Dim loopBody As SyntaxResult = ParseLoopBody(blocks(2), isParallel:=parallel)

            If loopBody.isException Then
                Return loopBody
            End If

            Dim body As New DeclareNewFunction With {
                .body = loopBody.expression,
                .funcName = "forloop_internal",
                .params = {}
            }

            Return New SyntaxResult(New ForLoop(variables, sequence.expression, body, parallel))
        End Function

        Private Function ParseLoopBody(tokens As Token(), ByRef isParallel As Boolean) As SyntaxResult
            If tokens(Scan0) = (TokenType.open, "{") Then
                Return tokens _
                    .Skip(1) _
                    .DoCall(AddressOf ClosureExpressionSyntax.ClosureExpression)
            ElseIf tokens(Scan0) = (TokenType.operator, "%") AndAlso
                   tokens(1) = (TokenType.identifier, "dopar") AndAlso
                   tokens(2) = (TokenType.operator, "%") Then

                ' for(...) %dopar% {...}
                isParallel = True

                Return tokens _
                    .Skip(4) _
                    .DoCall(AddressOf ClosureExpressionSyntax.ClosureExpression)
            Else
                Throw New SyntaxErrorException
            End If
        End Function
    End Module
End Namespace
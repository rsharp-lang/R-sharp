Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module WhileLoopSyntax

        Public Function CreateLoopExpression(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim keyword As Token = code(Scan0)(Scan0)
            Dim test As SyntaxResult

            code = code _
                .Skip(1) _
                .IteratesALL _
                .SplitByTopLevelDelimiter(Language.TokenType.close)
            test = Expression.CreateExpression(code(Scan0).Skip(1), opts)

            If test.isException Then
                Return test
            End If

            Dim tokens As Token() = code.Skip(1).IteratesALL.Skip(1).ToArray
            Dim loopBody As SyntaxResult = SyntaxImplements.ClosureExpression(tokens.Skip(1).Take(tokens.Length - 2), opts)
            Dim stackframe As StackFrame = opts.GetStackTrace(keyword, "whileLoop_closure")

            If loopBody.isException Then
                Return loopBody
            Else
                Return New WhileLoop(test.expression, loopBody.expression, stackframe)
            End If
        End Function
    End Module
End Namespace
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module FormulaExpressionSyntax

        Public Function RunParse(code As Token()(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim y As String = code(Scan0)(Scan0).text
            Dim expression As SyntaxResult = SyntaxResult.CreateExpression(code.Skip(2).IteratesALL, opts)

            If expression.isException Then
                Return expression
            End If

            Return New FormulaExpression(y, expression.expression)
        End Function

        Public Function GetExpressionLiteral(code As Token()(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim stackFrame As StackFrame = opts.GetStackTrace(code(Scan0)(Scan0), "__expression__literal")
            Dim literal As SyntaxResult = SyntaxResult.CreateExpression(code.Skip(1).IteratesALL, opts)

            If literal.isException Then
                Return literal
            Else
                Return New ExpressionLiteral(literal.expression, stackFrame)
            End If
        End Function
    End Module
End Namespace
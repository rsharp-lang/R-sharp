Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
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
    End Module
End Namespace
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module FunctionAcceptorSyntax

        Public Function FunctionAcceptorInvoke(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")
            Dim funcInvoke As SyntaxResult = code(Scan0).JoinIterates(code(1)).DoCall(Function(fi) Expression.CreateExpression(fi, opts))

            If funcInvoke.isException Then
                Return funcInvoke
            End If

            Dim Rscript As Rscript = opts.source
            Dim invoke As FunctionInvoke = funcInvoke.expression
            Dim firstParameterBody As Expression() = code(2) _
                .Skip(1) _
                .Take(code(2).Length - 2) _
                .ToArray _
                .GetExpressions(Rscript, Nothing, opts) _
                .ToArray

            If opts.haveSyntaxErr Then
                Return New SyntaxResult(New SyntaxErrorException(opts.error), opts.debug)
            End If

            Dim parameterClosure As New ClosureExpression(firstParameterBody)

            invoke.parameters = {parameterClosure}.JoinIterates(invoke.parameters).ToArray

            Return New SyntaxResult(invoke)
        End Function
    End Module
End Namespace
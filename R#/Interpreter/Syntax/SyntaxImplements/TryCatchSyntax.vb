Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module TryCatchSyntax

        <Extension>
        Public Function CreateTryError(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")
            Dim tryVal = ExpressionBuilder.ParseExpression(New List(Of Token()) From {code(Scan0).Skip(2).Take(code(Scan0).Length - 2).ToArray}, opts)
            Dim stackframe As StackFrame = opts.GetStackTrace(tokens(Scan0), "try")

            If tryVal.isException Then
                Return tryVal
            End If

            Dim catchVal As SyntaxResult

            If code = 2 Then
                ' no catch
                catchVal = Nothing
            Else
                ' has catch
                catchVal = ClosureExpressionSyntax.ClosureExpression(code(2), opts)

                If catchVal.isException Then
                    Return catchVal
                End If
            End If

            Return New TryCatchExpression(tryVal.expression, catchVal?.expression, stackframe)
        End Function

    End Module
End Namespace
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class IfBranch : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim ifTest As Expression
        Dim trueClosure As DeclareNewFunction

        Sub New(tokens As IEnumerable(Of Token))
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.close)

            ifTest = Expression.CreateExpression(blocks(Scan0).Skip(1))
            trueClosure = New DeclareNewFunction With {
                .funcName = "if_closure_internal",
                .params = {},
                .body = New Program With {
                    .execQueue = blocks(2).Skip(1).ToArray _
                        .GetExpressions _
                        .ToArray
                }
            }
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim test As Boolean = Runtime.getFirst(ifTest.Evaluate(envir))

            If test Then
                Return trueClosure.Evaluate(envir)
            Else
                Return False
            End If
        End Function
    End Class
End Namespace
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module IfBranchSyntax

        Public Function IIfExpression(test As Token(), ifelse As List(Of Token())) As SyntaxResult
            Dim ifTest = Expression.CreateExpression(test)
            Dim trueResult = Expression.CreateExpression(ifelse(Scan0))
            Dim falseResult = Expression.CreateExpression(ifelse(2))

            If ifTest.isException Then
                Return ifTest
            ElseIf trueResult.isException Then
                Return trueResult
            ElseIf falseResult.isException Then
                Return falseResult
            Else
                Return New IIfExpression(
                    iftest:=ifTest.expression,
                    trueResult:=trueResult.expression,
                    falseResult:=falseResult.expression
                )
            End If
        End Function

        Public Function IfBranch(tokens As IEnumerable(Of Token)) As SyntaxResult
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.close)
            Dim ifTest = Expression.CreateExpression(blocks(Scan0).Skip(1))

            If ifTest.isException Then
                Return ifTest
            End If

            Dim closureInternal As SyntaxResult = blocks(2) _
                .Skip(1) _
                .DoCall(AddressOf SyntaxImplements.ClosureExpression)

            If closureInternal.isException Then
                Return closureInternal
            End If

            Return New SyntaxResult(New IfBranch(ifTest.expression, closureInternal.expression))
        End Function

        Public Function ElseIfBranch(tokens As IEnumerable(Of Token)) As SyntaxResult

        End Function

        Public Function ElseBranch(code As Token()) As SyntaxResult
            Dim syntaxResult As SyntaxResult = code _
                .Skip(1) _
                .Take(code.Length - 2) _
                .DoCall(AddressOf SyntaxImplements.ClosureExpression)

            If syntaxResult.isException Then
                Return syntaxResult
            Else
                Return New ElseBranch(syntaxResult.expression)
            End If
        End Function
    End Module
End Namespace
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module IfBranchSyntax

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
    End Module
End Namespace
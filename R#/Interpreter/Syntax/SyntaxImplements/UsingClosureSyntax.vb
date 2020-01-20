Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module UsingClosureSyntax

        Public Function UsingClosure(code As IEnumerable(Of Token())) As SyntaxResult
            Dim input As Token() = code.IteratesALL.ToArray
            Dim tokens As Token()() = input _
                .SplitByTopLevelDelimiter(TokenType.close) _
                .ToArray
            Dim parmPart As Token() = tokens(Scan0)

            If tokens(1).Length = 1 AndAlso tokens(1)(Scan0) = (TokenType.close, ")") Then
                parmPart = parmPart + tokens(1).AsList
            End If

            Dim closureSyntax = SyntaxImplements.ClosureExpression(tokens(2).Skip(1))

            If closureSyntax.isException Then
                Return closureSyntax
            Else
                tokens = parmPart.SplitByTopLevelDelimiter(TokenType.keyword, tokenText:="as")
            End If

            Dim paramsSyntax = SyntaxImplements.DeclareNewVariable(tokens(Scan0), tokens(2))

            If paramsSyntax.isException Then
                Return paramsSyntax
            Else
                Return New UsingClosure(paramsSyntax.expression, closureSyntax.expression)
            End If
        End Function
    End Module
End Namespace
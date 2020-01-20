Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module RequireSyntax

        Public Function Require(names As Token()) As SyntaxResult
            Dim packages As New List(Of Expression)

            For Each name As SyntaxResult In names _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(b) Not b.isComma) _
                .Select(AddressOf Expression.CreateExpression)

                If name.isException Then
                    Return name
                Else
                    packages += name.expression
                End If
            Next

            Return New SyntaxResult(New Require(packages))
        End Function
    End Module
End Namespace
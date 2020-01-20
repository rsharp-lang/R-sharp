Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.SyntaxImplements

    Module DeclareNewFunctionSyntax

        Public Function DeclareNewFunction(code As List(Of Token())) As DeclareNewFunction
            Dim [declare] As Token() = code(4)
            Dim parts = [declare].SplitByTopLevelDelimiter(TokenType.close)
            Dim paramPart = parts(Scan0).Skip(1).ToArray
            Dim bodyPart = parts(2).Skip(1).ToArray.DoCall(AddressOf SyntaxImplements.ClosureExpression)
            Dim funcName = code(1)(Scan0).text

            Call getParameters(paramPart)


        End Function

        Private Sub getParameters(tokens As Token())
            Dim parts As Token()() = tokens.SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .ToArray

            params = parts _
                .Select(Function(t)
                            Dim [let] As New List(Of Token) From {
                                New Token With {.name = TokenType.keyword, .text = "let"}
                            }
                            Return New DeclareNewVariable([let] + t)
                        End Function) _
                .ToArray
        End Sub
    End Module
End Namespace
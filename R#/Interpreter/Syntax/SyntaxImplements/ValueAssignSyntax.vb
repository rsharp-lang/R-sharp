Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module ValueAssignSyntax

        Public Function ValueAssign(tokens As List(Of Token())) As SyntaxResult
            Dim targetSymbols = DeclareNewVariableSyntax _
                .getNames(tokens(Scan0)) _
                .Select(Function(name) New Literal(name)) _
                .ToArray
            Dim isByRef = tokens(Scan0)(Scan0).text = "="
            Dim value As SyntaxResult = tokens.Skip(2) _
                .AsList _
                .DoCall(AddressOf Expression.ParseExpression)

            If value.isException Then
                Return value
            Else
                Return New ValueAssign(targetSymbols, value.expression) With {
                    .isByRef = isByRef
                }
            End If
        End Function
    End Module
End Namespace
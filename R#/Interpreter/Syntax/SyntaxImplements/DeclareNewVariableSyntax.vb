Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module DeclareNewVariableSyntax

        Public Function DeclareNewVariable(code As List(Of Token())) As SyntaxResult
            Dim var As New DeclareNewVariable
            Dim valSyntaxtemp As SyntaxResult = Nothing

            ' 0   1    2   3    4 5
            ' let var [as type [= ...]]
            var.names = getNames(code(1))
            var.hasInitializeExpression = True

            If code = 2 Then
                var.m_type = TypeCodes.generic
            ElseIf code(2).isKeyword("as") Then
                var.m_type = code(3)(Scan0).text.GetRTypeCode

                If code.Count > 4 AndAlso code(4).isOperator("=", "<-") Then
                    valSyntaxtemp = code.Skip(5).AsList.DoCall(AddressOf Expression.ParseExpression)
                End If
            Else
                var.m_type = TypeCodes.generic

                If code > 2 AndAlso code(2).isOperator("=", "<-") Then
                    valSyntaxtemp = code.Skip(3).AsList.DoCall(AddressOf Expression.ParseExpression)
                End If
            End If

            If valSyntaxtemp Is Nothing Then
                var.hasInitializeExpression = False
            Else
                var.value = valSyntaxtemp.expression
            End If

            Return New SyntaxResult(var)
        End Function

        Public Function DeclareNewVariable(code As List(Of Token)) As SyntaxResult
            Return SyntaxImplements.DeclareNewVariable(code:=code.SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True))
        End Function

        Public Function DeclareNewVariable(singleToken As Token()) As SyntaxResult
            Return New DeclareNewVariable With {
                .names = getNames(singleToken),
                .m_type = TypeCodes.generic,
                .hasInitializeExpression = False,
                .Value = Nothing
            }
        End Function

        Public Function DeclareNewVariable(symbol As Token(), value As Token()) As SyntaxResult
            Dim valSyntaxTemp As SyntaxResult = Expression.CreateExpression(value)

            If valSyntaxTemp.isException Then
                Return valSyntaxTemp
            End If

            Return New DeclareNewVariable With {
                .hasInitializeExpression = True,
                .names = getNames(symbol),
                .value = valSyntaxTemp.expression,
                .m_type = .value.type
            }
        End Function

        Friend Function getNames(code As Token()) As String()
            If code.Length > 1 Then
                Return code.Skip(1) _
                    .Take(code.Length - 2) _
                    .Where(Function(token) Not token.name = TokenType.comma) _
                    .Select(Function(symbol)
                                If symbol.name <> TokenType.identifier Then
                                    Throw New SyntaxErrorException
                                Else
                                    Return symbol.text
                                End If
                            End Function) _
                    .ToArray
            Else
                Return {code(Scan0).text}
            End If
        End Function
    End Module
End Namespace
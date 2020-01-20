Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.SyntaxImplements

    Module DeclareNewVariableSyntax

        Public Function DeclareNewVariable(code As List(Of Token())) As SyntaxResult
            ' 0   1    2   3    4 5
            ' let var [as type [= ...]]
            names = getNames(code(1))

            If code = 2 Then
                Type = TypeCodes.generic
            ElseIf code(2).isKeyword("as") Then
                Type = code(3)(Scan0).text.GetRTypeCode

                If code.Count > 4 AndAlso code(4).isOperator("=", "<-") Then
                    Call code.Skip(5).DoCall(AddressOf getInitializeValue)
                End If
            Else
                Type = TypeCodes.generic

                If code > 2 AndAlso code(2).isOperator("=", "<-") Then
                    Call code.Skip(3).DoCall(AddressOf getInitializeValue)
                End If
            End If
        End Function

        Public Function DeclareNewVariable(code As List(Of Token)) As SyntaxResult
            Return SyntaxImplements.DeclareNewVariable(code:=code.SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True))
        End Function

        Public Function DeclareNewVariable(singleToken As Token()) As SyntaxResult
            names = getNames(singleToken)
            Type = TypeCodes.generic
            hasInitializeExpression = False
            Value = Nothing
        End Function

        Public Function DeclareNewVariable(symbol As Token(), value As Token()) As SyntaxResult
            Me.names = getNames(symbol)
            Me.type = TypeCodes.generic
            Me.hasInitializeExpression = True
            Me.value = Expression.CreateExpression(value)
        End Function

        Friend Shared Function getNames(code As Token()) As String()
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub getInitializeValue(code As IEnumerable(Of Token()))
            hasInitializeExpression = True
            Value = Expression.ParseExpression(code.AsList)
        End Sub
    End Module
End Namespace
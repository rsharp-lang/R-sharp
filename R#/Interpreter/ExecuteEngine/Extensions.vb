Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine

    <HideModuleName>
    <Extension>
    Module Extensions

        <Extension>
        Public Function isComma(tokens As Token()) As Boolean
            If tokens.Length = 1 AndAlso tokens(Scan0).name = TokenType.comma Then
                Return True
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function isLiteral(tokens As Token(), Optional type As TokenType = TokenType.invalid) As Boolean
            If tokens.Length = 1 AndAlso tokens(Scan0).name Like Expression.literalTypes Then
                If type <> TokenType.invalid Then
                    Return tokens(Scan0).name = type
                Else
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function isIdentifier(tokens As Token()) As Boolean
            If tokens.Length = 1 AndAlso tokens(Scan0).name = TokenType.identifier Then
                Return True
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function isKeyword(tokens As Token(), Optional keyword$ = Nothing) As Boolean
            If tokens.Length = 1 AndAlso tokens(Scan0).name = TokenType.keyword Then
                If Not keyword.StringEmpty Then
                    Return tokens(Scan0).text = keyword
                Else
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function isOperator(tokens As Token(), ParamArray operators As String()) As Boolean
            If tokens.Length = 1 AndAlso tokens(Scan0).name = TokenType.operator Then
                If operators.Length > 0 Then
                    Dim op$ = tokens(Scan0).text

                    For Each test As String In operators
                        If op = test Then
                            Return True
                        End If
                    Next

                    Return False
                Else
                    Return True
                End If
            Else
                Return False
            End If
        End Function
    End Module
End Namespace
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ExpressionSignature
    ''' </summary>
    <HideModuleName>
    <Extension>
    Module ExpressionSignature

        ReadOnly valueAssigns As Index(Of String) = {"<-", "="}

        <Extension>
        Public Function isNamespaceReferenceCall(tokens As List(Of [Variant](Of Expression, String))) As Boolean
            If Not tokens(1) Like GetType(String) OrElse Not tokens(1).TryCast(Of String) = "::" Then
                Return False
            ElseIf Not tokens(2) Like GetType(FunctionInvoke) Then
                Return False
            ElseIf Not tokens(Scan0) Like GetType(SymbolReference) Then
                Return False
            Else
                Return True
            End If
        End Function

        <Extension>
        Public Function isByrefCall(tokens As List(Of [Variant](Of Expression, String))) As Boolean
            If Not tokens(Scan0) Like GetType(FunctionInvoke) Then
                Return False
            ElseIf Not tokens(1) Like GetType(String) OrElse Not tokens(1).TryCast(Of String) Like valueAssigns Then
                Return False
            ElseIf Not tokens >= 3 Then
                Return False
            Else
                Return True
            End If
        End Function

        ''' <summary>
        ''' XXX(YYY)
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isFunctionInvoke(tokens As Token()) As Boolean
            If Not tokens(Scan0).name = TokenType.identifier Then
                Return False
            ElseIf Not tokens(1).name = TokenType.open OrElse Not tokens.Last.name = TokenType.close Then
                Return False
            Else
                Return True
            End If
        End Function

        ''' <summary>
        ''' [x,y,z]
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isTuple(tokens As Token()) As Boolean
            If tokens(Scan0).name <> TokenType.open OrElse tokens.Last.name <> TokenType.close Then
                Return False
            End If

            For i As Integer = 1 To tokens.Length - 2
                If i Mod 2 = 0 Then
                    If Not tokens(i).name = TokenType.comma Then
                        Return False
                    End If
                Else
                    If Not tokens(i).name = TokenType.identifier Then
                        Return False
                    End If
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' ,
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isComma(tokens As Token()) As Boolean
            If tokens.Length = 1 AndAlso tokens(Scan0).name = TokenType.comma Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' XXX
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
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

        ''' <summary>
        ''' XXX
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isIdentifier(tokens As Token()) As Boolean
            If tokens.Length = 1 AndAlso tokens(Scan0).name = TokenType.identifier Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' XXX
        ''' </summary>
        ''' <param name="token"></param>
        ''' <param name="keyword"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isKeyword(token As Token, Optional keyword$ = Nothing) As Boolean
            If token.name = TokenType.keyword Then
                If Not keyword.StringEmpty Then
                    Return token.text = keyword
                Else
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' XXX
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="keyword$"></param>
        ''' <returns></returns>
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

        ''' <summary>
        ''' XXX
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="keywords"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isOneOfKeywords(tokens As Token(), ParamArray keywords$()) As Boolean
            If keywords.Length = 0 OrElse tokens.IsNullOrEmpty Then
                Return False
            End If

            If tokens.Length = 1 AndAlso tokens(Scan0).name = TokenType.keyword Then
                Return keywords.IndexOf(tokens(Scan0).text) > -1
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' *
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="operators"></param>
        ''' <returns></returns>
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
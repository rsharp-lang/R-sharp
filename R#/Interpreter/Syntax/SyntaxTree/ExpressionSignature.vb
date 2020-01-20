#Region "Microsoft.VisualBasic::50e395897691cad30daca6f53dcb981b, R#\Interpreter\Syntax\SyntaxTree\ExpressionSignature.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module ExpressionSignature
    ' 
    '         Function: ifElseTriple, isByRefCall, isComma, isFunctionInvoke, isIdentifier
    '                   (+2 Overloads) isKeyword, isLambdaFunction, isLiteral, isNamespaceReferenceCall, isOneOfKeywords
    '                   isOperator, isSequenceSyntax, isSimpleSymbolIndexer, isStackOf, isTuple
    '                   parseComplexSymbolIndexer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser

    ''' <summary>
    ''' The signature determination of the given expression tokens 
    ''' </summary>
    <HideModuleName>
    <Extension>
    Module ExpressionSignature

        Friend ReadOnly valueAssignOperatorSymbols As Index(Of String) = {"<-", "="}

        <DebuggerStepThrough>
        <Extension>
        Public Function isSequenceSyntax(tokens As List(Of Token())) As Boolean
            Return tokens(Scan0).SplitByTopLevelDelimiter(TokenType.sequence) > 1
        End Function

        ''' <summary>
        ''' The given code tokens is a lambda function?
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        <Extension>
        Public Function isLambdaFunction(code As List(Of Token())) As Boolean
            Return code > 2 AndAlso (code(Scan0).isIdentifier OrElse code(Scan0).isTuple) AndAlso code(1).isOperator("->", "=>")
        End Function

        <Extension>
        Public Function ifElseTriple(tokens As Token()) As (test As Token(), ifelse As List(Of Token()))
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.iif)

            If blocks = 1 OrElse blocks = 2 Then
                Return Nothing
            ElseIf blocks > 3 Then
                Return Nothing
            End If

            Dim test = blocks(Scan0)
            Dim ifelse = blocks(2).SplitByTopLevelDelimiter(TokenType.sequence)

            If Not ifelse = 3 Then
                Return Nothing
            Else
                Return (test, ifelse)
            End If
        End Function

        <Extension>
        Public Function isNamespaceReferenceCall(tokens As [Variant](Of Expression, String)()) As Boolean
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
        Public Function isByRefCall(tokens As [Variant](Of Expression, String)()) As Boolean
            If Not tokens(Scan0) Like GetType(FunctionInvoke) Then
                Return False
            ElseIf Not tokens(1) Like GetType(String) OrElse Not tokens(1).TryCast(Of String) Like valueAssignOperatorSymbols Then
                Return False
            ElseIf Not tokens.Length >= 3 Then
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
            ElseIf Not tokens(1) = (TokenType.open, "(") OrElse Not tokens.Last = (TokenType.close, ")") Then
                Return False
            Else
                Return True
            End If
        End Function

        <Extension>
        Public Function isSimpleSymbolIndexer(tokens As Token()) As Boolean
            If Not tokens(Scan0).name = TokenType.identifier Then
                Return False
            ElseIf Not tokens(1) = (TokenType.open, "[") OrElse Not tokens.Last = (TokenType.close, "]") Then
                Return False
            Else
                Return True
            End If
        End Function

        <Extension>
        Public Function parseComplexSymbolIndexer(tokens As Token()) As SyntaxResult
            ' func(...)[x]
            Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")
            Dim indexer = code.Last

            If indexer.isStackOf("[", "]") Then
                Return code.Take(code.Count - 1) _
                    .IteratesALL _
                    .ToArray _
                    .DoCall(Function(a)
                                Return SyntaxImplements.SymbolIndexer(a, indexer)
                            End Function)
            Else
                Return Nothing
            End If
        End Function

        <Extension>
        Public Function isStackOf(tokens As Token(), open$, close$) As Boolean
            Return (tokens(Scan0) = (TokenType.open, open)) AndAlso (tokens.Last = (TokenType.close, close))
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

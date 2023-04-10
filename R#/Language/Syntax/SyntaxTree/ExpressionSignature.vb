#Region "Microsoft.VisualBasic::fd8884e72f621388e5108867a7ba1878, D:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxTree/ExpressionSignature.vb"

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


    ' Code Statistics:

    '   Total Lines: 407
    '    Code Lines: 288
    ' Comment Lines: 71
    '   Blank Lines: 48
    '     File Size: 14.69 KB


    '     Module ExpressionSignature
    ' 
    '         Function: ifElseTriple, isAcceptor, isAnonymous, isByRefCall, isComma
    '                   isFunctionInvoke, isIdentifier, (+2 Overloads) isKeyword, (+2 Overloads) isLambdaFunction, isLiteral
    '                   isNamespaceReferenceCall, isOneOfKeywords, isOperator, isSequenceSyntax, isSimpleSymbolIndexer
    '                   isStackOf, isTuple, isValueAssign, isVectorLoop, parseComplexSymbolIndexer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser

    ''' <summary>
    ''' The signature determination of the given expression tokens 
    ''' </summary>
    <HideModuleName>
    <Extension>
    Module ExpressionSignature

        Friend ReadOnly iterateAssign As Index(Of String) = {"+=", "-=", "*=", "/="}
        Friend ReadOnly valueAssignOperatorSymbols As Index(Of String) = {"<-", "="}
        Friend ReadOnly lambdaOperator As Index(Of String) = {"->", "=>"}
        Friend ReadOnly literalTypes As Index(Of TokenType) = {
            TokenType.stringLiteral,
            TokenType.booleanLiteral,
            TokenType.integerLiteral,
            TokenType.numberLiteral,
            TokenType.missingLiteral
        }

        <Extension>
        Public Function isValueAssign(tokens As List(Of Token())) As Boolean
            Return tokens(1).Length = 1 AndAlso tokens(1)(Scan0).name = TokenType.operator AndAlso tokens(1)(Scan0).text Like valueAssignOperatorSymbols
        End Function

        <DebuggerStepThrough>
        <Extension>
        Public Function isSequenceSyntax(tokens As List(Of Token())) As Boolean
            Return tokens(Scan0).SplitByTopLevelDelimiter(TokenType.sequence) > 1
        End Function

        ''' <summary>
        ''' The given code tokens is a lambda function?
        ''' 
        ''' x -> ...
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        <Extension>
        Public Function isLambdaFunction(code As List(Of Token())) As Boolean
            Return code > 2 AndAlso (code(Scan0).isIdentifier OrElse code(Scan0).isTuple) AndAlso code(1).isOperator("->", "=>")
        End Function

        ''' <summary>
        ''' The given code tokens is a lambda function?
        ''' 
        ''' x -> ...
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        ''' 
        <DebuggerStepThrough>
        <Extension>
        Public Function isLambdaFunction(code As [Variant](Of Expression, String)()) As Boolean
            Return code.Length > 2 AndAlso (TypeOf code(Scan0).VA Is SymbolReference OrElse TypeOf code(Scan0).VA Is VectorLiteral) AndAlso code(1).VB Like lambdaOperator
        End Function

        <Extension>
        Public Function ifElseTriple(tokens As Token()) As (test As Token(), ifelse As List(Of Token()))
            If tokens.Length <= 2 Then
                Return Nothing
            End If

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
            Static callableSymbols As Index(Of TokenType) = {
                TokenType.identifier,
                TokenType.regexp,
                TokenType.keyword
            }

            If tokens.Length < 3 Then
                ' a() -> 3 (min size)
                Return False
            End If

            If Not tokens(Scan0).name Like callableSymbols Then
                Return False
            ElseIf Not tokens(1) = (TokenType.open, "(") OrElse Not tokens.Last = (TokenType.close, ")") Then
                Return False
            Else
                Return True
            End If
        End Function

        <Extension>
        Public Function isSimpleSymbolIndexer(tokens As Token()) As Boolean
            Dim xToken As Token = tokens(Scan0)

            If Not (xToken.name = TokenType.identifier OrElse xToken.name = TokenType.keyword) Then
                Return False
            ElseIf Not tokens(1) = (TokenType.open, "[") OrElse Not tokens.Last = (TokenType.close, "]") Then
                Return False
            Else
                Return True
            End If
        End Function

        <Extension>
        Public Function isAnonymous(code As List(Of Token())) As Boolean
            If code(Scan0)(Scan0) <> (TokenType.keyword, "function") Then
                Return False
            End If

            If code = 3 Then
                Dim pOpen As Token = code(2).First
                Dim pClose As Token = code(2).Last

                Return pOpen = (TokenType.open, "{") AndAlso pClose = (TokenType.close, "}")
            ElseIf code = 2 Then
                Dim pOpen As Token = code(1).First
                Dim pClose As Token = code(1).Last

                Return pOpen = (TokenType.open, "(") AndAlso pClose = (TokenType.close, "}")
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function isAcceptor(tokens As Token()) As Boolean
            If tokens(Scan0) = (TokenType.keyword, "function") Then
                Return False
            Else
                Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")

                If code <> 3 Then
                    Return False
                End If

                Dim pOpen As Token = code(2).First
                Dim pClose As Token = code(2).Last

                If pOpen = (TokenType.open, "{") AndAlso pClose = (TokenType.close, "}") Then
                    Return True
                Else
                    Return False
                End If
            End If
        End Function

        <Extension>
        Public Function parseComplexSymbolIndexer(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            ' func(...)[x]
            Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")
            Dim indexer = code.Last

            If indexer.isStackOf("[", "]") Then
                Return code.Take(code.Count - 1) _
                    .IteratesALL _
                    .ToArray _
                    .DoCall(Function(a)
                                Return SyntaxImplements.SymbolIndexer(a, indexer, opts)
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
        ''' length is 1 and also the token is comma
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
            If tokens.Length = 1 AndAlso tokens(Scan0).name Like ExpressionSignature.literalTypes Then
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
        Public Function isVectorLoop(tokens As Token()) As Boolean
            If tokens.Length = 4 Then
                If tokens(0).name = TokenType.identifier OrElse tokens(0).name = TokenType.keyword Then
                    If tokens(1) = (TokenType.open, "{") AndAlso tokens(3) = (TokenType.close, "}") Then
                        Return tokens(2).name = TokenType.annotation
                    End If
                End If
            End If

            Return False
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
            If tokens Is Nothing Then
                Return False
            End If

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
            If tokens.Length > 1 Then
                Return False
            End If

            Dim firstToken As Token = tokens(Scan0)
            Dim op$

            If firstToken.name = TokenType.operator OrElse firstToken = (TokenType.keyword, {"like", "in", "is", "between"}) Then
                If operators.Length > 0 Then
                    op$ = firstToken.text

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

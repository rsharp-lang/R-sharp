#Region "Microsoft.VisualBasic::bf71e38c21adcfaa52166e3e6bca66bf, R#\Interpreter\ExecuteEngine\Expression.vb"

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

'     Class Expression
' 
'         Properties: expressionName
' 
'         Function: CreateExpression, ParseExpression
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public MustInherit Class Expression

        ''' <summary>
        ''' 推断出的这个表达式可能产生的值的类型
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property type As TypeCodes

        Public ReadOnly Property expressionName As String
            Get
                Return MyClass.GetType.Name
            End Get
        End Property

        Public MustOverride Function Evaluate(envir As Environment) As Object

        Friend Shared ReadOnly literalTypes As Index(Of TokenType) = {
            TokenType.stringLiteral,
            TokenType.booleanLiteral,
            TokenType.integerLiteral,
            TokenType.numberLiteral,
            TokenType.missingLiteral
        }

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateExpression(code As IEnumerable(Of Token)) As Expression
            Return code _
                .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True) _
                .DoCall(AddressOf ParseExpression)
        End Function

        Friend Shared Function keywordExpressionHandler(code As List(Of Token())) As Expression
            Dim keyword As String = code(Scan0)(Scan0).text

            Select Case keyword
                Case "let"
                    If code > 4 AndAlso code(3).isKeyword("function") Then
                        Return New DeclareNewFunction(code)
                    Else
                        Return New DeclareNewVariable(code)
                    End If
                Case "if" : Return New IfBranch(code.Skip(1).IteratesALL)
                Case "else" : Return New ElseBranch(code.Skip(1).IteratesALL.ToArray)
                Case "elseif" : Return New ElseIfBranch(code.Skip(1).IteratesALL)
                Case "return" : Return New ReturnValue(code.Skip(1).IteratesALL)
                Case "for" : Return New ForLoop(code.Skip(1).IteratesALL)
                Case "from" : Return New LinqExpression(code)
                Case "imports" : Return New [Imports](code)
                Case "function"
                    Dim [let] = New Token() {New Token(TokenType.keyword, "let")}
                    Dim name = New Token() {New Token(TokenType.identifier, "<$anonymous>")}
                    Dim [as] = New Token() {New Token(TokenType.keyword, "as")}

                    code = ({[let], name, [as]}) + code

                    Return New DeclareNewFunction(code)
                Case "suppress"
                    Dim evaluate As Expression = code _
                        .Skip(1) _
                        .IteratesALL _
                        .DoCall(AddressOf Expression.CreateExpression)

                    Return New Suppress(evaluate)
                Case "modeof", "typeof"
                    Return New ModeOf(keyword, code(1))
                Case "require"
                    Return code(1) _
                        .Skip(1) _
                        .Take(code(1).Length - 2) _
                        .ToArray _
                        .DoCall(Function(tokens)
                                    Return New Require(tokens)
                                End Function)
                Case Else
                    Throw New SyntaxErrorException
            End Select
        End Function

        Friend Shared Function ParseExpression(code As List(Of Token())) As Expression
            If code(Scan0).isKeyword Then
                Return code.DoCall(AddressOf keywordExpressionHandler)
            ElseIf code = 1 Then
                Dim item As Token() = code(Scan0)

                If item.isLiteral Then
                    Return New Literal(item(Scan0))
                ElseIf item.isIdentifier Then
                    Return New SymbolReference(item(Scan0))
                Else
                    Dim ifelse = item.ifElseTriple

                    If ifelse.ifelse Is Nothing Then
                        Return item.CreateTree
                    Else
                        Return New IIfExpression(ifelse.test, ifelse.ifelse)
                    End If
                End If
            ElseIf code.isLambdaFunction Then
                ' is a lambda function
                Return New DeclareLambdaFunction(code)
            End If

            If code(Scan0).isIdentifier Then
                If code(1).isOperator Then
                    Dim opText$ = code(1)(Scan0).text

                    If opText = "=" OrElse opText = "<-" Then
                        Return New ValueAssign(code)
                    End If
                End If
            ElseIf code(1).isOperator("=", "<-") Then
                ' tuple value assign
                Dim tuple = code(Scan0).Skip(1) _
                    .Take(code(Scan0).Length - 2) _
                    .SplitByTopLevelDelimiter(TokenType.comma) _
                    .Where(Function(t) Not t.isComma) _
                    .Select(AddressOf Expression.CreateExpression) _
                    .ToArray
                Dim value = code(2)

                Return New ValueAssign(tuple, Expression.CreateExpression(value))
            ElseIf code = 2 Then
                If code(Scan0).Length = 1 AndAlso code(Scan0)(Scan0) = (TokenType.operator, "$") Then
                    Return New FunctionInvoke(code.IteratesALL.ToArray)
                End If
            ElseIf code = 3 Then
                If code.isSequenceSyntax Then
                    Dim seq = code(Scan0).SplitByTopLevelDelimiter(TokenType.sequence)
                    Dim from = seq(Scan0)
                    Dim [to] = seq(2)
                    Dim steps As Token() = Nothing

                    If code > 1 Then
                        If code(1).isKeyword("step") Then
                            steps = code(2)
                        Else
                            Throw New SyntaxErrorException
                        End If
                    End If

                    Return New SequenceLiteral(from, [to], steps)
                End If
            End If

            Return code.ParseBinaryExpression
        End Function
    End Class
End Namespace

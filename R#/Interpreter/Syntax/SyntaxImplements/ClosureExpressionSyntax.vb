#Region "Microsoft.VisualBasic::ef66da0989b62575ba3f7ee422648253, R#\Interpreter\Syntax\SyntaxImplements\ClosureExpressionSyntax.vb"

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

    '     Module ClosureExpressionSyntax
    ' 
    '         Function: ClosureExpression, isJsonMember
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module ClosureExpressionSyntax

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="tokens">
        ''' 应该是去掉了最外层的``{}``的
        ''' </param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        Public Function ClosureExpression(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim [error] As SyntaxResult = Nothing
            Dim allTokens As Token() = tokens.ToArray

            If allTokens.IsNullOrEmpty Then
                ' an empty closure
                ' {}

                Return New SyntaxResult(New ClosureExpression({}))

            ElseIf allTokens.First = (TokenType.open, "{") AndAlso
                   allTokens.Last = (TokenType.close, "}") AndAlso
                   allTokens.SplitByTopLevelDelimiter(TokenType.terminator) = 1 Then

                allTokens = allTokens _
                    .Skip(1) _
                    .Take(allTokens.Length - 2) _
                    .ToArray
            End If

            Dim ex As Exception = Nothing
            Dim lineBlocks = allTokens _
                .SplitByTopLevelDelimiter(TokenType.terminator, err:=ex) _
                .SafeQuery _
                .ToArray
            Dim isJSON As Boolean = False

            If Not ex Is Nothing Then
                Return SyntaxResult.CreateError(ex, opts.SetCurrentRange(allTokens))
            End If

            If lineBlocks.Length = 1 Then
                ' 可能是一行代码
                ' 也可能是 {a: value}
                isJSON = lineBlocks(Scan0).isJsonMember
            Else
                isJSON = False
            End If

            If Not isJSON Then
                Dim lines As New List(Of Expression)

                For Each line As Token() In lineBlocks
                    If line.Length = 0 OrElse line.isTerminator(opts.keepsCommentLines) Then
                        ' skip code comments
                        ' do nothing
                        Continue For
                    End If

                    [error] = Nothing
                    line _
                        .HandleExpressionBlock(Sub(exr) [error] = exr, opts) _
                        .DoCall(AddressOf lines.AddRange)

                    If Not [error] Is Nothing Then
                        Return [error]
                    End If
                Next

                Return New SyntaxResult(New ClosureExpression(lines.ToArray))
            Else
                Dim members As New List(Of NamedValue(Of Expression))

                lineBlocks = allTokens _
                    .SplitByTopLevelDelimiter(TokenType.comma, err:=ex) _
                    .SafeQuery _
                    .ToArray

                If Not ex Is Nothing Then
                    Return SyntaxResult.CreateError(ex, opts.SetCurrentRange(allTokens))
                End If

                For Each member As Token() In lineBlocks
                    If member.Length = 0 OrElse (member.Length = 1 AndAlso member(Scan0).name = TokenType.comma) Then
                        ' skip code comments
                        ' do nothing
                        Continue For
                    End If

                    If Not member.isJsonMember Then
                        Return SyntaxResult.CreateError(New InvalidExpressionException, opts.SetCurrentRange(member))
                    End If

                    Dim name As Token = member(Scan0)
                    Dim value As Token() = member.Skip(2).ToArray
                    Dim valExpr = opts.ParseExpression(value, opts)

                    If valExpr.isException Then
                        Return valExpr
                    End If

                    Call New NamedValue(Of Expression) With {
                        .Name = name.text,
                        .Value = valExpr.expression
                    }.DoCall(AddressOf members.Add)
                Next

                Return New JSONLiteral(members)
            End If
        End Function

        <Extension>
        Private Function isJsonMember(line As Token()) As Boolean
            If Not line.Length >= 3 Then
                Return False
            End If

            Dim name As Token = line(Scan0)

            If Not (name.name = TokenType.identifier OrElse name.name = TokenType.stringLiteral) Then
                Return False
            End If

            Dim jsonAssign As Token = line(1)

            If Not jsonAssign.name = TokenType.sequence Then
                Return False
            End If

            Return True
        End Function
    End Module
End Namespace

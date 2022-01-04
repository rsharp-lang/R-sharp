#Region "Microsoft.VisualBasic::a7d7dcb9efff2406e95a77a88571ebe4, R#\Interpreter\Extensions.vb"

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

'     Module Extensions
' 
'         Function: (+2 Overloads) GetExpressions, isTerminator
' 
'         Sub: SyntaxErrorHelper
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter

    <HideModuleName> Module Extensions

        ''' <summary>
        ''' 
        ''' </summary>
        ReadOnly ignores As Index(Of TokenType) = {
            TokenType.comment,
            TokenType.terminator
        }

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        <DebuggerStepThrough>
        Friend Function isTerminator(block As Token(), keepsComment As Boolean) As Boolean
            If block.Length <> 1 Then
                Return False
            Else
                Dim token As Token = block(Scan0)

                If keepsComment Then
                    Return token.name = TokenType.terminator
                Else
                    Return token.name Like ignores
                End If
            End If
        End Function

        ''' <summary>
        ''' 从所输入的R脚本文本之中解析出语句表达式列表
        ''' </summary>
        ''' <param name="Rscript"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetExpressions(Rscript As Rscript, opts As SyntaxBuilderOptions) As IEnumerable(Of Expression)
            Dim tokens As Token() = Rscript.GetTokens
            ' check for invalid tokens
            ' or syntax error
            Dim syntaxErrors As Token = tokens _
                .Where(Function(t) t.name = TokenType.invalid) _
                .FirstOrDefault

            If Not syntaxErrors Is Nothing Then
                Dim info As New StringBuilder
                Dim line As String = Rscript.GetByLineNumber(syntaxErrors.span.line)

                info.AppendLine($"parser error near by {syntaxErrors.span.ToString} produce an invalid syntax token:")
                info.AppendLine()
                info.AppendLine("    " & line)
                info.AppendLine("    " & New String("~"c, line.Length))

                opts.error = info.ToString

                Return {}
            Else
                Return tokens.GetExpressions(Nothing, opts)
            End If
        End Function

        <Extension>
        Friend Iterator Function HandleExpressionBlock(block As Token(),
                                                       errHandler As Action(Of SyntaxResult),
                                                       opts As SyntaxBuilderOptions) As IEnumerable(Of Expression)
            ' have some bugs about
            ' handles closure
            ' 
            Dim parts() = block _
                .Where(Function(t)
                           If opts.keepsCommentLines Then
                               Return True
                           Else
                               Return Not t.name = TokenType.comment
                           End If
                       End Function) _
                .SplitByTopLevelDelimiter(TokenType.close,, "}") _
                .Split(2)

            For Each joinBlock As Token()() In parts
                block = joinBlock(Scan0) _
                    .JoinIterates(joinBlock.ElementAtOrDefault(1)) _
                    .ToArray

                If opts.keepsCommentLines Then
                    If block.All(Function(t) t.name = TokenType.comment) Then
                        ' is top level code comments
                        If block.Length = 1 Then
                            Yield New CodeComment(block(Scan0))
                        Else
                            Yield CodeComment.FromBlockComments(block)
                        End If

                        Continue For
                    Else
                        Dim i As Integer = 0

                        For Each line As Token In block
                            If line.name = TokenType.comment Then
                                Yield New CodeComment(line)
                            Else
                                Exit For
                            End If

                            i += 1
                        Next

                        block = block _
                            .Skip(i) _
                            .Where(Function(t) t.name <> TokenType.comment) _
                            .ToArray
                    End If
                End If

                Dim expr As SyntaxResult = opts.ParseExpression(block, opts)

                If expr.isException Then
                    ' config error message in the options
                    Call errHandler(expr)
                    Return
                Else
                    Yield expr.expression
                End If
            Next
        End Function

        ''' <summary>
        ''' 将单词化的R脚本文件解析为语句表达式列表
        ''' </summary>
        ''' <param name="tokens">R脚本单词</param>
        ''' <param name="errHandler"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GetExpressions(tokens As Token(), errHandler As Action(Of SyntaxResult), opts As SyntaxBuilderOptions) As IEnumerable(Of Expression)
            Dim err As Exception = Nothing
            Dim hasError As Boolean = False

            If errHandler Is Nothing Then
                errHandler = Sub(syntaxErr)
                                 hasError = True
                                 opts.error = syntaxErr.error.ToString
                             End Sub
            End If

            Dim list = tokens _
                .SplitByTopLevelDelimiter(TokenType.terminator, err:=err) _
                .SafeQuery _
                .ToArray

            If Not err Is Nothing Then
                Call errHandler(SyntaxResult.CreateError(err, opts.SetCurrentRange(tokens)))
            End If

            For Each block As Token() In list
                If block.Length = 0 OrElse block.isTerminator(opts.keepsCommentLines) Then
                    ' skip code comments
                    ' do nothing
                Else
                    For Each line As Expression In block.HandleExpressionBlock(errHandler, opts)
                        Yield line
                    Next
                End If

                If hasError Then
                    Exit For
                End If
            Next
        End Function
    End Module
End Namespace

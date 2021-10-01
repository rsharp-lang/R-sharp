#Region "Microsoft.VisualBasic::a8334029ba39667b00d91f1faf964e6a, R#\Interpreter\Extensions.vb"

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
        Private Function isTerminator(block As Token()) As Boolean
            Return block.Length = 1 AndAlso block(Scan0).name Like ignores
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
                Return tokens.GetExpressions(Rscript, Nothing, opts)
            End If
        End Function

        ''' <summary>
        ''' 将单词化的R脚本文件解析为语句表达式列表
        ''' </summary>
        ''' <param name="tokens">R脚本单词</param>
        ''' <param name="Rscript"></param>
        ''' <param name="errHandler"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function GetExpressions(tokens As Token(),
                                                Rscript As Rscript,
                                                errHandler As Action(Of SyntaxResult, Token()),
                                                opts As SyntaxBuilderOptions) As IEnumerable(Of Expression)

            Dim err As Exception = Nothing
            Dim expr As SyntaxResult

            If errHandler Is Nothing Then
                errHandler = Sub(syntaxErr, block)
                                 Call SyntaxErrorHelper(
                                    opts:=opts,
                                    syntaxResult:=syntaxErr,
                                    Rscript:=Rscript,
                                    tokens:=block
                                 )
                             End Sub
            End If

            For Each block As Token() In tokens.SplitByTopLevelDelimiter(TokenType.terminator, err:=err).SafeQuery
                If block.Length = 0 OrElse block.isTerminator Then
                    ' skip code comments
                    ' do nothing
                ElseIf Not err Is Nothing Then
                    Call errHandler(New SyntaxResult(err, opts.debug), {})
                Else
                    ' have some bugs about
                    ' handles closure
                    Dim parts() = block _
                        .Where(Function(t) Not t.name = TokenType.comment) _
                        .SplitByTopLevelDelimiter(TokenType.close,, "}") _
                        .Split(2)

                    For Each joinBlock As Token()() In parts
                        block = joinBlock(Scan0).JoinIterates(joinBlock.ElementAtOrDefault(1)).ToArray
                        expr = Expression.CreateExpression(block, opts)

                        If expr.isException Then
                            ' config error message in the options
                            Call errHandler(expr, block)
                            Return
                        Else
                            Yield expr.expression
                        End If
                    Next
                End If
            Next
        End Function

        Private Sub SyntaxErrorHelper(Rscript As Rscript, tokens As Token(), syntaxResult As SyntaxResult, opts As SyntaxBuilderOptions)
            Dim rawText As String = Rscript.GetRawText(tokens)
            Dim err As Exception = syntaxResult.error
            Dim message As String = err.ToString
            Dim errorsPromptLine = New String("~"c, rawText.LineTokens.MaxLengthString.Length)

            message &= vbCrLf & vbCrLf & "Syntax error nearby:"
            message &= vbCrLf & vbCrLf & rawText
            message &= vbCrLf & errorsPromptLine
            message &= vbCrLf & vbCrLf & $"Range from {tokens.First.span.start} at line {tokens.First.span.line}, to {tokens.Last.span.stops} at line {tokens.Last.span.line}."

            If opts.debug Then
                message &= vbCrLf & vbCrLf & "The parser stack trace:"
                message &= vbCrLf & vbCrLf & syntaxResult.stackTrace
                message &= vbCrLf & vbCrLf & "   --=== End of the R# Parser StackTrace ===--"
                message &= vbCrLf & vbCrLf
            End If

            If Not opts.debug Then
                opts.error = message
            Else
                Throw New Exception(message)
            End If
        End Sub
    End Module
End Namespace

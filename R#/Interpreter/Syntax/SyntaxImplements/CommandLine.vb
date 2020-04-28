#Region "Microsoft.VisualBasic::fca03c271f3cadeb6874f5175abf540d, R#\Interpreter\Syntax\SyntaxImplements\CommandLine.vb"

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

'     Module CommandLineSyntax
' 
'         Function: CommandLine, CommandLineArgument, isInterpolation
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module CommandLineSyntax

        Const InterpolatePattern$ = "[$]\{.+?\}"

        Public Function CommandLine(shell As Token, opts As SyntaxBuilderOptions) As SyntaxResult
            If Not shell.text _
                .Match(InterpolatePattern, RegexOptions.Singleline) _
                .StringEmpty Then

                ' 如果是字符串插值，在Windows平台上会需要注意转义问题
                ' 因为windows平台上文件夹符号为\
                ' 可能会对$产生转义，从而导致字符串插值失败
                Dim temp As SyntaxResult = SyntaxImplements.StringInterpolation(shell, opts)

                If temp.isException Then
                    Return temp
                Else
                    Return New CommandLine(temp.expression)
                End If
            Else
                Dim literalSyntax As SyntaxResult = SyntaxImplements.LiteralSyntax(shell, opts)

                If literalSyntax.isException Then
                    Return literalSyntax
                Else
                    Return New CommandLine(literalSyntax.expression)
                End If
            End If
        End Function

        <Extension>
        Friend Function isInterpolation(text As String) As Boolean
            Return Not text.Match(InterpolatePattern, RegexOptions.Singleline).StringEmpty
        End Function

        Public Function CommandLineArgument(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim name As SyntaxResult

            If tokens.First = TokenType.iif Then
                name = tokens _
                    .Skip(1) _
                    .DoCall(Function(code)
                                Return Expression.CreateExpression(code, opts)
                            End Function)
            Else
                name = Expression.CreateExpression(tokens, opts)
            End If

            If name.isException Then
                Return name
            Else
                Return New ArgumentValue(name.expression)
            End If
        End Function
    End Module
End Namespace

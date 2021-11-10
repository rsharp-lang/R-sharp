#Region "Microsoft.VisualBasic::09e83acb4e5434d9eaf2cd8274a0fc4b, R#\Interpreter\Syntax\SyntaxImplements\ClosureExpressionSyntax.vb"

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
'         Function: ClosureExpression
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
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
            Dim lineBlocks = allTokens.SplitByTopLevelDelimiter(TokenType.terminator, err:=ex).SafeQuery.ToArray
            Dim isJSON As Boolean = False

            If Not ex Is Nothing Then
                Return New SyntaxResult(ex, opts.debug)
            End If

            If lineBlocks.Length = 1 Then
                ' 可能是一行代码
            Else
                isJSON = False
            End If

            Dim lines As Expression() = Interpreter _
                .GetExpressions(allTokens, Nothing, Sub(ex, null) [error] = ex, opts) _
                .ToArray

            If Not [error] Is Nothing Then
                Return [error]
            Else
                Return New SyntaxResult(New ClosureExpression(lines))
            End If
        End Function
    End Module
End Namespace

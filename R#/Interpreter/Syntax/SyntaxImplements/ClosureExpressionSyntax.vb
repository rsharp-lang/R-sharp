#Region "Microsoft.VisualBasic::3f0b7e3f0a0aabd24c77de5d37b506f0, R#\Interpreter\Syntax\SyntaxImplements\ClosureExpressionSyntax.vb"

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

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
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
            Dim lines As Expression() = Interpreter _
                .GetExpressions(tokens.ToArray, Nothing, Sub(ex) [error] = ex, opts) _
                .ToArray

            If Not [error] Is Nothing Then
                Return [error]
            Else
                Return New SyntaxResult(New ClosureExpression(lines))
            End If
        End Function
    End Module
End Namespace

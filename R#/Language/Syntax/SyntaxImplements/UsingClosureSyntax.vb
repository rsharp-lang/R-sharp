#Region "Microsoft.VisualBasic::4bdeded76b888a76e90d87cb41358947, R#\Language\Syntax\SyntaxImplements\UsingClosureSyntax.vb"

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

    '   Total Lines: 54
    '    Code Lines: 45
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 2.22 KB


    '     Module UsingClosureSyntax
    ' 
    '         Function: UsingClosure
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module UsingClosureSyntax

        Public Function UsingClosure(code As IEnumerable(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim input As Token() = code.IteratesALL.ToArray
            Dim tokens As Token()() = input _
                .SplitByTopLevelDelimiter(TokenType.close) _
                .ToArray
            Dim parmPart As Token() = tokens(Scan0)

            If tokens(1).Length = 1 AndAlso tokens(1)(Scan0) = (TokenType.close, ")") Then
                parmPart = parmPart + tokens(1).AsList
            End If

            Dim closureSyntax = SyntaxImplements.ClosureExpression(tokens(2).Skip(1), opts)

            If closureSyntax.isException Then
                Return closureSyntax
            Else
                tokens = parmPart.SplitByTopLevelDelimiter(TokenType.keyword, tokenText:="as")
            End If

            Dim paramsSyntax = SyntaxImplements.DeclareNewSymbol(tokens(Scan0), tokens(2), opts, True)

            If paramsSyntax.isException Then
                Return paramsSyntax
            Else
                Dim stackframe As New StackFrame With {
                    .File = opts.source.fileName,
                    .Line = parmPart(Scan0).span.line,
                    .Method = New Method With {
                        .Method = $"using({tokens(Scan0).Select(Function(t) t.text).JoinBy(" ")})",
                        .[Module] = "using_closure",
                        .[Namespace] = "SMRUCC/R#"
                    }
                }
                Dim [using] As New UsingClosure(
                    params:=paramsSyntax.expression,
                    closure:=closureSyntax.expression,
                    stackframe:=stackframe
                )

                Return New SyntaxResult([using])
            End If
        End Function
    End Module
End Namespace

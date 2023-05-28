#Region "Microsoft.VisualBasic::5d8513064f36b4174d2f5f23fa76ebd1, F:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxImplements/LinqExpressionSyntax.vb"

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

    '   Total Lines: 41
    '    Code Lines: 30
    ' Comment Lines: 6
    '   Blank Lines: 5
    '     File Size: 1.65 KB


    '     Module LinqExpressionSyntax
    ' 
    '         Function: LinqExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.LINQ.Syntax
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module LinqExpressionSyntax

        Friend ReadOnly linqKeywordDelimiters As String() = {"where", "distinct", "select", "order", "group", "let"}

        ''' <summary>
        ''' produce a <see cref="LinqQuery"/>
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LinqExpression(tokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim stackframe As New StackFrame With {
                .File = opts.source.fileName,
                .Line = tokens.First()(Scan0).span.line,
                .Method = New Method With {
                    .Method = "linq_closure",
                    .[Module] = "linq_closure",
                    .[Namespace] = "SMRUCC/R#"
                }
            }
            Dim LINQ As SyntaxParserResult = tokens.IteratesALL.PopulateQueryExpression(opts)

            If LINQ.isError Then
                Return New SyntaxResult(LINQ.message)
            Else
                Return New LinqQuery(LINQ.expression, stackframe)
            End If
        End Function
    End Module
End Namespace

﻿#Region "Microsoft.VisualBasic::7fdba9edf0d42dfe2c83642344bd1554, R#\Language\Syntax\SyntaxImplements\WhileLoopSyntax.vb"

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

    '   Total Lines: 35
    '    Code Lines: 28 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (20.00%)
    '     File Size: 1.39 KB


    '     Module WhileLoopSyntax
    ' 
    '         Function: CreateLoopExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module WhileLoopSyntax

        Public Function CreateLoopExpression(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim keyword As Token = code(Scan0)(Scan0)
            Dim test As SyntaxResult

            code = code _
                .Skip(1) _
                .IteratesALL _
                .SplitByTopLevelDelimiter(Language.TokenType.close)
            test = opts.ParseExpression(code(Scan0).Skip(1), opts)

            If test.isException Then
                Return test
            End If

            Dim tokens As Token() = code.Skip(1).IteratesALL.Skip(1).ToArray
            Dim loopBody As SyntaxResult = SyntaxImplements.ClosureExpression(tokens.Skip(1).Take(tokens.Length - 2), opts)
            Dim stackframe As StackFrame = opts.GetStackTrace(keyword, "whileLoop_closure")

            If loopBody.isException Then
                Return loopBody
            Else
                Return New WhileLoop(test.expression, loopBody.expression, stackframe)
            End If
        End Function
    End Module
End Namespace

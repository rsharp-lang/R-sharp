#Region "Microsoft.VisualBasic::be4f91fa26cb919102640ee69cb0d7ba, R-sharp\R#\Interpreter\Syntax\SyntaxImplements\TryCatchSyntax.vb"

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

    '   Total Lines: 40
    '    Code Lines: 29
    ' Comment Lines: 2
    '   Blank Lines: 9
    '     File Size: 1.49 KB


    '     Module TryCatchSyntax
    ' 
    '         Function: CreateTryError
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module TryCatchSyntax

        <Extension>
        Public Function CreateTryError(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim code = tokens.SplitByTopLevelDelimiter(TokenType.close, tokenText:=")")
            Dim tryVal = ExpressionBuilder.ParseExpression(New List(Of Token()) From {code(Scan0).Skip(2).Take(code(Scan0).Length - 2).ToArray}, opts)
            Dim stackframe As StackFrame = opts.GetStackTrace(tokens(Scan0), "try")

            If tryVal.isException Then
                Return tryVal
            End If

            Dim catchVal As SyntaxResult

            If code = 2 Then
                ' no catch
                catchVal = Nothing
            Else
                ' has catch
                catchVal = ClosureExpressionSyntax.ClosureExpression(code(2), opts)

                If catchVal.isException Then
                    Return catchVal
                End If
            End If

            Return New TryCatchExpression(tryVal.expression, catchVal?.expression, stackframe)
        End Function

    End Module
End Namespace

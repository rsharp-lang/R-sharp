#Region "Microsoft.VisualBasic::570f0f12f049dac865b7c13d0dcc42af, R#\Interpreter\Syntax\SyntaxImplements\FunctionInvokeSyntax.vb"

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

    '     Module FunctionInvokeSyntax
    ' 
    '         Function: FunctionInvoke
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module FunctionInvokeSyntax

        Public Function FunctionInvoke(tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim params = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .ToArray

            Dim funcName = New Literal(tokens(Scan0).text)
            Dim span = tokens(Scan0).span
            Dim parameters As New List(Of Expression)

            For Each token As SyntaxResult In params _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .Select(Function(param)
                            ' name = value
                            ' value
                            ' fix bugs of using the keyword as identifier
                            Dim parts = param.SplitByTopLevelDelimiter(TokenType.operator, False, "=")

                            If parts = 3 Then
                                ' name = value
                                Return SyntaxImplements.ValueAssign(parts, opts)
                            Else
                                ' is a value expression
                                Return Expression.CreateExpression(param, opts)
                            End If
                        End Function)

                If token.isException Then
                    Return token
                Else
                    parameters.Add(token.expression)
                End If
            Next

            Return New FunctionInvoke(funcName, parameters.ToArray)
        End Function
    End Module
End Namespace

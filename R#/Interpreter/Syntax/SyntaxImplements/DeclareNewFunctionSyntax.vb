#Region "Microsoft.VisualBasic::813b515c3d75d06036670aa70564a8b4, R#\Interpreter\Syntax\SyntaxImplements\DeclareNewFunctionSyntax.vb"

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

    '     Module DeclareNewFunctionSyntax
    ' 
    '         Function: DeclareNewFunction, getParameters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module DeclareNewFunctionSyntax

        Public Function DeclareNewFunction(code As List(Of Token())) As SyntaxResult
            Dim [declare] As Token() = code(4)
            Dim parts As List(Of Token()) = [declare].SplitByTopLevelDelimiter(TokenType.close)
            Dim paramPart As Token() = parts(Scan0).Skip(1).ToArray
            Dim bodyPart As SyntaxResult = parts(2).Skip(1) _
                .ToArray _
                .DoCall(AddressOf SyntaxImplements.ClosureExpression)

            If bodyPart.isException Then
                Return bodyPart
            End If

            Dim funcName As String = code(1)(Scan0).text
            Dim params As New List(Of DeclareNewVariable)
            Dim err As SyntaxResult = getParameters(paramPart, params)

            If err.isException Then
                Return err
            Else
                Return New DeclareNewFunction With {
                    .funcName = funcName,
                    .body = bodyPart.expression,
                    .envir = Nothing,
                    .params = params.ToArray
                }
            End If
        End Function

        Private Function getParameters(tokens As Token(), ByRef params As List(Of DeclareNewVariable)) As SyntaxResult
            Dim parts As Token()() = tokens.SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .ToArray

            For Each syntaxTemp As SyntaxResult In parts _
                .Select(Function(t)
                            Dim [let] As New List(Of Token) From {
                                New Token With {.name = TokenType.keyword, .text = "let"}
                            }
                            Return SyntaxImplements.DeclareNewVariable([let] + t)
                        End Function)

                If syntaxTemp.isException Then
                    Return syntaxTemp
                Else
                    params.Add(syntaxTemp.expression)
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace

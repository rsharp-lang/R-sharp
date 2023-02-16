#Region "Microsoft.VisualBasic::0a0e4c9440e6f5428d2f615f64dabbf4, E:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxImplements/DeclareNewFunctionSyntax.vb"

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

    '   Total Lines: 140
    '    Code Lines: 106
    ' Comment Lines: 16
    '   Blank Lines: 18
    '     File Size: 6.03 KB


    '     Module DeclareNewFunctionSyntax
    ' 
    '         Function: DeclareAnonymousFunction, DeclareNewFunction, getParameters, ReturnValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module DeclareNewFunctionSyntax

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="code">
        ''' 这个数组应该是有两个元素组成：
        ''' 
        ''' 1. function
        ''' 2. (...){...}
        ''' </param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        Public Function DeclareAnonymousFunction(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim [let] = New Token() {New Token(TokenType.keyword, "let")}
            Dim name As New Token(TokenType.identifier, $"<${App.GetNextUniqueName("anonymous_")}>") With {
                .span = code(Scan0)(Scan0).span
            }
            Dim [as] = New Token() {New Token(TokenType.keyword, "as")}

            code = ({[let], New Token() {name}, [as]}) + code

            Return SyntaxImplements.DeclareNewFunction(code, opts)
        End Function

        Public Function ReturnValue(value As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            With value.ToArray
                ' just return
                ' no value
                If .Length = 0 Then
                    Return New ReturnValue(Literal.NULL)
                Else
                    Dim valueSyntax As SyntaxResult =
                        .DoCall(Function(tokens)
                                    Return opts.ParseExpression(tokens, opts)
                                End Function)

                    If valueSyntax.isException Then
                        Return valueSyntax
                    Else
                        Return New ReturnValue(valueSyntax.expression)
                    End If
                End If
            End With
        End Function

        Public Function DeclareNewFunction(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim [declare] As Token() = code.Skip(4).IteratesALL.ToArray
            Dim parts As List(Of Token()) = [declare].SplitByTopLevelDelimiter(TokenType.close)
            Dim paramPart As Token() = parts(Scan0).Skip(1).ToArray
            Dim bodyPart As SyntaxResult
            ' test if is an inline function
            Dim bodyPart2 = parts.Skip(2).IteratesALL.ToArray

            If bodyPart2(Scan0) = (TokenType.open, "{") AndAlso bodyPart2.Last = (TokenType.close, "}") Then
                bodyPart = bodyPart2 _
                    .Skip(1) _
                    .Take(bodyPart2.Length - 2) _
                    .ToArray _
                    .DoCall(Function(tokens)
                                Return SyntaxImplements.ClosureExpression(tokens, opts)
                            End Function)
            Else
                bodyPart = SyntaxImplements.ClosureExpression(bodyPart2, opts)
            End If

            If bodyPart.isException Then
                Return bodyPart
            End If

            Dim funcName As String = code(1)(Scan0).text
            Dim params As New List(Of DeclareNewSymbol)
            Dim err As SyntaxResult = getParameters(paramPart, params, opts)

            If err IsNot Nothing AndAlso err.isException Then
                Return err
            Else
                Dim stack As New StackFrame With {
                    .File = opts.source.fileName,
                    .Line = code(1)(Scan0).span.line,
                    .Method = New Method With {
                        .Method = funcName,
                        .[Module] = "declare_function",
                        .[Namespace] = "SMRUCC/R#"
                    }
                }
                Dim func As New DeclareNewFunction(
                    funcName:=funcName,
                    body:=bodyPart.expression,
                    parameters:=params.ToArray,
                    stackframe:=stack
                )

                Return New SyntaxResult(func)
            End If
        End Function

        Public Function getParameters(tokens As Token(), ByRef params As List(Of DeclareNewSymbol), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim parts As Token()() = tokens.SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .ToArray

            For Each syntaxTemp As SyntaxResult In parts _
                .Select(Function(t)
                            ' param
                            ' param = value
                            Dim syntaxParts = t.SplitByTopLevelDelimiter(TokenType.operator, False, "=")

                            If syntaxParts = 1 Then
                                Return SyntaxImplements.DeclareNewSymbol(t, opts)
                            ElseIf syntaxParts = 3 Then
                                Return SyntaxImplements.DeclareNewSymbol(syntaxParts(0), syntaxParts(2), opts, True)
                            Else
                                Return SyntaxResult.CreateError("syntax error on declare function parameters!", opts.SetCurrentRange(t))
                            End If
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

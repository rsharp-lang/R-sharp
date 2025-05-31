#Region "Microsoft.VisualBasic::603ae28760c97166b138d39e85066b4b, R#\Language\Syntax\SyntaxImplements\DeclareNewFunctionSyntax.vb"

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

    '   Total Lines: 208
    '    Code Lines: 154 (74.04%)
    ' Comment Lines: 21 (10.10%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 33 (15.87%)
    '     File Size: 8.62 KB


    '     Module DeclareNewFunctionSyntax
    ' 
    '         Function: CheckAnnotatedFunction, DeclareAnnotatedFunction, DeclareAnonymousFunction, DeclareNewFunction, getParameters
    '                   ReturnValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Syntax.SyntaxParser.SyntaxImplements

    Module DeclareNewFunctionSyntax

        <Extension>
        Public Function CheckAnnotatedFunction(code As List(Of Token())) As Boolean
            If code.Count < 5 Then
                Return False
            End If

            ' [@attr "xxxx"]
            ' [@attr2]
            ' const xxx = function() {
            '     ...
            ' }
            Dim func_keyword = code(code.Count - 2)

            If func_keyword.Length <> 1 OrElse Not func_keyword(0).isKeyword("function") Then
                Return False
            End If

            Dim attrs = code(0)

            If attrs.Length < 3 Then
                Return False
            ElseIf attrs.First.name <> TokenType.open OrElse attrs.Last.name <> TokenType.close Then
                Return False
            ElseIf attrs.First.text <> "[" OrElse attrs.Last.text <> "]" Then
                Return False
            End If

            Return True
        End Function

        <Extension>
        Public Function DeclareAnnotatedFunction(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim attrs = code(0).SplitByTopLevelDelimiter(TokenType.close,, "]")
            Dim func = DeclareNewFunction(New List(Of Token())(code.Skip(1)), opts)

            For Each attr As Token() In attrs
                If attr.Length = 1 OrElse attr(0).name = TokenType.close Then
                    Continue For
                End If

                Dim attrName As String = attr(1).text
                Dim attrVals As String() = attr.Skip(2).Select(Function(a) a.text).ToArray

                Call DirectCast(func.expression, SymbolExpression).AddCustomAttribute(attrName, attrVals)
            Next

            Return func
        End Function

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

            If parts = 0 Then
                opts.SetCurrentRange(code.IteratesALL.ToArray)
                Return New SyntaxResult(SyntaxError.CreateError(opts, New Exception("no function body to declared!")))
            End If

            Dim paramPart As Token() = parts(Scan0).Skip(1).ToArray
            Dim bodyPart As SyntaxResult
            ' test if is an inline function
            Dim bodyPart2 = parts.Skip(2).IteratesALL.ToArray

            If bodyPart2.IsNullOrEmpty Then
                Dim allspan As CodeSpan() = code _
                    .IteratesALL _
                    .Where(Function(ti) Not ti.span Is Nothing) _
                    .Select(Function(ti) ti.span) _
                    .ToArray
                Dim t0 As CodeSpan = allspan.First
                Dim t1 As CodeSpan = allspan.Last

                Return New SyntaxResult(SyntaxError.CreateError(opts, "no function body was found!", t0, t1))
            End If

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

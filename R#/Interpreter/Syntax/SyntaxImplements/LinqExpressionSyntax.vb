#Region "Microsoft.VisualBasic::1150f5ed2c15943954030465a49e42ae, R#\Interpreter\Syntax\SyntaxImplements\LinqExpressionSyntax.vb"

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

    '     Module LinqExpressionSyntax
    ' 
    '         Function: LinqExpression
    '         Class LinqSyntaxParser
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: doParseLINQProgram, groupBy, local, project, sort
    '                       which
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module LinqExpressionSyntax

        <Extension>
        Public Function LinqExpression(tokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim variables As New List(Of String)
            Dim i As Integer = 0
            Dim sequence As SyntaxResult = Nothing
            Dim locals As New List(Of DeclareNewVariable)

            For i = 1 To tokens.Count - 1
                If tokens(i).isIdentifier Then
                    variables.Add(tokens(i)(Scan0).text)
                ElseIf tokens(i).isKeyword("in") Then
                    sequence = Expression.CreateExpression(tokens(i + 1), opts)
                    Exit For
                End If
            Next

            If sequence Is Nothing Then
                Return New SyntaxResult("Missing sequence provider!", opts.debug)
            ElseIf sequence.isException Then
                Return sequence
            Else
                i += 2
                locals = New DeclareNewVariable With {
                    .names = variables.ToArray,
                    .hasInitializeExpression = False,
                    .value = Nothing
                }
            End If

            tokens = tokens _
                .Skip(i) _
                .IteratesALL _
                .SplitByTopLevelDelimiter(TokenType.keyword)

            Dim projection As Expression = Nothing
            Dim output As New List(Of FunctionInvoke)
            Dim program As ClosureExpression = Nothing
            Dim p As New Pointer(Of Token())(tokens)
            Dim parser As New LinqSyntaxParser(p, opts)
            Dim [error] As SyntaxResult = parser.doParseLINQProgram(locals, projection, output, program)
            Dim stackframe As New StackFrame With {
                .File = opts.source.fileName,
                .Line = tokens.First()(Scan0).span.line,
                .Method = New Method With {
                    .Method = "linq_closure",
                    .[Module] = "linq_closure",
                    .[Namespace] = "SMRUCC/R#"
                }
            }

            If Not [error] Is Nothing AndAlso [error].isException Then
                Return [error]
            Else
                Return New LinqExpression(
                    locals:=locals,
                    sequence:=sequence.expression,
                    program:=program,
                    projection:=projection,
                    output:=output,
                    stackframe:=stackframe
                )
            End If
        End Function

        ReadOnly linqKeywordDelimiters As String() = {"where", "distinct", "select", "order", "group", "let"}

        Private Class LinqSyntaxParser

            Dim buffer As New List(Of Token())
            Dim token As Token()
            Dim program As New List(Of Expression)
            Dim p As Pointer(Of Token())
            Dim opts As SyntaxBuilderOptions

            Sub New(p As Pointer(Of Token()), opts As SyntaxBuilderOptions)
                Me.p = p
                Me.opts = opts
            End Sub

            Private Function local(locals As List(Of DeclareNewVariable)) As SyntaxResult
                buffer += token

                Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                    buffer += ++p
                Loop

                Dim declares = buffer _
                    .IteratesALL _
                    .SplitByTopLevelDelimiter(TokenType.operator, True) _
                    .DoCall(Function(blocks)
                                Return SyntaxImplements.DeclareNewVariable(blocks, False, opts)
                            End Function)

                If declares.isException Then
                    Return declares
                End If

                Dim [declare] As DeclareNewVariable = declares.expression

                program += New ValueAssign([declare].names, [declare].value)
                locals += [declare]
                [declare].value = Nothing

                Return Nothing
            End Function

            Private Function which() As SyntaxResult
                Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                    buffer += ++p
                Loop

                Dim exprSyntax As SyntaxResult = buffer _
                    .IteratesALL _
                    .DoCall(Function(code)
                                Return Expression.CreateExpression(code, opts)
                            End Function)

                If exprSyntax.isException Then
                    Return exprSyntax
                End If

                ' 需要取反才可以正常执行中断语句
                ' 例如 where 5 < 2
                ' if test的结果为false
                ' 则当前迭代循环需要跳过
                ' 即执行trueclosure部分
                ' 或者添加一个else closure
                Dim booleanExp As New BinaryExpression(exprSyntax.expression, Literal.FALSE, "==")
                Dim stackframe As StackFrame = opts.GetStackTrace(buffer.First(Scan0))

                program += New IfBranch(
                    ifTest:=booleanExp,
                    trueClosure:={New ReturnValue(Literal.NULL)},
                    stackframe:=stackframe
                )

                Return Nothing
            End Function

            Private Function sort(outputs As List(Of FunctionInvoke), stacktrace As StackFrame) As SyntaxResult
                ' order by xxx asc
                Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                    buffer += ++p
                Loop

                token = buffer.IteratesALL.ToArray

                If Not token(Scan0).isKeyword("by") Then
                    Return New SyntaxResult("Missing 'by' keyword for sort!", opts.debug)
                End If

                ' skip first by keyword
                Dim exprSyntax As SyntaxResult = token _
                    .Skip(1) _
                    .Take(token.Length - 2) _
                    .DoCall(Function(code)
                                Return Expression.CreateExpression(code, opts)
                            End Function)

                If exprSyntax.isException Then
                    Return exprSyntax
                End If

                outputs += New FunctionInvoke("sort", stacktrace, exprSyntax.expression, New Literal(token.Last.isKeyword("descending")))

                Return Nothing
            End Function

            Private Function project(ByRef projection As Expression, stacktrace As StackFrame) As SyntaxResult
                If Not projection Is Nothing Then
                    Return New SyntaxResult("Only allows one project function!", opts.debug)
                End If

                Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                    buffer += ++p
                Loop

                Dim projectSyntax = Expression.CreateExpression(buffer.IteratesALL, opts)

                If projectSyntax.isException Then
                    Return projectSyntax
                Else
                    projection = projectSyntax.expression
                End If

                If TypeOf projection Is VectorLiteral Then
                    projection = New FunctionInvoke("list", stacktrace, DirectCast(projection, VectorLiteral).ToArray)
                End If

                Return Nothing
            End Function

            Private Function groupBy() As SyntaxResult
                Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                    buffer += ++p
                Loop

                Return New SyntaxResult(New NotImplementedException, opts.debug)
            End Function

            Friend Function doParseLINQProgram(locals As List(Of DeclareNewVariable),
                                               ByRef projection As Expression,
                                               ByRef outputs As List(Of FunctionInvoke),
                                               ByRef programClosure As ClosureExpression) As SyntaxResult

                Dim syntaxResult As New Value(Of SyntaxResult)
                Dim stacktrace As StackFrame

                Do While Not p.EndRead
                    buffer *= 0
                    token = ++p

                    If Not token.isKeyword Then
                        Continue Do
                    Else
                        stacktrace = New StackFrame With {
                            .File = opts.source.fileName,
                            .Line = token(Scan0).span.line,
                            .Method = New Method With {
                                .Method = token(Scan0).text,
                                .[Module] = "exec_linq",
                                .[Namespace] = "SMRUCC/R#"
                            }
                        }
                    End If

                    Select Case token(Scan0).text
                        Case "let"
                            If Not (syntaxResult = local(locals)) Is Nothing Then
                                Return syntaxResult
                            End If
                        Case "where"
                            If Not (syntaxResult = which()) Is Nothing Then
                                Return syntaxResult
                            End If
                        Case "distinct"
                            outputs += New FunctionInvoke("unique", stacktrace, New SymbolReference("$"))
                        Case "order"
                            If Not (syntaxResult = sort(outputs, stacktrace)) Is Nothing Then
                                Return syntaxResult
                            End If
                        Case "select"
                            If Not (syntaxResult = project(projection, stacktrace)) Is Nothing Then
                                Return syntaxResult
                            End If
                        Case "group"
                            If Not (syntaxResult = groupBy()) Is Nothing Then
                                Return syntaxResult
                            End If
                        Case Else
                            Return New SyntaxResult(New SyntaxErrorException, opts.debug)
                    End Select
                Loop

                programClosure = New ClosureExpression(program.ToArray)

                Return Nothing
            End Function
        End Class
    End Module
End Namespace

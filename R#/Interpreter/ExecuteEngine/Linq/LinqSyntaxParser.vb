Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.ExecuteEngine.LINQ

    Friend Class LinqSyntaxParser

        Dim buffer As New List(Of Token())
        Dim token As Token()
        Dim program As New List(Of Expression)
        Dim p As Pointer(Of Token())
        Dim opts As SyntaxBuilderOptions

        Sub New(p As Pointer(Of Token()), opts As SyntaxBuilderOptions)
            Me.p = p
            Me.opts = opts
        End Sub

        Private Function local(locals As List(Of DeclareNewSymbol)) As SyntaxResult
            buffer += token

            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                buffer += ++p
            Loop

            Dim declares = buffer _
                .IteratesALL _
                .SplitByTopLevelDelimiter(TokenType.operator, True) _
                .DoCall(Function(blocks)
                            Return SyntaxImplements.DeclareNewSymbol(blocks, False, opts)
                        End Function)

            If declares.isException Then
                Return declares
            End If

            Dim [declare] As DeclareNewSymbol = declares.expression

            program += New ValueAssign([declare].names, [declare].value)
            locals += [declare]
            [declare].m_value = Nothing

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
            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(LinqExpressionSyntax.linqKeywordDelimiters)
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

        Friend Function doParseLINQProgram(locals As List(Of DeclareNewSymbol),
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
End Namespace
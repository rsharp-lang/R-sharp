Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module LinqExpressionSyntax

        Public Function LinqExpression(tokens As List(Of Token())) As SyntaxResult
            Dim variables As New List(Of String)
            Dim i As Integer = 0
            Dim sequence As SyntaxResult = Nothing
            Dim locals As DeclareNewVariable

            For i = 1 To tokens.Count - 1
                If tokens(i).isIdentifier Then
                    variables.Add(tokens(i)(Scan0).text)
                ElseIf tokens(i).isKeyword("in") Then
                    sequence = Expression.CreateExpression(tokens(i + 1))
                    Exit For
                End If
            Next

            If sequence Is Nothing Then
                Throw New SyntaxErrorException
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

            Call New Pointer(Of Token())(tokens).doParseLINQProgram
        End Function

        ReadOnly linqKeywordDelimiters As String() = {"where", "distinct", "select", "order", "group", "let"}

        <Extension>
        Private Sub doParseLINQProgram(p As Pointer(Of Token()))
            Dim buffer As New List(Of Token())
            Dim token As Token()
            Dim program As New List(Of Expression)
            Dim expression As Expression
            Dim output As New List(Of Expression)

            Do While Not p.EndRead
                buffer *= 0
                token = ++p

                If token.isKeyword Then
                    Select Case token(Scan0).text
                        Case "let"
                            buffer += token

                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            Dim declares = buffer _
                                .IteratesALL _
                                .SplitByTopLevelDelimiter(TokenType.operator, True) _
                                .DoCall(Function(blocks)
                                            Return New DeclareNewVariable(blocks)
                                        End Function)

                            program += New ValueAssign(declares.names, declares.value)
                            locals += declares
                            declares.value = Nothing
                        Case "where"
                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            expression = buffer _
                                .IteratesALL _
                                .DoCall(AddressOf Expression.CreateExpression)
                            ' 需要取反才可以正常执行中断语句
                            ' 例如 where 5 < 2
                            ' if test的结果为false
                            ' 则当前迭代循环需要跳过
                            ' 即执行trueclosure部分
                            ' 或者添加一个else closure
                            expression = New BinaryExpression(expression, Literal.FALSE, "==")
                            program += New IfBranch(expression, {New ReturnValue(Literal.NULL)})
                        Case "distinct"
                            output += New FunctionInvoke("unique", New SymbolReference("$"))
                        Case "order"
                            ' order by xxx asc
                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            token = buffer.IteratesALL.ToArray

                            If Not token(Scan0).isKeyword("by") Then
                                Throw New SyntaxErrorException
                            End If

                            ' skip first by keyword
                            expression = token _
                                .Skip(1) _
                                .Take(token.Length - 2) _
                                .DoCall(AddressOf Expression.CreateExpression)
                            output += New FunctionInvoke("sort", expression, New Literal(token.Last.isKeyword("descending")))
                        Case "select"
                            If Not projection Is Nothing Then
                                Throw New SyntaxErrorException("Only allows one project function!")
                            End If

                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            projection = Expression.CreateExpression(buffer.IteratesALL)

                            If TypeOf projection Is VectorLiteral Then
                                projection = New FunctionInvoke("list", DirectCast(projection, VectorLiteral).ToArray)
                            End If
                        Case "group"
                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            Throw New NotImplementedException
                        Case Else
                            Throw New SyntaxErrorException
                    End Select
                End If
            Loop

            Me.program = program.ToArray
            Me.output = output.ToArray
        End Sub

    End Module
End Namespace
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public MustInherit Class Expression

        ''' <summary>
        ''' 推断出的这个表达式可能产生的值的类型
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property type As TypeCodes

        Public MustOverride Function Evaluate(envir As Environment) As Object

        Shared ReadOnly literalTypes As Index(Of TokenType) = {
            TokenType.stringLiteral,
            TokenType.booleanLiteral,
            TokenType.integerLiteral,
            TokenType.numberLiteral
        }

        Public Shared Function CreateExpression(code As Token()) As Expression
            If code(Scan0).name = TokenType.keyword Then
                Dim keyword As String = code(Scan0).text

                Select Case keyword
                    Case "let" : Return New DeclareNewVariable(code)
                    Case "if" : Return New IfBranch(code)

                    Case Else
                        Throw New SyntaxErrorException
                End Select
            ElseIf code.Length = 1 Then
                Dim item As Token = code(Scan0)

                If item.name Like literalTypes Then
                    Return New Literal(item)
                ElseIf item.name = TokenType.identifier Then
                    Return New SymbolReference(item)
                Else
                    Throw New NotImplementedException
                End If
            End If

            If code(Scan0).name = TokenType.identifier Then
                If code(1).name = TokenType.operator Then
                    If code(1).text = "=" OrElse code(1).text = "<-" Then
                        Return New ValueAssign(code)
                    End If
                End If
            End If

            Return code.DoCall(AddressOf CreateTree)
        End Function

        Private Shared Function CreateTree(tokens As Token()) As Expression
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.comma)

            If blocks = 1 Then
                ' 是一个复杂的表达式
                Return blocks(Scan0).DoCall(AddressOf ParseExpressionTree)
            ElseIf blocks(1)(Scan0).text = ":" Then
                ' is a sequence generator syntax
                Return New SequenceLiteral(blocks(Scan0), blocks(2), blocks.ElementAtOrDefault(4))
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Shared Function ParseExpressionTree(tokens As Token()) As Expression
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.operator)

            If blocks = 1 Then
                ' 简单的表达式
                If tokens(Scan0).name = TokenType.identifier AndAlso tokens(1).name = TokenType.open Then
                    Return New FunctionInvoke(tokens)
                ElseIf tokens(Scan0).name = TokenType.open Then
                    Dim openSymbol = tokens(Scan0).text

                    If openSymbol = "[" Then
                        Return New VectorLiteral(tokens)
                    ElseIf openSymbol = "(" Then
                        ' 是一个表达式
                        Return tokens _
                            .Skip(1) _
                            .Take(tokens.Length - 2) _
                            .ToArray _
                            .SplitByTopLevelDelimiter(TokenType.operator) _
                            .DoCall(AddressOf ParseBinaryExpression)
                    ElseIf openSymbol = "{" Then
                        ' 是一个可以产生值的closure
                        Throw New NotImplementedException
                    End If
                End If
            Else
                Return ParseBinaryExpression(blocks)
            End If

            Throw New NotImplementedException
        End Function

        Shared ReadOnly operatorPriority As String() = {"^", "*/", "+-"}

        Private Shared Function ParseBinaryExpression(tokenBlocks As List(Of Token())) As Expression
            Dim buf As New List(Of [Variant](Of Expression, String))
            Dim oplist As New List(Of String)

            For i As Integer = 0 To tokenBlocks.Count - 1
                If i Mod 2 = 0 Then
                    Call buf.Add(Expression.CreateExpression(tokenBlocks(i)))
                Else
                    Call buf.Add(tokenBlocks(i)(Scan0).text)
                    Call oplist.Add(buf.Last.VB)
                End If
            Next

            ' 按照操作符的优先度进行构建
            For Each op As String In operatorPriority
                Dim nop As Integer = oplist.Where(Function(o) op.IndexOf(o) > -1).Count

                ' 从左往右计算
                For i As Integer = 0 To nop - 1
                    For j As Integer = 0 To buf.Count - 1
                        If buf(j) Like GetType(String) AndAlso op.IndexOf(buf(j).VB) > -1 Then
                            ' j-1 and j+1
                            Dim a = buf(j - 1)
                            Dim b = buf(j + 1)
                            Dim be As New BinaryExpression(a, b, buf(j).VB)

                            Call buf.RemoveRange(j - 1, 3)
                            Call buf.Insert(j - 1, be)

                            Exit For
                        End If
                    Next
                Next
            Next

            If buf > 1 Then
                Throw New SyntaxErrorException
            Else
                Return buf(Scan0)
            End If
        End Function
    End Class
End Namespace
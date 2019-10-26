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
                Return blocks(Scan0).DoCall(AddressOf ParseBinaryExpression)
            ElseIf blocks(1)(Scan0).text = ":" Then
                ' is a sequence generator syntax
                Return New SequenceLiteral(blocks(Scan0), blocks(2), blocks.ElementAtOrDefault(4))
            Else
                Throw New NotImplementedException
            End If
        End Function

        Private Shared Function ParseBinaryExpression(tokens As Token()) As Expression
            Dim blocks As List(Of Token()) = tokens.SplitByTopLevelDelimiter(TokenType.operator)

            If blocks = 1 Then
                ' 简单的表达式
                If tokens(Scan0).name = TokenType.identifier AndAlso tokens(1).name = TokenType.open Then
                    Return New FunctionInvoke(tokens)
                ElseIf tokens(Scan0).name = TokenType.open Then
                    Dim openSymbol = tokens(Scan0).text

                    If openSymbol = "[" Then
                        Return New VectorLiteral(tokens)
                    Else
                        Throw New NotImplementedException
                    End If
                End If
            Else

            End If

            Throw New NotImplementedException
        End Function
    End Class
End Namespace
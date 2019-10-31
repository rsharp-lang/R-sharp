Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
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

        Friend Shared ReadOnly literalTypes As Index(Of TokenType) = {
            TokenType.stringLiteral,
            TokenType.booleanLiteral,
            TokenType.integerLiteral,
            TokenType.numberLiteral,
            TokenType.missingLiteral
        }

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateExpression(code As IEnumerable(Of Token)) As Expression
            Return code _
                .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True) _
                .DoCall(AddressOf ParseExpression)
        End Function

        Friend Shared Function ParseExpression(code As List(Of Token())) As Expression
            If code(Scan0).isKeyword Then
                Dim keyword As String = code(Scan0)(Scan0).text

                Select Case keyword
                    Case "let"
                        If code > 4 AndAlso code(3).isKeyword("function") Then
                            Return New DeclareNewFunction(code)
                        Else
                            Return New DeclareNewVariable(code)
                        End If
                    Case "if" : Return New IfBranch(code.Skip(1).IteratesALL)
                    Case "else" : Return New ElseBranch(code.Skip(1).IteratesALL.ToArray)
                    Case "elseif" : Return New ElseIfBranch(code.Skip(1).IteratesALL)
                    Case "return" : Return New ReturnValue(code.Skip(1).IteratesALL)
                    Case "for" : Return New ForLoop(code.Skip(1).IteratesALL)
                    Case Else
                        Throw New SyntaxErrorException
                End Select
            ElseIf code.Count = 1 Then
                Dim item As Token() = code(Scan0)

                If item.isLiteral Then
                    Return New Literal(item(Scan0))
                ElseIf item.isIdentifier Then
                    Return New SymbolReference(item(Scan0))
                Else
                    Return code(Scan0).CreateTree
                End If
            ElseIf code > 2 AndAlso (code(Scan0).isIdentifier OrElse code(Scan0).isTuple) AndAlso code(1).isOperator("->", "=>") Then
                ' is a lambda function
                Return New DeclareLambdaFunction(code)
            End If

            If code(Scan0).isIdentifier Then
                If code(1).isOperator Then
                    Dim opText$ = code(1)(Scan0).text

                    If opText = "=" OrElse opText = "<-" Then
                        Return New ValueAssign(code)
                    End If
                End If
            End If

            Return code.ParseBinaryExpression
        End Function
    End Class
End Namespace
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
            TokenType.numberLiteral
        }

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateExpression(code As Token()) As Expression
            Return code _
                .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True) _
                .DoCall(AddressOf CreateExpression)
        End Function

        Friend Shared Function CreateExpression(code As List(Of Token())) As Expression
            If code(Scan0).isKeyword Then
                Dim keyword As String = code(Scan0)(Scan0).text

                Select Case keyword
                    Case "let" : Return New DeclareNewVariable(code)
                    Case "if" : Return New IfBranch(code)

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
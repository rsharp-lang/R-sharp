Imports Microsoft.VisualBasic.ComponentModel.Collection
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
            TokenType.stringLiteral
        }

        Public Shared Function CreateExpression(code As Token()) As Expression
            If code(Scan0).name = TokenType.keyword Then
                Dim keyword As String = code(Scan0).text

                Select Case keyword
                    Case "let" : Return New DeclareNewVariable(code)
                    Case Else
                        Throw New SyntaxErrorException
                End Select
            ElseIf code.Length = 1 AndAlso code(Scan0).name Like literalTypes Then
                Return New Literal(code(Scan0))
            ElseIf code(Scan0).name = TokenType.open Then
                Dim openSymbol = code(Scan0).text

                If openSymbol = "[" Then
                    Return code.Skip(1) _
                        .Take(code.Length - 2) _
                        .ToArray _
                        .DoCall(Function(v) New VectorLiteral(v))
                Else
                    Throw New NotImplementedException
                End If
            End If

            Throw New NotImplementedException
        End Function
    End Class
End Namespace
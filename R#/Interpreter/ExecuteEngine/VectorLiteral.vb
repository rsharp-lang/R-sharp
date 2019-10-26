Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Interpreter.ExecuteEngine

    Public Class VectorLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly values As Expression()

        Sub New(tokens As Token())
            Dim blocks = tokens.Skip(1) _
                .Take(tokens.Length - 2) _
                .ToArray _
                .SplitByTopLevelDelimiter(TokenType.comma)
            Dim values As New List(Of Expression)

            For Each block As Token() In blocks
                If Not (block.Length = 1 AndAlso block(Scan0).name = TokenType.comma) Then
                    Call values.Add(block.DoCall(AddressOf Expression.CreateExpression))
                End If
            Next

            ' 还会剩余最后一个元素
            ' 所以在这里需要加上
            Me.values = values
            Me.type = values _
                .GroupBy(Function(exp) exp.type) _
                .OrderByDescending(Function(g) g.Count) _
                .FirstOrDefault _
               ?.Key
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return values _
                .Select(Function(exp) exp.Evaluate(envir)) _
                .ToArray
        End Function
    End Class
End Namespace
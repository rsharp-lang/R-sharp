Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class VectorLiteral : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly values As Expression()

        Sub New(tokens As Token())
            Dim blocks As List(Of Token()) = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
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
            Dim vector = values _
                .Select(Function(exp) exp.Evaluate(envir)) _
                .ToArray
            Dim result As Array = Environment.asRVector(type, vector)

            Return result
        End Function

        Public Function ToArray() As Expression()
            Return values.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"[{values.JoinBy(", ")}]"
        End Function
    End Class
End Namespace
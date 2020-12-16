Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File.Expressions

    Public Class RBinary : Inherits RExpression

        Public Property [operator] As String
        Public Property left As RExpression
        Public Property right As RExpression

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Return New BinaryExpression(left.GetExpression(desc), right.GetExpression(desc), [operator])
        End Function

        Public Shared Function FromBinarySymbol(bin As BinaryExpression) As RExpression
            Dim leftVal As RExpression = RExpression.CreateFromSymbolExpression(bin.left)
            Dim rightVal As RExpression = RExpression.CreateFromSymbolExpression(bin.right)

            If TypeOf leftVal Is ParserError Then
                Return leftVal
            ElseIf TypeOf rightVal Is ParserError Then
                Return rightVal
            End If

            Return New RBinary With {
                .left = leftVal,
                .[operator] = bin.operator,
                .right = rightVal
            }
        End Function
    End Class
End Namespace
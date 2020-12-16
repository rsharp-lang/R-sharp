Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File.Expressions

    Public Class RBinary : Inherits RExpression

        Public Property [operator] As String
        Public Property left As JSONNode
        Public Property right As JSONNode

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Dim leftVal = left.GetExpression(desc)
            Dim rightVal = right.GetExpression(desc)

            If [operator] = "||" Then
                Return New BinaryOrExpression(leftVal, rightVal)
            ElseIf [operator] = "in" Then
                Return New BinaryInExpression(leftVal, rightVal)
            ElseIf [operator] = "between" Then
                Return New BinaryBetweenExpression(leftVal, rightVal)
            ElseIf [operator] = "<<" Then
                Return New AppendOperator(leftVal, rightVal)
            Else
                Return New BinaryExpression(leftVal, rightVal, [operator])
            End If
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
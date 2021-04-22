Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Interpreter.ExecuteEngine.LINQ

    Public MustInherit Class PipelineKeyword : Inherits LinqKeywordExpression

        Public MustOverride Overloads Function Exec(result As IEnumerable(Of JavaScriptObject), context As ExecutableContext) As IEnumerable(Of JavaScriptObject)

        ''' <summary>
        ''' 将字符串常量表示转换为变量引用
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <returns></returns>
        Protected Shared Function FixLiteral(expr As Expression) As Expression
            If TypeOf expr Is BinaryExpression Then
                Dim bin As BinaryExpression = DirectCast(expr, BinaryExpression)

                bin.left = FixLiteral(bin.left)
                bin.right = FixLiteral(bin.right)
            ElseIf TypeOf expr Is Literal Then
                expr = New SymbolReference(DirectCast(expr, Literal).ValueStr)
            ElseIf TypeOf expr Is FunctionInvoke Then
                DirectCast(expr, FunctionInvoke).parameters = DirectCast(expr, FunctionInvoke).parameters _
                    .Select(AddressOf FixLiteral) _
                    .ToArray
            End If

            Return expr
        End Function
    End Class
End Namespace
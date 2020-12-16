Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace System.Package.File.Expressions

    Public Class RVector : Inherits RExpression

        Public Property elements As JSONNode()

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function

        Public Shared Function FromVector(vec As VectorLiteral) As RExpression
            Return New RVector With {
                .elements = vec _
                    .Select(Function(a) CType(RExpression.CreateFromSymbolExpression(a), JSONNode)) _
                    .ToArray
            }
        End Function
    End Class
End Namespace
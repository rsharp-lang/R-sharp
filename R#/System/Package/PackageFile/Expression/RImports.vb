Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public Class RImports : Inherits RExpression

        Public Property packages As String()
        Public Property [module] As String

        Public Overrides Function GetExpression() As Expression
            Dim names As Expression() = packages _
                .Select(Function(name) New Literal(name)) _
                .ToArray

            If [module].StringEmpty Then
                ' require
                Return New Require(names)
            Else
                Return New [Imports](New VectorLiteral(names), New Literal([module]))
            End If
        End Function

        Public Shared Function GetRImports(require As Require) As [Variant](Of RImports, Message)

        End Function
    End Class
End Namespace
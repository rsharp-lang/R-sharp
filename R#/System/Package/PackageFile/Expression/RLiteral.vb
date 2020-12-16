Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public Class RLiteral : Inherits RExpression

        Public Property value As String
        Public Property type As TypeCodes

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Select Case type
                Case TypeCodes.boolean
                    Return New Literal(value.ParseBoolean)
                Case TypeCodes.double
                    Return New Literal(value.ParseDouble)
                Case TypeCodes.integer
                    Return New Literal(value.ParseInteger)
                Case Else
                    Return New Literal(value)
            End Select
        End Function

        Public Shared Function FromLiteral(x As Literal) As RExpression
            Return New RLiteral With {
                .type = x.type,
                .value = x.value.ToString
            }
        End Function
    End Class
End Namespace
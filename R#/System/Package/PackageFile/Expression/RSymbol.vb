Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public Class RSymbol : Inherits RExpression

        Public Property name As String
        Public Property type As TypeCodes
        Public Property value As String
        Public Property [readonly] As Boolean

        Public Overrides Function GetExpression() As Expression

        End Function
    End Class
End Namespace
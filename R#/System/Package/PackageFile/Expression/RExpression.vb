Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public MustInherit Class RExpression

        Public MustOverride Function GetExpression() As Expression

        Public Shared Function CreateFromSymbol(symbol As Symbol) As RExpression

        End Function
    End Class

    Public Class RImports : Inherits RExpression

        Public Property packages As String()
        Public Property [module] As String

        Public Overrides Function GetExpression() As Expression

        End Function
    End Class

End Namespace
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public Class RSymbol : Inherits RExpression
        Implements INamedValue

        Public Property name As String Implements INamedValue.Key
        Public Property type As TypeCodes
        Public Property value As RExpression
        Public Property [readonly] As Boolean

        Public Overrides Function GetExpression() As Expression

        End Function

        Public Shared Function GetSymbol(x As DeclareNewSymbol) As RExpression

        End Function
    End Class
End Namespace
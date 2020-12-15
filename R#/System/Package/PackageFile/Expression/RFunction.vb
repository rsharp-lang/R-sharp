Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace System.Package.File.Expressions

    Public Class RFunction : Inherits RExpression
        Implements INamedValue

        Public Property name As String Implements INamedValue.Key

        Public Overrides Function GetExpression() As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
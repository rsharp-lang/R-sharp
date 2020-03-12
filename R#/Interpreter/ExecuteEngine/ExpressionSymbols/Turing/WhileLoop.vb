Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class WhileLoop : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace